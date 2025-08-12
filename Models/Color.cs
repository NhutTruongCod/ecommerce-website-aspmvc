using System;
using System.Collections.Generic;

namespace webbanhang.Models;

public partial class Color
{
    public int ColorId { get; set; }

    public string ColorName { get; set; } = null!;

    public virtual ICollection<Productvariant> Productvariants { get; set; } = new List<Productvariant>();
}
