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
    [Display(Name = "Purchase")]
    Purchase,
}
