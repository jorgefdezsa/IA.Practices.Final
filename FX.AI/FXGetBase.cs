using CQRS.Infrastructure.Redis;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FX.AI;

public class FXGetBase
{
    private readonly ILogger<FXGetBase> _logger;
    private readonly IPromptReadModelRepository _redisRepo;

    public FXGetBase(IPromptReadModelRepository redisRepo, ILogger<FXGetBase> logger)
    {
        _redisRepo = redisRepo; 
        _logger = logger;
    }

    [Function("FXGetBase")]
    public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        string? idQuery = req.Query["id"];

        if (!Guid.TryParse(idQuery, out Guid id))
        {
            return new BadRequestObjectResult("Invalid or missing 'id' parameter.");
        }

        var prompt = await _redisRepo.GetByIdAsync(id);

        return new OkObjectResult(prompt);
    }
}