#nullable disable
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
    [Display(Name = "PC/Mac")]
    StandAlone,
}