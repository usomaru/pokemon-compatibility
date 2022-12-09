using PokemonCompatibilityAPI.Model;

namespace PokemonCompatibilityAPI.Domains
{
    public interface ITypeCompatibilityService
    {
        Task<TypeCompatibilityResponse> GetTypeCompatibility(string typeName);
    }
}