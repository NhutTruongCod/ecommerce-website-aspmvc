using Microsoft.AspNetCore.Mvc;
using webbanhang.Models;
using webbanhang.Data;
using Microsoft.EntityFrameworkCore;
using webbanhang.Services;
using Microsoft.AspNetCore.Authorization;
using webbanhang.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace webbanhang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly MyDbContext _context;
        private readonly IProductService _productService;

        public ProductController(MyDbContext context, IProductService productService)
        {
            _context = context;
            _productService = productService;
        }

        public async Task<IActionResult> Index(int id)
        {
            var products = await _productService.GetAllProductsAsync();
            return View(products);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // GET: Hiển thị form tạo sản phẩm
            var categories = await _context.Productcategories.ToListAsync();
            var colors = await _context.Colors.ToListAsync();
            var sizes = await _context.Sizes.ToListAsync();

            var model = new ProductViewModel
            {
                Categories = new SelectList(categories, "CategoryId", "CategoryName"),
                Colors = new SelectList(colors, "ColorId", "ColorName"),
                Sizes = new SelectList(sizes, "SizeId", "SizeName")
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _context.Productcategories.ToListAsync();
                var colors = await _context.Colors.ToListAsync();
                var sizes = await _context.Sizes.ToListAsync();

                model.Categories = new SelectList(categories, "CategoryId", "CategoryName");
                model.Colors = new SelectList(colors, "ColorId", "ColorName");
                model.Sizes = new SelectList(sizes, "SizeId", "SizeName");
                return View(model);
            }

            var productId = await _productService.CreateProductAsync(model);
            return RedirectToAction("ManageInventory", new { id = productId });
        }

        // Quản lý tồn kho cho sản phẩm
        public async Task<IActionResult> ManageInventory(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateInventory(int variantId, int quantity)
        {
            var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.VariantId == variantId);
            if (inventory != null)
            {
                inventory.Quantity = quantity;
                await _context.SaveChangesAsync();
            }
            return Json(new { success = true });
        }

        // Edit sản phẩm với inventory
        public async Task<IActionResult> EditWithInventory(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var categories = await _context.Productcategories.ToListAsync();
            var colors = await _context.Colors.ToListAsync();
            var sizes = await _context.Sizes.ToListAsync();

            var model = new ProductViewModel
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                CategoryId = product.CategoryId ?? 0,
                Description = product.Description,
                Price = product.Productvariants.FirstOrDefault()?.Price ?? 0,
                SizeIds = product.Productvariants.Select(v => v.SizeId ?? 0).Distinct().ToList(),
                ColorIds = product.Productvariants.Select(v => v.ColorId ?? 0).Distinct().ToList(),
                Categories = new SelectList(categories, "CategoryId", "CategoryName"),
                Colors = new SelectList(colors, "ColorId", "ColorName"),
                Sizes = new SelectList(sizes, "SizeId", "SizeName")
            };

            ViewBag.Product = product!; // Pass product data for inventory display
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditWithInventory(ProductViewModel model, Dictionary<string, int> inventoryData)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _context.Productcategories.ToListAsync();
                var colors = await _context.Colors.ToListAsync();
                var sizes = await _context.Sizes.ToListAsync();

                model.Categories = new SelectList(categories, "CategoryId", "CategoryName");
                model.Colors = new SelectList(colors, "ColorId", "ColorName");
                model.Sizes = new SelectList(sizes, "SizeId", "SizeName");

                var product = await _productService.GetProductByIdAsync(model.ProductId);
                ViewBag.Product = product!;
                return View(model);
            }

            // Update product with variants
            await _productService.UpdateProductAsync(model.ProductId, model);

            // Update inventory for all variants
            if (inventoryData != null)
            {
                var product = await _productService.GetProductByIdAsync(model.ProductId);
                foreach (var variant in product.Productvariants)
                {
                    var key = $"variant_{variant.SizeId}_{variant.ColorId}";
                    if (inventoryData.ContainsKey(key))
                    {
                        var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.VariantId == variant.VariantId);
                        if (inventory != null)
                        {
                            inventory.Quantity = inventoryData[key];
                        }
                    }
                }
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Cập nhật sản phẩm và inventory thành công!";
            return RedirectToAction("Index");
        }

        // Edit Product
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var categories = await _context.Productcategories.ToListAsync();
            var colors = await _context.Colors.ToListAsync();
            var sizes = await _context.Sizes.ToListAsync();

            var model = new ProductViewModel
            {
                ProductName = product.ProductName,
                CategoryId = product.CategoryId ?? 0,
                Description = product.Description,
                Price = product.Productvariants?.FirstOrDefault()?.Price ?? 0,
                ColorIds = product.Productvariants?.Where(v => v.ColorId.HasValue).Select(v => v.ColorId!.Value).Distinct().ToList() ?? new(),
                SizeIds = product.Productvariants?.Where(v => v.SizeId.HasValue).Select(v => v.SizeId!.Value).Distinct().ToList() ?? new(),
                Categories = new SelectList(categories, "CategoryId", "CategoryName", product.CategoryId),
                Colors = new SelectList(colors, "ColorId", "ColorName"),
                Sizes = new SelectList(sizes, "SizeId", "SizeName")
            };

            ViewBag.Product = product; // For displaying existing images
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _context.Productcategories.ToListAsync();
                var colors = await _context.Colors.ToListAsync();
                var sizes = await _context.Sizes.ToListAsync();

                model.Categories = new SelectList(categories, "CategoryId", "CategoryName", model.CategoryId);
                model.Colors = new SelectList(colors, "ColorId", "ColorName");
                model.Sizes = new SelectList(sizes, "SizeId", "SizeName");

                var product = await _productService.GetProductByIdAsync(id);
                ViewBag.Product = product;
                return View(model);
            }

            await _productService.UpdateProductAsync(id, model);
            return RedirectToAction("Index");
        }

        // Delete Product
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productService.DeleteProductAsync(id);
            return RedirectToAction("Index");
        }
    }
}