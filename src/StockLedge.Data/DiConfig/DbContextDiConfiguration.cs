using Microsoft.Extensions.DependencyInjection;
using StockLedge.Data.Contexts;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace StockLedge.Data.DiConfig
{
	public static class DbContextDiConfiguration
	{
		public static IServiceCollection DbContextDiConfig(this IServiceCollection services, IConfiguration configuration)
		{
			var connectionString = configuration.GetConnectionString("DefaultConnection");

			services.AddDbContext<StockLedgeDbContext>(options =>
			options.UseMySql(
				connectionString,
				ServerVersion.AutoDetect(connectionString),
				b => b.MigrationsAssembly(typeof(StockLedgeDbContext).Assembly.FullName)
			));
			return services;
		}
	}
}
