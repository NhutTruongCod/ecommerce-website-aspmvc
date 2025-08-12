using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using webbanhang.Models;
using webbanhang.Data;
using Microsoft.EntityFrameworkCore;

namespace webbanhang.ViewComponents
{
    public class MenuCategoriesViewComponent : ViewComponent
    {
        private readonly MyDbContext _context;

        public MenuCategoriesViewComponent(MyDbContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            var categories = _context.Productcategories.ToList();
            return View(categories);
        }
    }
}
