using Microsoft.EntityFrameworkCore;

namespace StockLedge.Data.Contexts
{
	public class StockLedgeDbContext : DbContext
	{
		public StockLedgeDbContext(DbContextOptions<StockLedgeDbContext> options)
			: base(options)
		{
		}

		//db sets here

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.ApplyConfigurationsFromAssembly(typeof(StockLedgeDbContext).Assembly);
		}
	}
}
