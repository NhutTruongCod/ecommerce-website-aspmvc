using System;
using System.Collections.Generic;

namespace webbanhang.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public int? CategoryId { get; set; }

    public string? Description { get; set; }

    public virtual Productcategory? Category { get; set; }

    public virtual ICollection<Productimage> Productimages { get; set; } = new List<Productimage>();

    public virtual ICollection<Productvariant> Productvariants { get; set; } = new List<Productvariant>();
}
