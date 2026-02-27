using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Warehouse.Api.Data;

namespace Warehouse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeesController(WarehouseDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? departmentId)
    {
        var query =
            from e in db.Employees
            join d in db.Departments on e.DepartmentId equals d.Id
            join u in db.Users on e.UserId equals u.Id
            select new
            {
                e.Id,
                e.FullName,
                e.Position,
                e.Phone,
                e.IsActive,
                Department = new { d.Id, d.Name, d.Code },
                User = new { u.Id, u.Username, u.Email }
            };

        if (departmentId.HasValue)
        {
            query = query.Where(x => x.Department.Id == departmentId.Value);
        }

        return Ok(await query.OrderBy(x => x.FullName).ToListAsync());
    }
}
