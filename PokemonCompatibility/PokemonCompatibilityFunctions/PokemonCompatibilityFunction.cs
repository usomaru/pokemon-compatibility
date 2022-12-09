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
        private static string IntroductionMessage = "おしえてほしいタイプをいってね。";
        private static string ErrorMessage { get; } = "すみません、わかりませんでした！";
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
                // インテント使って返事されるやつ
                case IntentRequest ir:
                    {
                        // これでタイプ名が取得できるはず
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
                        Text = "すみません。わかりません",
                    };
                    break;
            }
            return new OkObjectResult(skillResponse);
            
        }

        //インテントの処理を記述
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
                                var responseText = "こうかばつぐんのタイプは" + string.Join(",", compatibilityResponse.StrongType) + "です。いまひとつは" + string.Join(",", compatibilityResponse.WeakType) + "です";
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
                        ResponseMessage = "また明日",
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
