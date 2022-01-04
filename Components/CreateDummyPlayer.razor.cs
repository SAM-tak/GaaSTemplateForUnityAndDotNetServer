using System;
using System.Threading.Tasks;
using Bogus;
using YourGameServer.Controllers;
using YourGameServer.Extensions;
using YourGameServer.Models;

namespace YourGameServer.Components;

partial class CreateDummyPlayer
{
    private async Task Execute()
    {
        buttonDisabled = true;
        await Task.Delay(1);
        Console.WriteLine($"LocalRootPathComponent {navigationManager.ToLocalBasePathComponent()}");

        var fakers = new Faker[] {
            new Faker("en"),
            new Faker("fr"),
            new Faker("de"),
            new Faker("it"),
            new Faker("es"),
            new Faker("ja"),
            new Faker("zh_CN"),
            new Faker("zh_TW"),
            new Faker("ko"),
        };

        for(int i = 0; i < createDummyPlayerModel.Count; ++i) {
            latestLUID = await LUID.NewLUIDStringAsync(null);
            //Console.WriteLine($"New LUID {luid.id1} {luid.id0} {latestLUID}");
            var playerAccount = await PlayerAccountsController.CreateAsync(context, new AccountCreationModel {
                DeviceType = (DeviceType)Random.Shared.Next(2),
                DeviceId = Guid.NewGuid().ToString("N")
            });

            var faker = fakers[Random.Shared.Next(fakers.Length)];

            playerAccount.Profile.Name = faker.Name.FullName();
            playerAccount.Profile.Motto = faker.Lorem.Sentence(8);

            context.Add(playerAccount);
        }
        await context.SaveChangesAsync();
        buttonDisabled = false;
        await Task.Delay(1);
    }

    private async Task DeleteAll()
    {
        button2Disabled = true;
        await Task.Delay(1);

        context.PlayerAccounts.RemoveRange(context.PlayerAccounts);

        await context.SaveChangesAsync();
        button2Disabled = false;
        await Task.Delay(1);
    }
}
