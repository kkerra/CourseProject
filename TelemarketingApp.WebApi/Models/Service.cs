using System;
using System.Collections.Generic;

namespace TelemarketingApp.WebApi.Models;

public partial class Service
{
    public int ServiceId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public string Category { get; set; } = null!;

    public virtual ICollection<Call> Calls { get; set; } = new List<Call>();
}
