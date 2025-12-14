dotnet ef migrations add InitialCreate --output-dir Data/Migrations
dotnet ef database update
dotnet ef migrations add SeedGenres --output-dir Data/Migrations

dotnet new blazor --interactivity None --empty -n GameStoreFrontEnd

dotnet tool install -g Aspire.Cli

dotnet add package Aspire.Hosting.PostGreSQL
dotnet add package Aspire.Npgsql.EntityFramework.PostgreSQL

dotnet add package Aspire.Hosting.Redis
dotnet add package Aspire.StackExchange.Redis.DistributedCaching