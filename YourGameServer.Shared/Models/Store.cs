using System.ComponentModel.DataAnnotations;

namespace YourGameServer.Shared.Models;

public enum Store
{
    [Display(Name = "Not Specified")]
    NotSpecified,
    [Display(Name = "App Store")]
    AppStore,
    [Display(Name = "Google Play")]
    GooglePlay,
    [Display(Name = "Microsoft Store")]
    MicrosoftStore,
    [Display(Name = "PS Store")]
    PSStore,
    [Display(Name = "Nintendo Store")]
    NintendoStore,
    [Display(Name = "Steam")]
    Steam,
    [Display(Name = "Epic Store")]
    EpicStore,
    [Display(Name = "EA Origin")]
    EAOrigin,
    [Display(Name = "UPlay")]
    UPlay,
    [Display(Name = "DMM")]
    DMM,
}