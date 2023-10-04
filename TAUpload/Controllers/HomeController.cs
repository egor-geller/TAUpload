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
using System;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using TAUpload.Utils;

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

        [HttpDelete]
        public async Task<ActionResult<string>> DeleteFile([FromForm] DeleteDto dto)
        {
            await Task.Run(() =>
            {
                gnEntityFilesService.DeleteFileFromDB(dto);
                gnEntityFilesService.DeleteLocalFile(dto);
            });

            return Ok();
        }

        [HttpPost("/xls")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<string>> UploadXlsFile([FromForm] ExcelDto dto)
        {
            logger.Info($"TAUpload:UploadXlsFile:Start UploadXlsFile");
            if (dto.Files == null || dto.Files.Count <= 0)
            {
                logger.Info($"TAUpload:UploadXlsFile:The received stream is null! Aborting...");
                return NotFound("File not found");
            }

            string columns = "ABCDEFGHIJKLMNOPQRSTUVWX";
            // Check for headers
            string[] currentCellValue = new string[columns.Length];
            JsonItem excelData = await JsonFileReader.ReadAsync<JsonItem>(@"excelColums.json"); 
            foreach (var file in dto.Files)
            {
                if (file.Length > 0)
                {
                    string fileExt = System.IO.Path.GetExtension(file.FileName).ToLower();
                    string path = file.FileName;
                    using (var filestream = System.IO.File.Create(path))
                    {
                        await file.CopyToAsync(filestream);
                        logger.Info($"TAUpload:UploadXlsFile: File {file.FileName} has been saved");

                        for (int i = 0; i < columns.Length; i++)
                        {
                            currentCellValue[i] = (await GetCellValue(filestream, excelData.SheetName, columns[i] + "1")).Result.ToString();
                        }
                    }

                    for (int i = 0; i < excelData.Columns.Length; i++)
                    {
                        if (!excelData.Columns[i].Equals(currentCellValue[i]))
                        {
                            System.IO.File.Delete(path);
                            logger.Info($"TAUpload:UploadXlsFile: File {file.FileName} has been deleted");
                            return BadRequest("Content of file is not matching the template");
                        }
                    }
                }
            }

            return Ok();
        }

        // Retrieve the value of a cell, given a file name, sheet name, 
        // and address name.
        private async Task<ActionResult<string>> GetCellValue(Stream fileName, string sheetName, string addressName)
        {
            string value = string.Empty;

            try
            {
                // Open the spreadsheet document for read-only access.
                using (var document = SpreadsheetDocument.Open(fileName, false))
                {
                    if (document.WorkbookPart == null)
                    {
                        return NotFound("File 'document.WorkbookPart' not found");
                    }
                    // Retrieve a reference to the workbook part.
                    WorkbookPart wbPart = document.WorkbookPart;

                    // Find the sheet with the supplied name, and then use that 
                    // Sheet object to retrieve a reference to the first worksheet.
                    Sheet theSheet = wbPart.Workbook.Descendants<Sheet>()
                                                    .First(s => s.Name == sheetName);

                    // Retrieve a reference to the worksheet part.
                    if (wbPart.GetPartById(theSheet.Id) is not WorksheetPart wsPart)
                    {
                        return NotFound("Sheet id not found");
                    }

                    await Task.Run(() =>
                    {
                        // Use its Worksheet property to get a reference to the cell 
                        // whose address matches the address you supplied.
                        Cell theCell = wsPart.Worksheet.Descendants<Cell>()
                                                       .First(c => c.CellReference == addressName);

                        // If the cell does not exist, return an empty string.
                        if (theCell.InnerText.Length > 0)
                        {
                            value = theCell.InnerText;

                            // If the cell represents an integer number, you are done. 
                            // For dates, this code returns the serialized value that 
                            // represents the date. The code handles strings and 
                            // Booleans individually. For shared strings, the code 
                            // looks up the corresponding value in the shared string 
                            // table. For Booleans, the code converts the value into 
                            // the words TRUE or FALSE.
                            if (theCell.DataType != null)
                            {
                                switch (theCell.DataType.Value)
                                {
                                    case CellValues.SharedString:

                                        // For shared strings, look up the value in the
                                        // shared strings table.
                                        var stringTable =
                                            wbPart.GetPartsOfType<SharedStringTablePart>()
                                            .FirstOrDefault();

                                        // If the shared string table is missing, something 
                                        // is wrong. Return the index that is in
                                        // the cell. Otherwise, look up the correct text in 
                                        // the table.
                                        if (stringTable != null)
                                        {
                                            value =
                                                stringTable.SharedStringTable
                                                .ElementAt(int.Parse(value)).InnerText;
                                        }
                                        break;

                                    case CellValues.Boolean:
                                        value = value switch
                                        {
                                            "0" => "FALSE",
                                            _ => "TRUE",
                                        };
                                        break;
                                }
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }

            return value;
        }
    }
}