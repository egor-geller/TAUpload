using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TAUpload.Models
{
    public sealed class DeleteDto
    {
        [Required]
        public string? DbId { get; set; }
        [Required]
        public string? SqlServerName { get; set; }
        [Required]
        public string? SqlDbName { get; set; }
        [Required]
        public string? ObjectType { get; set; }
        [Required]
        public string PathName { get; set; } = string.Empty;
        [Required]
        public string? FileName { get; set; }
        public string EntityOnly { get; set; } = string.Empty;
        [Required]
        public string EntityKey { get; set; } = string.Empty;

        public override string? ToString()
        {
            return new StringBuilder()
                .Append("DbId: ").Append(DbId).Append('\n')
                .Append("ObjectType: ").Append(ObjectType).Append('\n')
                .Append("SqlDbName: ").Append(SqlDbName).Append('\n')
                .Append("SqlServerName: ").Append(SqlServerName).Append('\n')
                .Append("FileName: ").Append(FileName).Append('\n')
                .Append("PathName: ").Append(PathName).Append('\n')
                .Append("EntityOnly: ").Append(EntityOnly).Append('\n')
                .Append("EntityKey: ").Append(EntityKey).Append('\n')
                .ToString();
        }
    }
}
