using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PokemonCompatibilityAPI.Domains;
using PokemonCompatibilityAPI.Model;

namespace PokemonCompatibilityAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PokemonCompatibilityController : ControllerBase
    {
        private readonly ITypeCompatibilityService _typeCompatibilityService;
        public PokemonCompatibilityController(ITypeCompatibilityService typeCompatibilityService)
        {
            _typeCompatibilityService = typeCompatibilityService;
        }

        [HttpGet("{typeName}")]
        public async Task<TypeCompatibilityResponse> GetTypeCompatibilityAsync(string typeName)
        {
            return await _typeCompatibilityService.GetTypeCompatibility(typeName);
        }

    }
}
