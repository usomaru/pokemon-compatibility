using PokemonCompatibilityAPI.Model;

namespace PokemonCompatibilityAPI.Data
{
    public interface ITypeCompatibilityRepository
    {
        Task<IEnumerable<TypeCompatibilityModel>> GetTypeCompatibility(string typeName);
    }
}