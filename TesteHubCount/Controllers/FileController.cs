using Microsoft.AspNetCore.Mvc;
using TesteHubCount.ValueObjects;

namespace TesteHubCount.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FileController : ControllerBase
    {
        private readonly ILogger<FileController> _logger;
        private readonly IOrderService _orderService;
        public FileController(ILogger<FileController> logger, IOrderService fileService)
        {
            _logger = logger;
            _orderService = fileService;
        }
        [HttpPost("process-file")]
        public async Task<IActionResult> ProcessFile(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("Nenhum arquivo enviado");
                }

                var fileData = await _orderService.ProcessOrderFile(file);

                return Ok(fileData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar a planilha");
                return StatusCode(500, new { Error = "Erro ao processar a planilha", Details = ex.Message });
            }
        }
    }
}