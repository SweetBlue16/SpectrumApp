using System.ComponentModel.DataAnnotations;

namespace Spectrum.API.Dtos.Reviews
{
    public class CreateReviewDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del videojuego debe ser válido.")]
        public int GameId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "La calificación debe estar entre 1 y 5.")]
        public int Rating { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "El contenido de la reseña es obligatorio.")]
        [MaxLength(2000, ErrorMessage = "El contenido de la reseña no puede superar los 2000 caracteres.")]
        public string Content { get; set; } = string.Empty;

        [MaxLength(255, ErrorMessage = "La URL de la imagen no puede superar los 255 caracteres.")]
        public string? ImageUrl { get; set; }
    }
}