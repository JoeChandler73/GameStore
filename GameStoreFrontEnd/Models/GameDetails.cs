using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using GameStoreFrontEnd.Converters;

namespace GameStoreFrontEnd.Models;

public class GameDetails
{
    public int Id { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 3)]
    public required string Name { get; set; }

    [Required(ErrorMessage = "The Genre Field is Required")]
    [JsonConverter(typeof(StringConverter))]
    public string? GenreId { get; set; }

    [Range(1, 100)]
    public decimal Price { get; set; }

    public DateOnly ReleaseDate { get; set; }
}
