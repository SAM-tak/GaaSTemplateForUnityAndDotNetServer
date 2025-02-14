#nullable disable
using System.ComponentModel.DataAnnotations;

namespace YourGameServer.Shared.Models;

public enum ConsumableOrigin
{
    [Display(Name = "Not Specified")]
    NotSpecified,
    [Display(Name = "Log-In Reward")]
    LogInReward,
    [Display(Name = "Mission Reward")]
    MissionReward,
    [Display(Name = "Quest Reward")]
    QuestReward,
    [Display(Name = "Achievement Reward")]
    AchievementReward,
    [Display(Name = "Insentive")]
    Insentive,
    [Display(Name = "Wide-Distribution")]
    Distribution,
    [Display(Name = "Loot Box")]
    LootBox,
    [Display(Name = "Compensation")]
    Compensation,
    [Display(Name = "Apologies")]
    Apologies,
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