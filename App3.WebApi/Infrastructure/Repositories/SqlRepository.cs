using System.Data.SqlClient;
using System.Threading.Tasks;
using App3.WebApi.Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace App3.WebApi.Infrastructure.Repositories
{
    public class SqlRepository : ISqlRepository
    {
        private readonly IConfiguration _configuration;
        private const string Query = "SELECT GETDATE()";

        public SqlRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task Persist(string message)
        {
            await using var conn = new SqlConnection(_configuration["SqlDbConnString"]);
            await conn.OpenAsync();

            //Do something more complex
            await using var cmd = new SqlCommand(Query, conn);
            var res = await cmd.ExecuteScalarAsync();
        }
    }
}
