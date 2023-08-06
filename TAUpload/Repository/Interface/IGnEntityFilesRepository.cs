using TAUpload.Models;

namespace TAUpload.Repository.Interface
{
    public interface IGnEntityFilesRepository
    {
        string SelectTeur(DownloadDTO dto);
        bool FileExistInDB(DownloadDTO dto);
        void Save(DownloadDTO dto);
        void UpdateTeurAndFileType(DownloadDTO dto);
        void DeleteFileFromDB(DownloadDTO dto);
        void DeleteAllFiles(DownloadDTO dto);
    }
}
