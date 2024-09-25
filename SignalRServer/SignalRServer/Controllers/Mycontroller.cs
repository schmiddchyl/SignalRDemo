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

        // Example of receiving data via HTTP
        [HttpPost("upload-file")]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            // You can process the file and send progress updates via SignalR here.
            await _hubContext.Clients.All.SendAsync("FileUploadProgress", 0);

            // Example of saving the file
            var filePath = Path.Combine("UploadedFiles", file.FileName);
            using (var stream = System.IO.File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }

            await _hubContext.Clients.All.SendAsync("FileUploadComplete", file.FileName);

            return Ok(new { message = $"File {file.FileName} uploaded successfully" });
        }
    }
}