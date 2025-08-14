using System;
using System.Collections.Generic;

namespace webbanhang.Models;

public partial class User
{
    public int UserId { get; set; }

    public string? FullName { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int RoleId { get; set; }

    public virtual Cart? Cart { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual Role Role { get; set; } = null!;
}
