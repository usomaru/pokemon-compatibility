using Dapper;
using PokemonCompatibilityAPI.Model;
using System.Data.SqlClient;

namespace PokemonCompatibilityAPI.Data
{
    public class TypeCompatibilityRepository : ITypeCompatibilityRepository
    {
        private readonly string _connectionString;
       
        public TypeCompatibilityRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<TypeCompatibilityModel>> GetTypeCompatibility(string typeName)
        {
            using var connection = new SqlConnection(_connectionString);
                var items=  await connection.QueryAsync<TypeCompatibilityModel>(
                    "[dbo].[GetTypeCompatibility]",
                    new
                    {
                        TypeName = typeName
                    },
                    commandType: System.Data.CommandType.StoredProcedure
                    );
            return items;
        }
    }
}
