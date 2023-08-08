using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TAUpload.Models
{
    public sealed class WatermarkDto
    {
        [Required]
        public string? Directory { get; set; }
        [Required]
        public string? Filename { get; set; }

        public override string? ToString()
        {
            return new StringBuilder()
                .Append("Directory: ").Append(Directory).Append('\n')
                .Append("Filename: ").Append(Filename).Append('\n')
                .ToString();
        }
    }
}
