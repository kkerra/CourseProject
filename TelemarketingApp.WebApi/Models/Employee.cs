using System;
using System.Collections.Generic;

namespace TelemarketingApp.WebApi.Models;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public string Surname { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Patronymic { get; set; }

    public string Login { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? Email { get; set; }

    public int? RoleId { get; set; }

    public virtual ICollection<Call> Calls { get; set; } = new List<Call>();

    public virtual Role? Role { get; set; }
}
