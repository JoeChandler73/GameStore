dotnet ef migrations add InitialCreate --output-dir Data/Migrations
dotnet ef database update
dotnet ef migrations add SeedGenres --output-dir Data/Migrations

dotnet new blazor --interactivity None --empty -n GameStoreFrontEnd