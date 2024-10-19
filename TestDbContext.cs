using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace TypedFaultDemo;

internal class TestDbContext(DbContextOptions<TestDbContext> options): DbContext(options)
{
    public DbSet<TestEntity> TestEntities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AddTransactionalOutboxEntities();
    }
}
