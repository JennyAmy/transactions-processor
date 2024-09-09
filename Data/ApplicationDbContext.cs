using Microsoft.EntityFrameworkCore;
using TransactionsProcessor.Entities;
using TransactionsProcessor.Utilities;

namespace TransactionsProcessor.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            foreach (var entity in builder.Model.GetEntityTypes())
            {
                foreach (var property in entity.GetProperties())
                {
                    var columnName = property.GetColumnName();
                    if (!string.IsNullOrEmpty(columnName))
                    {
                        var newColumnName = HelperMethods.ToSnakeCaseAndUpper(columnName);
                        property.SetColumnName(newColumnName);
                    }
                }
            }
        }
    }
}
