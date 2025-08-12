using System;
using System.Collections.Generic;

namespace webbanhang.Models;

public partial class Shippinginfo
{
    public int ShippingId { get; set; }

    public int? OrderId { get; set; }

    public string? ReceiverName { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public string? ShippingStatus { get; set; }

    public virtual Order? Order { get; set; }
}
