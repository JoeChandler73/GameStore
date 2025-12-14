using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace GameStoreApi.Data;

public static class DataExtensions
{
    public static async Task MigrateDb(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetService<GameStoreContext>();
        await dbContext.Database.MigrateAsync();
    }
}
