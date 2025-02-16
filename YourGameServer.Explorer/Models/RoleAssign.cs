using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace YourGameServer.Explorer.Models;

[PrimaryKey(nameof(NameIdentifier), nameof(Role))]
public record RoleAssign
{
    [Display(Name = "ID")]
    public string NameIdentifier { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    [ForeignKey("NameIdentifier"), JsonIgnore]
    public User? Owner { get; init; }
}
