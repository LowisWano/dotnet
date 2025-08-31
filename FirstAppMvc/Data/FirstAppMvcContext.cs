using FirstAppMvc.Models;
using Microsoft.EntityFrameworkCore;

namespace FirstAppMvc.Data
{
    public class FirstAppMvcContext : DbContext
    {
        public FirstAppMvcContext(DbContextOptions<FirstAppMvcContext> options):base(options) { }

        public DbSet<Expense> Expenses { get; set; }
    }
}
