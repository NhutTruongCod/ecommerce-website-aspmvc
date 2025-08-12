using System;
using System.Collections.Generic;

namespace webbanhang.Models;

public partial class Size
{
    public int SizeId { get; set; }

    public string SizeName { get; set; } = null!;

    public int? CategoryId { get; set; }

    public virtual Productcategory? Category { get; set; }

    public virtual ICollection<Productvariant> Productvariants { get; set; } = new List<Productvariant>();
}
