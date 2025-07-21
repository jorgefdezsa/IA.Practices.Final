namespace CQRS.Domain.Aggregates
{
    using CQRS.Domain.Events;

    public class PromptAggregate
    {
        public Guid Id { get; private set; }
        public string Prompt { get; private set; } = string.Empty;
        public List<object> Events { get; } = new();

        public static PromptAggregate Register(Guid id, string prompt)
        {
            var evt = new PromptRegisteredEvent(id, prompt);
            var agg = new PromptAggregate();
            agg.Apply(evt);
            agg.Events.Add(evt);
            return agg;
        }

        private void Apply(PromptRegisteredEvent evt)
        {
            Id = evt.Id;
            Prompt = evt.Prompt;
        }
    }
}
