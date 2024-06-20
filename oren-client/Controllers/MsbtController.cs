using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using oren_client.Lib.Formats;

namespace msbtApi.Controllers;

[Route("api/msbt")]
[ApiController]
public class MsbtController : ControllerBase
{
    /*private readonly IMsbtService _msbtService;

    public MsbtController(IMsbtService msbtService)
    {
        _msbtService = msbtService;
    }*/
    
    [HttpPost("msbtToStringList")]
    public async Task<IActionResult> MsbtToStringList(IFormFile msbtFile)
    {
        MSBT msbt = new(msbtFile.OpenReadStream(), msbtFile.FileName);
        
        return Ok(msbt.MessagesToStringList());
    }
}