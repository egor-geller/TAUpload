using TAUpload.Models;

namespace TAUpload.Service.Interface
{
    public interface IGnEntityFilesService
    {
        Task<bool> FileExistInDB(DownloadDTO dto);
        Task<int> SaveDB(DownloadDTO dto);
        void UpdateTeurAndFileType(DownloadDTO dto);
        void DeleteFileFromDB(DownloadDTO dto);
        void DeleteFileFromDB(DeleteDto dto);
        void DeleteAllFiles(DownloadDTO dto);
        void DeleteLocalFile(DownloadDTO dto);
        void DeleteLocalFile(DeleteDto dto);
        Task<int> SaveLocalFile(DownloadDTO dto);
    }
}
