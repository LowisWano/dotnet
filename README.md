# ASP .NET MVC Workflow

## Contents
- Installation and Database Setup
- How to create an MVC project with visual studio
- Model
- Context
- Migrations and Syncing Database
- Service
- Controllers
- Views
- Forms and HTTP Action Methods

___

## Installation and Database Setup
Before creating a project, we need to install Visual Studio IDE, SSMS, and SQL Server to develop and run the application.

### Installation
1. Install Visual Studio from https://visualstudio.microsoft.com/

2. Install SQL Server Management Studio 21 from https://learn.microsoft.com/en-us/ssms/install/install

### Setup SQL Server Instance with Docker
To setup database, follow these main steps:

1. Install and run Docker on your system.

2. Pull the Microsoft SQL Server Docker image
``` bash
docker pull mcr.microsoft.com/mssql/server:2019-latest
```
3. Run instance
```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Password123$" -p 1433:1433 --name myFirstApp -d mcr.microsoft.com/mssql/server:2019-latest
```

### Connect visual studio project with SQL Server Instance
1. Click `View > Server Explorer > Data Connections > Create New SQL Server Database`
2. Use these connection details:
    - Server Name: `localhost,1433`
    - User Name: `sa`
    - Password: `Password123$`
    - Encrypt: `Optional`
    - Check `Trust Server Certificate`
    - Create database name
3. Right click on the database and click `properties`
4. Copy the connection string
5. Paste the connection string to your `appsettings.json`
```json
{
  ...,
  "ConnectionStrings": {
    "DefaultConnectionString": "Data Source=localhost,1433;Initial Catalog=myFirstApp_data;User ID=sa;Password=Password123$;Pooling=False;Encrypt=False;Trust Server Certificate=True"
  }
}

```
___

## How to create an MVC project with visual studio
1. Click `Create New Project > ASP.NET Core Web App (Model-View-Controller) Template`
2. Create Project Name
3. Select `.NET 9.0` Framework and click `Create`.

___

## Model
Models are the entities defined in your project's database. Every `model` should have a corresponding `controller` and `view` to follow separation of concerns . For example, if we have an `Expenses` model, it should have an `ExpensesController.cs` and its corresponding views like `Index.cshtml`.

To create a model:

1. Right Click on the `Models` folder and click `Add > Class`
2. Create a name for the model `<Name>.cs`
3. Define the class

```cs
# Expense.cs
using System.ComponentModel.DataAnnotations;

namespace FirstAppMvc.Models
{
    public class Expense
    {
        public int Id { get; set; }

        [Required]
        public string Description { get; set; } = null!;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount needs to be higher than 0")]
        public double Amount { get; set; }

        [Required]
        public string Category { get; set; } = null!;

        public DateTime Date { get; set; } = DateTime.Now;
    }
}

```
## Context
The context class acts as the bridge between the database and the project. Here we will define the instances for each model classes.

1. Right click on the project root and click `Add > New Folder`
2. Name folder as `Data`
3. Right click on `Data` and click `Add > Class` and name it as `<Name>Context.cs`

Next we install .NET Entity Framework Core Dependencies using NuGet Package

1. Right click on `Dependencies > Manage NuGet Packages`
2. Install `Microsoft.EntityFrameworkCore.SqlServer`
3. Install `Microsoft.EntityFrameworkCore`
4. Install `Microsoft.EntityFrameworkCore.Tools`

Your context class should look something like this
```cs
# FirstAppMvcContext.cs
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

```
___

## Migrations and Syncing Database
Before we setup migrations, setup the services in `Program.cs`
```cs
using FirstAppMvc.Data;
using FirstAppMvc.Data.Service;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<FirstAppMvcContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnectionString")));

builder.Services.AddScoped<IExpensesService, ExpensesService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

```

Then we run the following commands to apply the Model classes into the database

```bash
# create migration
Add-Migration "migration name here"

# sync migrations with database
Update-Database
```

Try checking in SSMS if the changes are reflected. Click on the table and select `Refresh`

___

## Service
Setup context in a service class for security.

Create an interface to define its method signatures
```cs
# IExpensesService.cs
using FirstAppMvc.Models;

namespace FirstAppMvc.Data.Service
{
    public interface IExpensesService
    {
        Task<IEnumerable<Expense>> GetAll();
        Task Add(Expense expense);
        IQueryable GetChartData();
    }
}
```

Create a service class to write the logic
```cs
# ExpensesService.cs
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
```

## Controllers
- Right click controllers folder then select `Add` > `Controller...`
- Select `MVC Controller - Empty`
- Create name of the controller `<name>Controller.cs`

```cs
namespace FirstAppMvc.Controllers
{
    public class ExpensesController : Controller
    {
        private readonly IExpensesService _expensesService;
        
        public ExpensesController(IExpensesService expensesService)
        {
            _expensesService = expensesService;
        }

        public async Task<IActionResult> Index()
        {
            var expenses = await _expensesService.GetAll();
            return View(expenses);
        }

        public IActionResult Create()
        {
            return View();
        }

        public IActionResult Update()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Create(Expense expense)
        {
            if(ModelState.IsValid)
            {
                await _expensesService.Add(expense);
                return RedirectToAction("Index");
            }

            return View(expense);
        }

        public IActionResult GetChart()
        {
            var data = _expensesService.GetChartData();
            return Json(data);
        }
    }
}

```

## Views
- In the method of the controller you are making a view for, right click then select `Add View`
- Click `Add`
- Create name of the view `<name>.cshtml`

```html
@model IEnumerable<FirstAppMvc.Models.Expense>

<h2>
    My Expenses
</h2>

<div class="container">
    <table class="table table-bordered">
        <thead class="table-light">
            <tr>
                <th>Description</th>
                <th>Amount</th>
                <th>Category</th>
                <th>Date</th>
            </tr>
        </thead>
        <tbody>
            @foreach(var item in Model)
            {
                <tr>
                    <td>@item.Description</td>
                    <td>@item.Amount $</td>
                    <td>@item.Category</td>
                    <td>@item.Date.ToString("yyyy-MM-dd")</td>
                </tr>
            }
        </tbody>
    </table>
</div>
```

`@model IEnumerable<FirstAppMvc.Models.Expense>` defines the model that we pass on to the view from the `expenses` variable we passed in the `Index()` method from the controller.

```cs
# ExpensesController.cs
public async Task<IActionResult> Index()
{
    var expenses = await _expensesService.GetAll();
    return View(expenses);
}
```

## Forms and HTTP Actions
Next, let us create a `/Create` page

In the controller, define the `Create()` view method and its action method
```cs
public IActionResult Create()
{
    return View();
}

[HttpPost]
public async Task<IActionResult> Create(Expense expense)
{
    if(ModelState.IsValid)
    {
        await _expensesService.Add(expense);
        return RedirectToAction("Index");
    }

    return View(expense);
}
```

Right click on the `Create()` view method and click `Add View`:
```cshtml
@model FirstAppMvc.Models.Expense

<div class="container">
    <h2>Add Expense</h2>
    <form asp-action="Create" method="post">
        <div>
            <label asp-for="Description" class="form-label"></label>
            <input asp-for="Description" class="form-control"/>
        </div>

        <div>
            <label asp-for="Amount" class="form-label"></label>
            <input asp-for="Amount" class="form-control" />
        </div>

        <div>
            <label asp-for="Category" class="form-label"></label>
            <input asp-for="Category" class="form-control" />
        </div>
        <button type="submit" class="btn btn-primary">Add Expense</button>
    </form>
</div>
```

The html property `asp-action="Create"` specifies which method this form sends the data to.