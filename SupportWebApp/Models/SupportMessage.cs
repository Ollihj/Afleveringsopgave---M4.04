using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace SupportWebApp.Models;

// En supporthenvendelse - gemmes som ét JSON-dokument i CosmosDB
public class SupportMessage
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonProperty("name")]
    [Required(ErrorMessage = "Navn skal udfyldes")]
    [StringLength(100, ErrorMessage = "Navn må højst være 100 tegn")]
    public string Name { get; set; } = "";

    [JsonProperty("email")]
    [Required(ErrorMessage = "Email skal udfyldes")]
    [EmailAddress(ErrorMessage = "Ugyldig email")]
    public string Email { get; set; } = "";

    [JsonProperty("phone")]
    [Phone(ErrorMessage = "Ugyldigt telefonnummer")]
    public string Phone { get; set; } = "";

    // Partition key i CosmosDB (/category)
    [JsonProperty("category")]
    [Required(ErrorMessage = "Vælg en kategori")]
    public string Category { get; set; } = "";

    [JsonProperty("description")]
    [Required(ErrorMessage = "Beskrivelse skal udfyldes")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Beskrivelsen skal være mellem 10 og 2000 tegn")]
    public string Description { get; set; } = "";

    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
