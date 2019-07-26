using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using studo.Models.Options;
using studo.Services.Configure;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace studo.Controllers.Logs
{
    [Produces("text/plain")]
    [Route("api/logs")]
    public class LogsController : Controller
    {
        private readonly LogsOptions options;
        public LogsController(IOptions<LogsOptions> options)
        {
            this.options = options.Value;
        }

        [Authorize(Roles = RolesConstants.Admin)]
        [HttpGet("{dateTime:datetime}")]
        public async Task<IActionResult> GetFileWithLogs(DateTime dateTime)
        {
            string normalizedDate = dateTime.ToString("yyyyMM");
            var path = Path.Combine(Directory.GetCurrentDirectory(), options.PathToFolder, $"{options.LogsFilesName}{normalizedDate}{options.LogsFilesExtensions}");

            if (!System.IO.File.Exists(path))
                return NotFound(dateTime);

            var temp = new MemoryStream();

            using (var sourceStream = System.IO.File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var provider = new AesCryptoServiceProvider())
            using (var cryptoTransform = provider.CreateEncryptor())
            using (var cryptoStream = new CryptoStream(temp, cryptoTransform, CryptoStreamMode.Write, true))
            {
                temp.Write(provider.IV, 0, provider.IV.Length);
                await sourceStream.CopyToAsync(cryptoStream);
            }
            temp.Position = 0;
            return File(temp, "text/plain", "");
        }
    }
}
