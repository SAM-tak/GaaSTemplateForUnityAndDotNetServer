# GaaSTemplateForUnityAndDotNetServer

GaaS Template for Unity &amp; .NET server side repository

## Entity Framework Migration Example

```sh
dotnet ef migrations add InitialCreation --project YourGameServer.Shared --startup-project YourGameServer.Game --context SqliteGameDbContext
dotnet ef database update --project YourGameServer.Shared --startup-project YourGameServer.Game --context SqliteGameDbContext
```
