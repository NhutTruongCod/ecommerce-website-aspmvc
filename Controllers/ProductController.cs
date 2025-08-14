using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using webbanhang.Models;
using webbanhang.Data;
using Microsoft.EntityFrameworkCore;

namespace webbanhang.Controllers;

[Route("sanpham")]
public class ProductController : Controller
{
    private readonly ILogger<ProductController> _logger;
    private readonly MyDbContext _context;

    public ProductController(ILogger<ProductController> logger, MyDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    [HttpGet("")]
    public IActionResult Index(string q)
    {
        var productsQuery = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Productimages)
            .Include(p => p.Productvariants)
            .AsSplitQuery()
            .AsQueryable();

        if (!string.IsNullOrEmpty(q))
        {
            productsQuery = productsQuery.Where(p => p.ProductName.Contains(q) || p.Category.CategoryName.Contains(q));
        }

        var products = productsQuery.ToList();
        return View(products);
    }

    [HttpGet("category/{id}")]
    public IActionResult ByCategory(int id)
    {
        var products = _context.Products
            .Where(p => p.CategoryId == id)
            .Include(p => p.Category)
            .Include(p => p.Productimages)
            .Include(p => p.Productvariants)
            .ToList();
        return View("Index", products); // Dùng lại view Index
    }


    [HttpGet("{id}")]
    public IActionResult Detail(int id)
    {
        var product = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Productimages)
            .Include(p => p.Productvariants)
                .ThenInclude(v => v.Size)
            .Include(p => p.Productvariants)
                .ThenInclude(v => v.Color)
            .FirstOrDefault(p => p.ProductId == id);
        if (product == null)
        {
            return NotFound();
        }
        return View(product);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}