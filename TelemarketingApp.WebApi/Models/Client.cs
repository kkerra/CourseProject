using System;
using System.Collections.Generic;

namespace TelemarketingApp.WebApi.Models;

public partial class Client
{
    public int ClientId { get; set; }

    public string Surname { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Patronymic { get; set; }

    public string? Address { get; set; }

    public string PhoneNumber { get; set; } = null!;

    public string? InteractionStatus { get; set; }

    public virtual ICollection<Call> Calls { get; set; } = new List<Call>();
}
