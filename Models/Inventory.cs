using System;
using System.Collections.Generic;

namespace webbanhang.Models;

public partial class Inventory
{
    public int InventoryId { get; set; }

    public int? VariantId { get; set; }

    public int Quantity { get; set; }

    public virtual Productvariant? Variant { get; set; }
}
