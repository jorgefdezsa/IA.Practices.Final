using CQRS.Application.Handlers;
using CQRS.Infrastructure.EventStore;
using CQRS.Infrastructure.Redis;
using CQRS.Infrastructure.SQL;
using CQRS.Shared.Models;
using EventStore.Client;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Net.Http.Headers;

var builder = FunctionsApplication.CreateBuilder(args);

builder.Services.Configure<AzureOpenAIOptions>(
    builder.Configuration.GetSection("AzureOpenAI"));

builder.Services.AddHttpClient("AzureOpenAI", (serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<AzureOpenAIOptions>>().Value;

    client.BaseAddress = new Uri(options.Endpoint);
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);
});

builder.AddServiceDefaults();

builder.ConfigureFunctionsWebApplication();

//EventStoreDB
//builder.Services.AddSingleton(_ => new EventStoreClient(EventStoreClientSettings.Create("esdb://eventstore:2113?tls=false")));
builder.Services.AddSingleton(_ => new EventStoreClient(EventStoreClientSettings.Create("esdb://localhost:2113?tls=false")));
builder.Services.AddScoped<IEventStoreService, EventStoreDbService>();

//Redis
//builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("redis:6379"));
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("localhost:6379"));
builder.Services.AddScoped<IPromptReadModelRepository, RedisPromptReadModelRepository>();
builder.Services.AddScoped<RegisterPromptHandler>();
builder.Services.AddScoped<ISqlPromptModelRepository, SqlPromptReadModelRepository>();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Build().Run();
