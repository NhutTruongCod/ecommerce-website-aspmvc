using webbanhang.Models;

public class HomeViewModel
{
    public IEnumerable<Productcategory> Categories { get; set; }
    public IEnumerable<ProductHomeDto> Products { get; set; }
}

public class ProductHomeDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public decimal Price { get; set; }
    public string Thumbnail { get; set; }
}
