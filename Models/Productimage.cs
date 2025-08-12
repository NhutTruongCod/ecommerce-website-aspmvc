﻿using System;
using System.Collections.Generic;

namespace webbanhang.Models;

public partial class Productimage
{
    public int ImageId { get; set; }

    public int? ProductId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public bool? IsThumbnail { get; set; }

    public virtual Product? Product { get; set; }
}
