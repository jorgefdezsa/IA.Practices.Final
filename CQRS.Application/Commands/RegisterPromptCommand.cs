namespace CQRS.Application.Commands
{
    public record RegisterPromptCommand(Guid Id, string Prompt);
}
