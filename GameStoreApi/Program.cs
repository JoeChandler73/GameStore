using GameStoreApi.Data;
using GameStoreApi.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<GameStoreContext>("GameStore");
builder.AddRedisDistributedCache("redis");

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapGenresEndpoints();
app.MapGamesEndpoints();

await app.MigrateDb();

app.Run();
