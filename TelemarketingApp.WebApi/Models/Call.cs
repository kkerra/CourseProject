using System;
using System.Collections.Generic;

namespace TelemarketingApp.WebApi.Models;

public partial class Call
{
    public int CallId { get; set; }

    public DateTime CallDatetime { get; set; }

    public int Duration { get; set; }

    public string Result { get; set; } = null!;

    public string? Comment { get; set; }

    public int? EmployeeId { get; set; }

    public int? ClientId { get; set; }

    public virtual Client? Client { get; set; }

    public virtual Employee? Employee { get; set; }

    public virtual ICollection<Service> Services { get; set; } = new List<Service>();
}
