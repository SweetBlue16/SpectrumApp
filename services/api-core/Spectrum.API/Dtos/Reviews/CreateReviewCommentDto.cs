using System.ComponentModel.DataAnnotations;

namespace Spectrum.API.Dtos.Reviews
{
    public class CreateReviewCommentDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "El comentario es obligatorio.")]
        [MaxLength(1000, ErrorMessage = "El comentario no puede superar los 1000 caracteres.")]
        public string Content { get; set; } = string.Empty;
    }
}
