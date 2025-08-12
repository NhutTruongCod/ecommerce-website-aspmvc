using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using webbanhang.Models;
using webbanhang.Data;
using Microsoft.EntityFrameworkCore;

namespace webbanhang.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly MyDbContext _context;

    public HomeController(ILogger<HomeController> logger, MyDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        var category = _context.Productcategories.ToList();
        
        var product = _context.Products
            .Include(p => p.Productvariants)
            .Include(p => p.Productimages)
            .Select(p => new ProductHomeDto
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                Price = p.Productvariants.FirstOrDefault().Price,
                Thumbnail = p.Productimages.FirstOrDefault().ImageUrl
            }).ToList();
        return View(new HomeViewModel
        {
            Categories = category,
            Products = product
        });
    }

    [Route("gioi-thieu")]
    public IActionResult About()
    {
        return View();
    }

    [Route("lien-he")]
    public IActionResult Contact()
    {
        return View();
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
