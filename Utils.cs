using Microsoft.EntityFrameworkCore;

namespace TypedFaultDemo;
internal static class Utils
{
    public static async Task<IHost> MigrateDb<TContext>(this IHost host)
        where TContext : DbContext
    {
        using var serviceScope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var context = serviceScope.ServiceProvider.GetRequiredService<TContext>();

        await context.Database.MigrateAsync();

        return host;
    }
}
