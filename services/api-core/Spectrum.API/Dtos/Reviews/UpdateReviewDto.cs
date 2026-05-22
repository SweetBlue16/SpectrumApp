using System.ComponentModel.DataAnnotations;

namespace Spectrum.API.Dtos.Reviews
{
    public class UpdateReviewDto
    {
        [Range(5, 10, ErrorMessage = "La calificacion debe estar entre 5 y 10.")]
        public int? Rating { get; set; }

        [MinLength(1, ErrorMessage = "El titulo de la resena no puede estar vacio.")]
        [MaxLength(120, ErrorMessage = "El titulo de la resena no puede superar los 120 caracteres.")]
        public string? Title { get; set; }

        [MinLength(1, ErrorMessage = "El contenido de la resena no puede estar vacio.")]
        [MaxLength(2000, ErrorMessage = "El contenido de la resena no puede superar los 2000 caracteres.")]
        public string? Content { get; set; }

        [MaxLength(255, ErrorMessage = "La URL del adjunto no puede superar los 255 caracteres.")]
        public string? ImageUrl { get; set; }

        [MaxLength(50, ErrorMessage = "El tipo de archivo no puede superar los 50 caracteres.")]
        public string? MediaType { get; set; }
    }
}
