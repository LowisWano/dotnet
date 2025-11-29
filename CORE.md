# ASP.NET Core Web API Workflow

# Project Setup
- Open Visual Studio > ASP .NET CORE Web API
- Select .NET version `8.0`

# Connect project to database
- Run SQL Server
- Add connection string to appsettings.json
```json
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=localhost,1433;Initial Catalog=EmployeesDb;User ID=sa;Password=Password123$;Pooling=False;Encrypt=False;Trust Server Certificate=True"
  }
```
 
# Entity Framework Core
- Right click dependences > Manage NuGet Packages
- Install Microsoft.EntityFrameWorkCore.SqlServer and Microsoft.EntityFrameworkCore.Tools version 8.0.12 (this is for .NET 8.0, make sure to match compatible versions for the .NET version you are using)

# Models
- Right click project directory > Add > New Folder > name it `Models` > Add another folder inside it named `Entity`
- Add class for example `Employee.cs`
```cs
namespace WebApplication1.Models.Entities
{
    public class Employee
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public string? Phone { get; set; }
        public decimal Salary { get; set; }
    }
}
```

# Create DbContext
- Create folder named `Data`
- Inside that folder, create class name it `ApplicationDbContext.cs`
```cs
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models.Entities;

namespace WebApplication1.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options){ }

        public DbSet<Employee> Employees { get; set; }
    }
}
```
- This is where you will set your entities
- Go to Program.cs to inject DbContext to your project
```cs
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DbContext config here 
builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
```
- Apply migrations

```bash
add-migration "initial migration"
```
- sync database with migrations
```bash
update-databasee
```

# Create controllers
- Right click on folder `Controllers` > Add > Controllers...
- Select API Controller - Empty
- Name it `EmployeesController.cs`
- Inject DbContext to controller so that you can interact with the database

```cs
private readonly ApplicationDbContext dbContext;

public EmployeesController(ApplicationDbContext dbContext)
{
    this.dbContext = dbContext;
}
```

### GET all endpoint
```cs
[HttpGet]
public IActionResult GetAllEmployees()
{
    var allEmployees = dbContext.Employees.ToList();
    return Ok(allEmployees);
}
```

### GET by id endpoint
```cs
[HttpGet]
[Route("{id:guid}")]
public IActionResult GetEmployeeById(Guid id)
{
    var employee = dbContext.Employees.Find(id);

    if (employee is null)
    {
        return NotFound();
    }

    return Ok(employee);
}
```

### POST endpoint
```cs
[HttpPost]
public IActionResult AddEmployee(AddEmployeeDto addEmployeeDto)
{
    var employee = new Employee()
    {
        Name = addEmployeeDto.Name,
        Email = addEmployeeDto.Email,
        Phone = addEmployeeDto.Phone,
        Salary = addEmployeeDto.Salary
    };

    dbContext.Employees.Add(employee);
    dbContext.SaveChanges();

    return Ok(employee);
}
```

### PUT endpoint (update)
```cs
[HttpPut]
[Route("{id:guid}")]
public IActionResult UpdateEmployee(Guid id, UpdateEmployeeDto updateEmployeeDto)
{
    var employee = dbContext.Employees.Find(id);

    if (employee is null)
    {
        return NotFound();
    }

    employee.Name = updateEmployeeDto.Name;
    employee.Email = updateEmployeeDto.Email;
    employee.Phone = updateEmployeeDto.Phone;
    employee.Salary = updateEmployeeDto.Salary;

    dbContext.SaveChanges();
    return Ok(employee);
}
```

### DELETE endpoint
```cs
[HttpDelete]
[Route("{id:guid}")]
public IActionResult DeleteEmployee(Guid id)
{
    var employee = dbContext.Employees.Find(id);

    if (employee is null)
    {
        return NotFound();
    }

    dbContext.Employees.Remove(employee);
    dbContext.SaveChanges();
    
    return Ok();
}
```