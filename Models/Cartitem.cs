using System;
using System.Collections.Generic;

namespace webbanhang.Models;

public partial class Cartitem
{
    public int CartItemId { get; set; }

    public int? CartId { get; set; }

    public int? VariantId { get; set; }

    public int Quantity { get; set; }

    public virtual Cart? Cart { get; set; }

    public virtual Productvariant? Variant { get; set; }
}
