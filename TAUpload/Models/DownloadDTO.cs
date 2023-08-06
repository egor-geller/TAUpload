using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TAUpload.Models
{
    public sealed class DownloadDTO
    {
        public string FileTypeCd { get; set; } = string.Empty;
        public string Teur { get; set; } = string.Empty;
        [Required]
        public string EntityKey { get; set; } = string.Empty;
        public string EntityOnly { get; set; } = string.Empty;
        [Required]
        public string? FileName { get; set; }
        [Required]
        public string? Overwrite { get; set; }
        [Required]
        public string PathName { get; set; } = string.Empty;
        [Required]
        public string DirName { get; set; } = string.Empty;
        [Required]
        public string? ServerAlias { get; set; }
        [Required]
        public string? SqlServerName { get; set; }
        [Required]
        public string? SqlDbName { get; set; }
        [Required]
        public string? ObjectType { get; set; }
        [Required]
        public string? LoadUserID { get; set; }
        [Required]
        public string? DbId { get; set; }
        public bool Watermark { get; set; } = false;
        [Required]
        [FromForm]
        public List<IFormFile> Files { get; set; } = new List<IFormFile>();

        public override string? ToString()
        {
            var builder = new StringBuilder();
            return builder
                .Append("DbId: ").Append(DbId).Append('\n')
                .Append("LoadUserID: ").Append(LoadUserID).Append('\n')
                .Append("ObjectType: ").Append(ObjectType).Append('\n')
                .Append("SqlDbName: ").Append(SqlDbName).Append('\n')
                .Append("SqlServerName: ").Append(SqlServerName).Append('\n')
                .Append("ServerAlias: ").Append(ServerAlias).Append('\n')
                .Append("DirName: ").Append(DirName).Append('\n')
                .Append("PathName: ").Append(PathName).Append('\n')
                .Append("Overwrite: ").Append(Overwrite).Append('\n')
                .Append("FileName: ").Append(FileName).Append('\n')
                .Append("EntityOnly: ").Append(EntityOnly).Append('\n')
                .Append("EntityKey: ").Append(EntityKey).Append('\n')
                .Append("Teur: ").Append(Teur).Append('\n')
                .Append("FileTypeCd: ").Append(FileTypeCd).Append('\n')
                .Append("Watermark: ").Append(Watermark).Append('\n')
                .ToString();
        }
    }
}
