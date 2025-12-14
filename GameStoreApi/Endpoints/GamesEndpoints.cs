using System.Text.Json;
using GameStoreApi.Data;
using GameStoreApi.Dtos;
using GameStoreApi.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace GameStoreApi.Endpoints;

public static class GamesEndpoints
{
    private const string GetGameEndPointName = "GetGame";

    public static RouteGroupBuilder MapGamesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("games")
            .WithParameterValidation();

        // GET /games
        group.MapGet("/", async (GameStoreContext dbContext) =>
            await dbContext.Games
            .Include(game => game.Genre)
            .Select(game => game.ToGameSummaryDto())
            .AsNoTracking()
            .ToListAsync());

        // GET /games/1
        group.MapGet("/{id}", async (
            int id, 
            GameStoreContext dbContext,
            IDistributedCache cache,
            ILogger<Program> logger) =>
        {
            var cachedJson = await cache.GetStringAsync($"game:{id}");
            if(cachedJson is not null)
                return Results.Ok(JsonSerializer.Deserialize<GameDetailsDto>(cachedJson));
            
            logger.LogInformation("Cache MISS for game {GameId}", id);

            var game = await dbContext.Games.FindAsync(id);
            if(game is null)
                return Results.NotFound();

            var dto = game.ToGameDetailsDto();

            await cache.SetStringAsync(
                key: $"game:{id}", 
                value: JsonSerializer.Serialize(dto),
                options: new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });

            return Results.Ok(dto);
        })
        .WithName(GetGameEndPointName);


        // POST /games
        group.MapPost("/", async (CreateGameDto newGame, GameStoreContext dbContext) =>
        {
            var game = newGame.ToEntity();

            dbContext.Games.Add(game);
            await dbContext.SaveChangesAsync();

            return Results.CreatedAtRoute(GetGameEndPointName, new { Id = game.Id }, game.ToGameDetailsDto());
        });

        // PUT /games/1
        group.MapPut("/{id}", async (int id, UpdateGameDto updatedGame, GameStoreContext dbContext) =>
        {
            var existingGame = await dbContext.Games.FindAsync(id);

            if (existingGame is null)
                return Results.NotFound();

            dbContext.Entry(existingGame)
                .CurrentValues
                .SetValues(updatedGame.ToEntity(id));

            await dbContext.SaveChangesAsync();

            return Results.NoContent();
        });

        // DELETE /games/1
        group.MapDelete("/{id}", async (int id, GameStoreContext dbContext) => {

            await dbContext.Games
                .Where(game => game.Id == id)
                .ExecuteDeleteAsync();

            return Results.NoContent();
        });

        return group;
    }
}