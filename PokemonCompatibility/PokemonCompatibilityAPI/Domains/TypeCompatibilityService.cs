using PokemonCompatibilityAPI.Data;
using PokemonCompatibilityAPI.Model;

namespace PokemonCompatibilityAPI.Domains
{
    public class TypeCompatibilityService : ITypeCompatibilityService
    {
        private readonly ITypeCompatibilityRepository _typeCompatibilityRepository;
        public TypeCompatibilityService(ITypeCompatibilityRepository typeCompatibilityRepository)
        {
            _typeCompatibilityRepository = typeCompatibilityRepository;
        }

        public async Task<TypeCompatibilityResponse> GetTypeCompatibility(string typeName)
        {
            var typeList = await _typeCompatibilityRepository.GetTypeCompatibility(typeName);
            var strongType = typeList.Where(x => x.Power == (decimal)2.0).Select(x => x.CompatibilityType);
            var weakType = typeList.Where(x => x.Power == (decimal)0.5).Select(x => x.CompatibilityType);
            return new TypeCompatibilityResponse
            {
                StrongType = strongType.ToArray(),
                WeakType = weakType.ToArray()
            };
        }
    }
}
