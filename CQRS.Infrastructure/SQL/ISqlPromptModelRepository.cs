namespace CQRS.Infrastructure.SQL
{
    using CQRS.Shared.Models;
    public interface ISqlPromptModelRepository
    {
        Task SaveAsync(PromptReadModel model);
    }
}
