using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SmartFactory.Infrastructure.Data;

/// <summary>
/// Design-time factory for EF Core migrations.
/// This allows running migrations without starting the full application.
/// </summary>
public class SmartFactoryDbContextFactory : IDesignTimeDbContextFactory<SmartFactoryDbContext>
{
    public SmartFactoryDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SmartFactoryDbContext>();
        optionsBuilder.UseSqlite("Data Source=SmartFactory.db");

        return new SmartFactoryDbContext(optionsBuilder.Options);
    }
}
