namespace API.IA.Controllers
{
    using CQRS.Application.Commands;
    using CQRS.Application.Handlers;
    using CQRS.Infrastructure.Redis;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/prompts")]
    public class DalleController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Post(RegisterPromptCommand cmd, [FromServices] RegisterPromptHandler handler)
        {
            await handler.Handle(cmd);
            return Ok();
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id, [FromServices] IPromptReadModelRepository repo)
        {
            var prompt = await repo.GetByIdAsync(id);
            return prompt is null ? NotFound() : Ok(prompt);
        }
    }
}
