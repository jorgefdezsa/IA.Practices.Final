namespace CQRS.Infrastructure.Redis
{
    using CQRS.Shared.Models;
    using StackExchange.Redis;
    using System.Text.Json;

    public interface IPromptReadModelRepository
    {
        Task SaveAsync(PromptReadModel user);
        Task<PromptReadModel?> GetByIdAsync(Guid id);
    }

    public class RedisPromptReadModelRepository : IPromptReadModelRepository
    {
        private readonly IDatabase _db;

        public RedisPromptReadModelRepository(IConnectionMultiplexer redis) => _db = redis.GetDatabase();

        private static string GetKey(Guid id) => $"user:{id}";

        public async Task SaveAsync(PromptReadModel user)
        {
            var json = JsonSerializer.Serialize(user);
            await _db.StringSetAsync(GetKey(user.Id), json);
        }

        public async Task<PromptReadModel?> GetByIdAsync(Guid id)
        {
            var json = await _db.StringGetAsync(GetKey(id));
            return json.IsNullOrEmpty ? null : JsonSerializer.Deserialize<PromptReadModel>(json!);
        }
    }
}
