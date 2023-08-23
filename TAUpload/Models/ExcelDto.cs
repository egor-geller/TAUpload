using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace TAUpload.Models
{
    public sealed class ExcelDto
    {
        [Required]
        [FromForm]
        public List<IFormFile> Files { get; set; } = new List<IFormFile>();
    }
}
