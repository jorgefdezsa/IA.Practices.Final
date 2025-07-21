namespace CQRS.Domain.Events
{
    public record PromptRegisteredEvent(Guid Id, string Prompt);
}
