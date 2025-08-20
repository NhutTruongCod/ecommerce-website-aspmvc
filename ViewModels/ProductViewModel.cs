using webbanhang.Models;

namespace webbanhang.ViewModels
{

    public class ProductViewModel
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int CategoryId { get; set; }
        public Microsoft.AspNetCore.Mvc.Rendering.SelectList? Categories { get; set; }

        public string? Description { get; set; }
        public decimal Price { get; set; }

        public List<int> SizeIds { get; set; } = new();
        public List<int> ColorIds { get; set; } = new();
        public List<IFormFile> Images { get; set; } = new();

        // SelectLists for dropdowns
        public Microsoft.AspNetCore.Mvc.Rendering.SelectList? Sizes { get; set; }
        public Microsoft.AspNetCore.Mvc.Rendering.SelectList? Colors { get; set; }
    }
}