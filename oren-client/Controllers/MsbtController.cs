using System.IO.Compression;
using Ionic.Zip;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using oren_client.Lib.Formats;
using oren_client.Services.Interfaces;

namespace msbtApi.Controllers;

[Route("api/msbt")]
[ApiController]
public class MsbtController : ControllerBase
{
    private readonly IMsbtService _msbtService;

    public MsbtController(IMsbtService msbtService)
    {
        _msbtService = msbtService;
    }
    
    [HttpPost("msbtToStringList")]
    public async Task<IActionResult> MsbtToStringList(IFormFile msbtFile)
    {
        var list = _msbtService.MsbtToStringList(msbtFile);
        
        return Ok(list);
    }

    [HttpPost("zipToSheets")]
    public async Task<IActionResult> ZipToSheets(IFormFile zipWithMsbts)
    {
        var sheets = _msbtService.ZipToSheets(zipWithMsbts);
        
        return Ok(sheets);
    }
}