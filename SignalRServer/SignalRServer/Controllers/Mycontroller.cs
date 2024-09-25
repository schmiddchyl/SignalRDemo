using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace SignalRServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MyController : ControllerBase
    {
        private readonly IHubContext<FileHub> _hubContext;

        public MyController(IHubContext<FileHub> hubContext)
        {
            _hubContext = hubContext;
        }

        // Example endpoint for testing
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok("Pong!");
        }

        // Example method to invoke a SignalR hub method (like file upload initiation)
        [HttpPost("start-upload")]
        public async Task<IActionResult> StartFileUpload(string fileName)
        {
            // You can invoke SignalR methods from the controller
            await _hubContext.Clients.All.SendAsync("NotifyStartUpload", fileName);

            return Ok(new { message = $"Started file upload for {fileName}" });
        }

    }
}