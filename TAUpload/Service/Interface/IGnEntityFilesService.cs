using TAUpload.Models;

namespace TAUpload.Service.Interface
{
    public interface IGnEntityFilesService
    {
        Task<bool> FileExistInDB(DownloadDTO dto);
        Task<int> SaveDB(DownloadDTO dto);
        void UpdateTeurAndFileType(DownloadDTO dto);
        void DeleteFileFromDB(DownloadDTO dto);
        void DeleteAllFiles(DownloadDTO dto);
        void DeleteLocalFile(DownloadDTO dto);
        Task<int> SaveLocalFile(DownloadDTO dto);
        Task<int> SaveFileWithWatermark(string directory, string filename);
    }
}
