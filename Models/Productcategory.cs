using System;
using System.Collections.Generic;

namespace webbanhang.Models;

public partial class Productcategory
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public string? CategoryImages { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual ICollection<Size> Sizes { get; set; } = new List<Size>();
}
