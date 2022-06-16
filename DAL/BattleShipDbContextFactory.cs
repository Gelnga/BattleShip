using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DAL;

public class BattleShipDbContextFactory : IDesignTimeDbContextFactory<BattleShipDbContext>
{
    public BattleShipDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BattleShipDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5436;Username=postgres;Password=postgres;database=BattleShip");

        return new BattleShipDbContext(optionsBuilder.Options);
    }
}