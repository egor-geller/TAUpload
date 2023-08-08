using iText.IO.Image;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using NLog;
using Spire.Doc;
using System.Data.Entity.Core;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Reflection;
using TAUpload.Models;
using TAUpload.Repository.Interface;
using TAUpload.Service.Interface;
using System.Resources;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using System.Text;
using System.Diagnostics.Metrics;
using System;

namespace TAUpload.Service
{
    [SupportedOSPlatform("windows")]
    public class GnEntityFilesService : IGnEntityFilesService
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly IGnEntityFilesRepository entityFilesRepository;
        public GnEntityFilesService(IGnEntityFilesRepository entityFilesRepository)
        {
            this.entityFilesRepository = entityFilesRepository;
        }

        public async void DeleteAllFiles(DownloadDTO dto)
        {
            await Task.Run(() => { entityFilesRepository.DeleteAllFiles(dto); });
        }

        public async void DeleteFileFromDB(DownloadDTO dto)
        {
            await Task.Run(() => { entityFilesRepository.DeleteFileFromDB(dto); });
        }
        public async void DeleteFileFromDB(DeleteDto dto)
        {
            await Task.Run(() => { entityFilesRepository.DeleteFileFromDB(dto); });
        }

        public async void DeleteLocalFile(DownloadDTO dto)
        {
            string fullPath = System.IO.Path.Combine(dto.DirName, dto.PathName);
            var d = new DirectoryInfo(fullPath);
            if (!d.Exists)
            {
                logger.Warn($"TAUpload:DeleteLocalFile: directory does not exists: {fullPath}");
            }
            FileInfo[] Files = d.GetFiles(dto.FileName?[..dto.FileName.LastIndexOf(".")] + "*"); //Getting files
            if (Files.Length >= 0)
            {
                logger.Info($"TAUpload:DeleteLocalFile: There is {Files.Length} to be deleted");
            }
            await Task.Run(() =>
            {
                foreach (FileInfo file in Files)
                {
                    logger.Info($"TAUpload:DeleteLocalFile: Removing the file: {file.Name}");
                    try
                    {
                        File.Delete(file.FullName);
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"TAUpload:DeleteLocalFile: Problem deleting a file {file.Name}, \n{ex}");
                    }
                }
            });
        }

        public async void DeleteLocalFile(DeleteDto dto)
        {
            var d = new DirectoryInfo(dto.PathName);
            if (!d.Exists)
            {
                logger.Warn($"TAUpload:DeleteLocalFile: directory does not exists: {dto.PathName}");
            }
            FileInfo[] Files = d.GetFiles(dto.EntityKey + "*"); //Getting files
            if (Files.Length >= 0)
            {
                logger.Info($"TAUpload:DeleteLocalFile: There is {Files.Length} to be deleted");
            }
            await Task.Run(() =>
            {
                foreach (FileInfo file in Files)
                {
                    logger.Info($"TAUpload:DeleteLocalFile: Removing the file: {file.Name}");
                    try
                    {
                        File.Delete(file.FullName);
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"TAUpload:DeleteLocalFile: Problem deleting a file {file.Name}, \n{ex}");
                    }
                }
            });
        }

        private static string GetFullFileName(string entityOnly, string entityKey, string fullpath, string fileName)
        {
            string fullFileName;
            string path;
            if (entityOnly == "YES")
            {
                int numExt = (fileName.Length - fileName.LastIndexOf('.')) - 1;
                string fileExt = fileName.Substring(fileName.LastIndexOf('.'), numExt + 1);
                fullFileName = entityKey + fileExt;
                logger.Info($"path: {fullpath}, fullFileName: {fullFileName}");
                path = System.IO.Path.Combine(fullpath, fullFileName);
            }
            else
            {
                fullFileName = entityKey + '-' + fileName;
                logger.Info($"path: {fullpath}, fullFileName: {fullFileName}");
                path = System.IO.Path.Combine(fullpath, fullFileName);
            }
            return path;
        }

        public async Task<bool> FileExistInDB(DownloadDTO dto)
        {
            return await Task.Run(() =>
            {
                return entityFilesRepository.FileExistInDB(dto);
            });
        }

        public async Task<int> SaveDB(DownloadDTO dto)
        {
            await Task.Run(() =>
            {
                entityFilesRepository.Save(dto);
                if (!string.IsNullOrEmpty(dto.Teur) && !string.IsNullOrEmpty(dto.FileTypeCd))
                {
                    entityFilesRepository.UpdateTeurAndFileType(dto);
                    string tempTeur = entityFilesRepository.SelectTeur(dto);

                    if (!string.IsNullOrEmpty(tempTeur))
                    {
                        dto.Teur = RTL(tempTeur);
                        entityFilesRepository.UpdateTeurAndFileType(dto);
                    }
                }
            });
            return 200;
        }

        private static string RTL(string teur)
        {
            string[] newStr = teur.Split(' ');
            int count = 0;
            string num = "";

            if (!Regex.IsMatch(teur, @"[a-zA-Z]+"))
            {
                var finalStrings = new StringBuilder();
                foreach (var item in newStr)
                {
                    if (Regex.IsMatch(item, @"[\p{IsHebrew}]+[a-zA-Z0-9]+", RegexOptions.IgnoreCase) || Regex.IsMatch(item, @"[a-zA-Z0-9]+[\p{IsHebrew}]+", RegexOptions.IgnoreCase)) // ^[\u0590-\u05FF\u200f\u200e]+$ hebrew
                    {
                        int len = item.Length;
                        string[] temp = new string[len];
                        for (var i = 0; i < len; i++)
                        {
                            temp[len - i - 1] += item[i];

                        }
                        string joinString = string.Join("", temp.Reverse());
                        finalStrings.Append(joinString).Append(' ');

                    }
                    else if (Regex.IsMatch(item, @"^[\p{IsHebrew}]+$"))
                    {
                        finalStrings.Append(item).Append(' ');
                    }
                    else if (Regex.IsMatch(item, @"^[0-9]+$")) 
                    {
                        finalStrings.Append('‏').Append(item).Append(' ');
                    }
                    else
                    {
                        finalStrings.Append(item).Append(' ');

                    }
                }
                return finalStrings.ToString();
            }



            for (var i = 0; i < newStr.Length / 2; i++)
            {
                (newStr[newStr.Length - i - 1], newStr[i]) = (newStr[i], newStr[newStr.Length - i - 1]);
            }

            var finalString = new StringBuilder();
            foreach (var item in newStr)
            {
                if (Regex.IsMatch(item, @"[\p{IsHebrew}]+[a-zA-Z0-9]+", RegexOptions.IgnoreCase) || Regex.IsMatch(item, @"[a-zA-Z0-9]+[\p{IsHebrew}]+", RegexOptions.IgnoreCase)) // ^[\u0590-\u05FF\u200f\u200e]+$ hebrew
                {
                    int len = item.Length;
                    string[] temp = new string[len];
                    for (var i = 0; i < len; i++)
                    {
                        temp[len - i - 1] += item[i];
                    }
                    string joinString = string.Join("", temp.Reverse());
                    finalString.Append(joinString).Append(' ');

                }
                else if (Regex.IsMatch(item, @"^[\p{IsHebrew}]+$"))
                {
                    finalString.Append(item).Append(' ');
                }
                else
                {
                    finalString.Append(item).Append(' ');

                }
            }
            return finalString.ToString();
        }

        public async void UpdateTeurAndFileType(DownloadDTO dto)
        {
            await Task.Run(() => { entityFilesRepository.UpdateTeurAndFileType(dto); });
        }

        public async Task<int> SaveLocalFile(DownloadDTO dto)
        {
            var directory = new DirectoryInfo(System.IO.Path.Combine(dto.PathName, dto.DirName));
            if (!directory.Exists)
            {
                logger.Info($"TAUpload:UploadFile:directory does not exists: {directory.FullName}");
                // Create Directory
            }
            logger.Info($"TAUpload:UploadFile:FULLNAME: {directory.FullName}");

            long totalFilesSize = dto.Files.Sum(x => x.Length);
            foreach (var item in dto.Files)
            {
                logger.Info($"TAUpload:UploadFile:Files: {item.FileName}");
                if (item.Length > 0)
                {
                    try
                    {
                        var fileExt = System.IO.Path.GetExtension(item.FileName).ToLower();
                        string path = GetFullFileName(dto.EntityOnly, dto.EntityKey, directory.FullName, item.FileName);
                        using (var fileStream = System.IO.File.Create(path))
                        {
                            await item.CopyToAsync(fileStream);
                            logger.Info($"TAUpload:UploadFile: Created file {item.FileName}");
                        }
                        logger.Info($"TAUpload:UploadFile: File ext: {fileExt}");

                        if (dto.ObjectType != null && dto.ObjectType.ToLower().Equals(@"hzm"))
                        {
                            int statusCode = await SaveFileWithWatermark(path);
                            if (statusCode == 415)
                            {
                                return statusCode;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"TAUpload:UploadFile:ERROR {ex}");
                        return 500;
                    }
                }
            }
            return 200;
        }

        private static async Task<int> SaveFileWithWatermark(string path)
        {
            string fileExt = System.IO.Path.GetExtension(path).ToLower();
            return await Task.Run(() =>
            {
                switch (fileExt)
                {
                    case ".docx":
                    case ".doc":
                        ManipulateDocx(path);
                        break;
                    case ".pdf":
                        ManipulatePdf(path);
                        break;
                    default:
                        logger.Info($"TAUpload:UploadFile: Unsupported media type to manipulate {path}");
                        return 415;
                }
                return 200;
            });
        }

        private static async void ManipulateDocx(string filePath)
        {
            logger.Info($"TAUpload:UploadFile:ManipulateDocx: Starting watermark");
            var doc = new Spire.Doc.Document(filePath);
            InsertImageWatermark(doc);

            await Task.Run(() =>
            {
                doc.SaveToFile(filePath, Spire.Doc.FileFormat.Docx);
                logger.Info($"TAUpload:UploadFile:ManipulateDocx: Watermark saved");
            });
        }

        private static async void ManipulatePdf(string path)
        {
            logger.Info($"TAUpload:UploadFile:ManipulatePdf: Starting watermark");
            //var dest = System.IO.Path.Combine(path, "-watermark-" + filename + ".pdf");
            string ext = System.IO.Path.GetExtension(path);
            string dest = path[..path.LastIndexOf(".")] + "-watermark" + ext;
            logger.Info($"{dest} and {ext}");

            await Task.Run(() =>
            {
                using (var pdfDoc = new PdfDocument(new PdfReader(path), new PdfWriter(dest)))
                {
                    try
                    {
                        ImageData image = ImageDataFactory.Create(GetAndConvertResourceImageToByteArray());//ImageDataFactory.Create(@"C:\tmp\Upload\Sample-Watermark-Transparent.png");
                        iText.Layout.Element.Image imageModel = new iText.Layout.Element.Image(image);
                        AffineTransform at = AffineTransform.GetTranslateInstance(36, 300);
                        at.Concatenate(AffineTransform.GetScaleInstance(imageModel.GetImageScaledWidth(), imageModel.GetImageScaledHeight()));
                        var canvas = new PdfCanvas(pdfDoc.GetFirstPage());
                        float[] matrix = new float[6];
                        at.GetMatrix(matrix);
                        canvas.AddImageWithTransformationMatrix(image, matrix[0], matrix[1], matrix[2], matrix[3], matrix[4], matrix[5]);
                    }
                    catch (Exception e)
                    {
                        logger.Error($"TAUpload:UploadFile:ManipulatePdf: ERROR {e}");
                    }
                }
                logger.Info($"TAUpload:UploadFile:ManipulatePdf: Saved watermark");
            });

            /*await Task.Run(() =>
            {
                try
                {
                    System.IO.File.Delete(filePath);
                    logger.Info($"TAUpload:UploadFile:ManipulatePdf: File {filename} deleted");
                }
                catch (Exception e)
                {
                    logger.Error($"TAUpload:UploadFile:ManipulatePdf:ERROR deleting file {filePath}, \n\t{e}");
                }

                try
                {
                    var fileInfo = new FileInfo(dest);
                    if (fileInfo.Exists)
                    {
                        fileInfo.MoveTo(filePath);
                        logger.Info($"TAUpload:UploadFile:ManipulatePdf: Rename file to {filename}");
                    }
                }
                catch (Exception e)
                {
                    logger.Error($"TAUpload:UploadFile:ManipulatePdf:ERROR changing filename \n {dest} to {filePath}, \n\t{e}");
                }
            });*/
            logger.Info($"TAUpload:UploadFile:ManipulatePdf: Watermark saved");
        }


        private static void InsertImageWatermark(Spire.Doc.Document document)
        {
            try
            {
                var picture = new PictureWatermark();
                picture.Picture = System.Drawing.Image.FromStream(GetAndConvertResourceImageToStream()); // Image.FromFile support only on windows
                picture.Scaling = 250;
                picture.IsWashout = false;
                document.Watermark = picture;
                logger.Info($"TAUpload:UploadFile:ManDocx:InsertImageWatermark: Configured watermark");
            }
            catch (Exception e)
            {
                logger.Error($"TAUpload:UploadFile:ManipulatePdf:InsertImageWatermark:ERROR {e}");
            }
        }


        private static Stream GetAndConvertResourceImageToStream()
        {
            var rm = new ResourceManager("Resources", Assembly.GetExecutingAssembly());
            Bitmap bitmap = Properties.Resources.Sample_Watermark_Transparent;
            var memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, ImageFormat.Png);

            return memoryStream;
        }


        private static byte[] GetAndConvertResourceImageToByteArray()
        {
            var rm = new ResourceManager("Resources", Assembly.GetExecutingAssembly());
            Bitmap bitmap = Properties.Resources.Sample_Watermark_Transparent;
            var memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, ImageFormat.Png);

            return memoryStream.ToArray();
        }
    }
}
