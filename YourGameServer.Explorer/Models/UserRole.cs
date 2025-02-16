using System.ComponentModel.DataAnnotations;

namespace YourGameServer.Explorer.Models;

// Add profile data for application users by adding properties to the ApplicationUser class
public enum UserRole
{
    [Display(Name = "Guest")]
    Guest,
    [Display(Name = "Admin")]
    Admin,
    [Display(Name = "Dev")]
    Developer,
    [Display(Name = "GD")]
    GameDesigner,
    [Display(Name = "Tester")]
    Tester,
    [Display(Name = "Finance")]
    Finance,
    [Display(Name = "CS")]
    CustomerSupport,
}
