namespace CQRS.Infrastructure.SQL
{
    using CQRS.Shared.Models;
    using Dapper;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Configuration;
    using System.Data;

    public class SqlPromptReadModelRepository : ISqlPromptModelRepository
    {
        private readonly IDbConnection _db;

        public SqlPromptReadModelRepository(IConfiguration config)
        {
            _db = new SqlConnection(config.GetConnectionString("SqlServer"));
        }

        public async Task SaveAsync(PromptReadModel model)
        {
            const string sql = @"
            INSERT INTO PromptReadModel (Id, Prompt, ImageBase64)
            VALUES (@Id, @Prompt, @ImageBase64);
        ";

            await _db.ExecuteAsync(sql, new
            {
                model.Id,
                model.Prompt,
                model.ImageBase64
            });
        }
    }
}
