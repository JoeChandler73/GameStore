using GameStoreFrontEnd.Models;

namespace GameStoreFrontEnd.Clients;

public class GamesClient(HttpClient httpClient)
{
    public async Task<GameSummary[]> GetGamesAsync() => 
        await httpClient.GetFromJsonAsync<GameSummary[]>("games") ?? [];

    public async Task<GameDetails> GetGameAsync(int gameId) =>
        await httpClient.GetFromJsonAsync<GameDetails>($"games/{gameId}") ?? 
            throw new Exception("Coiuld not find game");

    public async Task AddGameAsync(GameDetails game) =>
        await httpClient.PostAsJsonAsync("games", game);

    public async Task UpdateGameAsync(GameDetails updatedGame) =>
        await httpClient.PutAsJsonAsync($"games/{updatedGame.Id}", updatedGame);

    public async Task DeleteGameAsync(int id) =>
        await httpClient.DeleteAsync($"games/{id}");
}
