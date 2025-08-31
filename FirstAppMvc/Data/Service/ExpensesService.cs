using FirstAppMvc.Models;
using Microsoft.EntityFrameworkCore;

namespace FirstAppMvc.Data.Service
{
    public class ExpensesService : IExpensesService
    {
        private readonly FirstAppMvcContext _context;

        public ExpensesService(FirstAppMvcContext context)
        {
            _context = context;
        }

        public async Task Add(Expense expense)
        {
            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Expense>> GetAll()
        {
            var expenses = await _context.Expenses.ToListAsync();
            return expenses;
        }

        public IQueryable GetChartData()
        {
            var data = _context.Expenses
                               .GroupBy(e => e.Category)
                               .Select(g => new
                               {
                                   Category = g.Key,
                                   Total = g.Sum(e => e.Amount)
                               });
            return data;
        }
    }
}
