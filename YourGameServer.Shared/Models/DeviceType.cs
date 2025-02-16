using System.ComponentModel.DataAnnotations;

namespace YourGameServer.Shared.Models;

public enum DeviceType
{
    [Display(Name = "iOS")]
    IOS,
    [Display(Name = "Android")]
    Android,
    [Display(Name = "Browser")]
    WebGL,
    [Display(Name = "Windows")]
    Windows,
    [Display(Name = "Mac")]
    Mac,
    [Display(Name = "Linux")]
    Linux,
    [Display(Name = "PS4")]
    PS4,
    [Display(Name = "PS5")]
    PS5,
    [Display(Name = "XBOX One")]
    XBoxOne,
    [Display(Name = "Switch")]
    Switch,
}