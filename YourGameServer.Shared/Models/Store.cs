using System.ComponentModel.DataAnnotations;

namespace YourGameServer.Shared.Models;

public enum Store
{
    [Display(Name = "Not Specified")]
    NotSpecified,
    // Official stores
    [Display(Name = "App Store")]
    AppStore = 1,
    [Display(Name = "Google Play")]
    GooglePlay,
    [Display(Name = "Kindle Store")]
    KindleStore,
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
    // External payment services
    [Display(Name = "Stripe")]
    Stripe = 1000,
    [Display(Name = "PayPal")]
    PayPal,
    [Display(Name = "Braintree")]
    Braintree,
    [Display(Name = "Adyen")]
    Adyen,
    [Display(Name = "Square")]
    Square,
    [Display(Name = "SBペイメントサービス")]
    SBPaymentService,
    [Display(Name = "ROBOT PAYMENT")]
    RobotPayment,
    [Display(Name = "GMO Payment")]
    GMOPayment,
    [Display(Name = "ペイジェント")]
    Paygent
}