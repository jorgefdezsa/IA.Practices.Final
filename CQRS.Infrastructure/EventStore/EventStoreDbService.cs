namespace CQRS.Infrastructure.EventStore
{
    using global::EventStore.Client;
    using System.Text.Json;

    public interface IEventStoreService
    {
        Task SaveAsync(string streamName, IEnumerable<object> events);
    }

    public class EventStoreDbService : IEventStoreService
    {
        private readonly EventStoreClient _client;

        public EventStoreDbService(EventStoreClient client) => _client = client;

        public async Task SaveAsync(string streamName, IEnumerable<object> events)
        {
            var eventData = events.Select(e => new EventData(
                Uuid.NewUuid(),
                e.GetType().Name,
                JsonSerializer.SerializeToUtf8Bytes(e)));

            await _client.AppendToStreamAsync(streamName, StreamState.Any, eventData);
        }
    }
}
