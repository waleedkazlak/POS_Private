using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace POS_Private.Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<POS_PrivateDbContext>
{
    public POS_PrivateDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<POS_PrivateDbContext>();
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=POS_Private;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True");

        return new POS_PrivateDbContext(optionsBuilder.Options);
    }
}
