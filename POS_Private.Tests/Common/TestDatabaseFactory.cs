using POS_Private.Domain.Interfaces;
using POS_Private.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace POS_Private.Tests.Common;

public static class TestDatabaseFactory
{
    public static (POS_PrivateDbContext Context, IUnitOfWork UnitOfWork) CreateInMemoryContext(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<POS_PrivateDbContext>()
            .UseInMemoryDatabase(databaseName: dbName ?? Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var context = new POS_PrivateDbContext(options);
        var uow = new UnitOfWork(context);
        return (context, uow);
    }

    public static async Task<(POS_PrivateDbContext Context, IUnitOfWork UnitOfWork)> CreateSeededContextAsync(string? dbName = null)
    {
        var (context, uow) = CreateInMemoryContext(dbName);
        await DatabaseSeeder.SeedAsync(context);
        return (context, uow);
    }
}
