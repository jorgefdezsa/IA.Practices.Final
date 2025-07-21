using CQRS.Application.Commands;
using CQRS.Application.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FX.AI;

public class FXDalle
{
    private readonly ILogger<FXDalle> _logger;
    private readonly RegisterPromptHandler _registerPromptHandler;

    public FXDalle(RegisterPromptHandler registerPromptHandler, ILogger<FXDalle> logger)
    {
        _logger = logger;
        _registerPromptHandler = registerPromptHandler;
    }

    [Function("FXDalle")]
    public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        RegisterPromptCommand cmd;

        try
        {
            cmd = JsonSerializer.Deserialize<RegisterPromptCommand>(requestBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error deserializing request body.");
            return new BadRequestObjectResult("Invalid JSON.");
        }

        if (cmd == null)
        {
            return new BadRequestObjectResult("Request body is empty or invalid.");
        }

        await _registerPromptHandler.Handle(cmd);

        return new OkObjectResult($"{cmd.Id} Insertado correctamente.");
    }
}