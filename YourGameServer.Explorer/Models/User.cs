using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace YourGameServer.Explorer.Models;

// Add profile data for application users by adding properties to the ApplicationUser class
public record User
{
    [Key, Display(Name = "ID")]
    public string NameIdentifier { get; set; } = string.Empty;
    [JsonIgnore]
    public List<RoleAssign>? RoleAssigns { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? EmailAddress { get; set; }
    public DateTime Since { get; set; }
    public DateTime LastLogin { get; set; }
}
