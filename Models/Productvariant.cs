using System;
using System.Collections.Generic;

namespace webbanhang.Models;

public partial class Productvariant
{
    public int VariantId { get; set; }

    public int? ProductId { get; set; }

    public int? SizeId { get; set; }

    public int? ColorId { get; set; }

    public decimal Price { get; set; }

    public string? Sku { get; set; }

    public virtual ICollection<Cartitem> Cartitems { get; set; } = new List<Cartitem>();

    public virtual Color? Color { get; set; }

    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();

    public virtual ICollection<Orderitem> Orderitems { get; set; } = new List<Orderitem>();

    public virtual Product? Product { get; set; }

    public virtual Size? Size { get; set; }
}
