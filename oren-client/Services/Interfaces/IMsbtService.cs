using Microsoft.AspNetCore.Http;
using oren_client.Models;

namespace oren_client.Services.Interfaces;

public interface IMsbtService
{
    Task<List<string>> MsbtToStringList(IFormFile msbtFile);

    Task<List<MsbtSheet>> ZipToSheets(IFormFile zipWithMsbts);
}