using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace POS_Private.Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<POS_PrivateDbContext>
{
    public POS_PrivateDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<POS_PrivateDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=pos_private;Username=postgres;Password=123456;Include Error Detail=true");

        return new POS_PrivateDbContext(optionsBuilder.Options);
    }
}
