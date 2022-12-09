using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Alexa.NET.Request.Type;
using PokemonCompatibilityFunctions.Model;
using System.Net.Http;
using Microsoft.Extensions.Configuration;

namespace PokemonCompatibilityFunctions
{
    public class PokemonCompatibilityFunction
    {
        private static string IntroductionMessage = "�������Ăق����^�C�v�������ĂˁB";
        private static string ErrorMessage { get; } = "���݂܂���A�킩��܂���ł����I";
        private readonly string _pokeCompatibilityApi;

        public PokemonCompatibilityFunction(IConfiguration configuration)
        {
            _pokeCompatibilityApi = configuration["Url"];
        }

        [FunctionName("PokemonCompatibilityFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req)
        {         
            string requestJson = "";
            using (var sr = new StreamReader(req.Body))
            {
                requestJson = await sr.ReadToEndAsync();
            }
            SkillRequest skillRequest = JsonConvert.DeserializeObject<SkillRequest>(requestJson);
            var skillResponse = new SkillResponse
            {
                Version = "1.0",
                Response = new ResponseBody()
            };
            switch (skillRequest.Request)
            {
                case LaunchRequest lr:
                    skillResponse.Response.OutputSpeech = new PlainTextOutputSpeech
                    {
                        Text = IntroductionMessage,
                    };
                    break;
                // �C���e���g�g���ĕԎ��������
                case IntentRequest ir:
                    {
                        // ����Ń^�C�v�����擾�ł���͂�
                        var typeName = ir.Intent.Slots["type"].Value;
                        var r = await HandleIntent(ir.Intent.Name, typeName);
                        
                        skillResponse.Response.OutputSpeech = new PlainTextOutputSpeech
                        {
                            Text = r.ResponseMessage
                        };
                        skillResponse.Response.ShouldEndSession = r.ResponseFinishFlg;
                    }
                    break;

                default:
                    skillResponse.Response.OutputSpeech = new PlainTextOutputSpeech
                    {
                        Text = "���݂܂���B�킩��܂���",
                    };
                    break;
            }
            return new OkObjectResult(skillResponse);
            
        }

        //�C���e���g�̏������L�q
        private async Task<HandleIntentResult> HandleIntent(string intent, string typeName)
        {
            switch (intent)
            {
                case "PokemonCompatibility":
                    {
                        var baseUrl = _pokeCompatibilityApi;
                        var url = baseUrl + typeName;
                        try
                        {
                            using (var client = new HttpClient())
                            {
                                var response = await client.GetAsync(url);
                                var json = await response.Content.ReadAsStringAsync();
                                var compatibilityResponse = JsonConvert.DeserializeObject<TypeCompatibilityResponse>(json);
                                var responseText = "�������΂���̃^�C�v��" + string.Join(",", compatibilityResponse.StrongType) + "�ł��B���܂ЂƂ�" + string.Join(",", compatibilityResponse.WeakType) + "�ł�";
                                return new HandleIntentResult
                                {
                                    ResponseMessage = responseText,
                                    ResponseFinishFlg = false,
                                };
                            }                         
                        } catch (Exception e)
                        {
                            return new HandleIntentResult
                            {
                                ResponseMessage = e.Message,
                                ResponseFinishFlg = false,
                            };
                        }
                        
                    }
                case "AMAZON.StopIntent":
                    return new HandleIntentResult
                    {
                        ResponseMessage = "�܂�����",
                        ResponseFinishFlg = true,
                    };
                default:
                    return new HandleIntentResult
                    {
                        ResponseMessage = ErrorMessage,
                        ResponseFinishFlg = false,
                    };
            }
        }
    }
}
