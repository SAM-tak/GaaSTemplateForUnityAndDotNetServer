using System.ComponentModel.DataAnnotations;

namespace YourGameServer.Explorer.Models;

// Add profile data for application users by adding properties to the ApplicationUser class
public enum ExplorerUserRole
{
    [Display(Name = "Guest")]
    Guest,
    [Display(Name = "Administrator")]
    Admin,
    [Display(Name = "Developer")]
    Developer,
    [Display(Name = "Tester")]
    Tester,
    [Display(Name = "Financialist")]
    Financialist,
}
