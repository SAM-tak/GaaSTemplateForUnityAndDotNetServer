# GaaSTemplateForUnityAndDotNetServer

GaaS Template for Unity &amp; .NET server side repository

## Build

```sh
dotnet build -tl:off -c Release
```

## Entity Framework Migration Example

```sh
dotnet ef migrations add InitialCreation --project YourGameServer.Shared --startup-project YourGameServer.Game --context SqliteGameDbContext
dotnet ef database update --project YourGameServer.Shared --startup-project YourGameServer.Game --context SqliteGameDbContext
```

## Expolorer Entity Framework Migration Example

```sh
YourGameServer.Explorer> dotnet ef migrations add InitialCreation --context SqliteExplorerDbContext
YourGameServer.Explorer> dotnet ef database update --context SqliteExplorerDbContext
```

## Test

```sh
dotnet test --tl:off --logger "trx;LogFileName=test_results.trx"
```

## [Generate random letters with Powershell](https://devblogs.microsoft.com/scripting/generate-random-letters-with-powershell/)

```sh
PS> -join ((50..57) + (65..90) + (97..122) | Get-Random -Count 16 | % {[char]$_})
```
