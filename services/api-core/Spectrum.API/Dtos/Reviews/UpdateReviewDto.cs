using System.ComponentModel.DataAnnotations;

namespace Spectrum.API.Dtos.Reviews
{
    public class UpdateReviewDto
    {
        [Range(1, 5, ErrorMessage = "La calificación debe estar entre 1 y 5.")]
        public int? Rating { get; set; }

        [MinLength(1, ErrorMessage = "El contenido de la reseña no puede estar vacío.")]
        [MaxLength(2000, ErrorMessage = "El contenido de la reseña no puede superar los 2000 caracteres.")]
        public string? Content { get; set; }

        [MaxLength(255, ErrorMessage = "La URL de la imagen no puede superar los 255 caracteres.")]
        public string? ImageUrl { get; set; }
    }
}