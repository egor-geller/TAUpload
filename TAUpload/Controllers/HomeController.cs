using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using Microsoft.AspNetCore.Mvc;
using NLog;
using Spire.Doc;
using iText.Kernel.Events;
using iText.Layout.Properties;
using iText.Layout;
using iText.Kernel.Geom;
using System.Runtime.Versioning;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Resources;
using TAUpload.Models;
using TAUpload.Service.Interface;
using TAUpload.Service;

namespace TAUpload.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : Controller
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly IGnEntityFilesService gnEntityFilesService;

        public HomeController(IGnEntityFilesService gnEntityFilesService)
        {
            this.gnEntityFilesService = gnEntityFilesService;
        }

        [HttpGet]
        public string GetSome()
        {
            return "Hello World";
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<string>> UploadFile([FromForm] DownloadDTO dto)
        {
            logger.Info($"TAUpload:UploadFile:Start UploadFile");
            logger.Info($"Upload file: {dto}");
            if (dto.Files == null || dto.Files.Count <= 0)
            {
                logger.Info($"TAUpload:UploadFile:The received stream is null! Aborting...");
                return NotFound("File not found");
            }

            int saveToDb = await gnEntityFilesService.SaveDB(dto);
            if(saveToDb != 200)
            {
                return StatusCode(500, "Error saving to DB");
            }

            gnEntityFilesService.DeleteLocalFile(dto);

            int saveLocalFile = await gnEntityFilesService.SaveLocalFile(dto);
            if(saveLocalFile == 415)
            {
                return StatusCode(saveLocalFile, "Unsuported Media File");
            }
            else if (saveLocalFile == 500)
            {
                return Problem("There is a problem while saving file localy", null, 500, null, null);
            }

            logger.Info($"TAUpload:UploadFile:End function UploadFile");
            return Ok();
        }
    }
}