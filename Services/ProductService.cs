using webbanhang.ViewModels;
using webbanhang.Models;
using webbanhang.Data;
using Microsoft.EntityFrameworkCore;

namespace webbanhang.Services;

public interface IProductService
{
    Task<List<Product>> GetAllProductsAsync();
    Task<Product?> GetProductByIdAsync(int productId);
    Task<int> CreateProductAsync(ProductViewModel model);
    Task UpdateProductAsync(int productId, ProductViewModel model);
    Task DeleteProductAsync(int productId);
}

public class ProductService : IProductService
{
    private readonly MyDbContext _context;
    private readonly IWebHostEnvironment _env;

    public ProductService(MyDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    public async Task<List<Product>> GetAllProductsAsync()
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Productimages)
            .Include(p => p.Productvariants)
                .ThenInclude(v => v.Color)
            .Include(p => p.Productvariants)
                .ThenInclude(v => v.Size)
            .Include(p => p.Productvariants)
                .ThenInclude(v => v.Inventories)
            .ToListAsync();
    }

    public async Task<Product?> GetProductByIdAsync(int productId)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Productimages)
            .Include(p => p.Productvariants)
                .ThenInclude(v => v.Color)
            .Include(p => p.Productvariants)
                .ThenInclude(v => v.Size)
            .Include(p => p.Productvariants)
                .ThenInclude(v => v.Inventories)
            .FirstOrDefaultAsync(p => p.ProductId == productId);
    }

    public async Task<int> CreateProductAsync(ProductViewModel model)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. Thêm sản phẩm
            var product = new Product
            {
                ProductName = model.ProductName ?? string.Empty,
                CategoryId = model.CategoryId,
                Description = model.Description ?? string.Empty
            };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // 2. Thêm variants
            foreach (var sizeId in model.SizeIds)
            {
                foreach (var colorId in model.ColorIds)
                {
                    // Lấy thông tin Size và Color để tạo SKU
                    var size = await _context.Sizes.FindAsync(sizeId);
                    var color = await _context.Colors.FindAsync(colorId);

                    var variant = new Productvariant
                    {
                        ProductId = product.ProductId,
                        SizeId = sizeId,
                        ColorId = colorId,
                        Price = model.Price,
                        Sku = $"{product.ProductName?.Replace(" ", "")}-{size?.SizeName}-{color?.ColorName}"
                    };
                    _context.Productvariants.Add(variant);
                    await _context.SaveChangesAsync();

                    // 3. Tồn kho mặc định = 0
                    _context.Inventories.Add(new Inventory
                    {
                        VariantId = variant.VariantId,
                        Quantity = 0
                    });
                }
            }
            await _context.SaveChangesAsync();

            // 4. Upload ảnh
            if (model.Images != null && model.Images.Any())
            {
                foreach (var image in model.Images)
                {
                    if (image != null && image.Length > 0)
                    {
                        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                        var path = Path.Combine(_env.WebRootPath, "uploads", fileName);

                        // Tạo thư mục nếu chưa có
                        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }

                        _context.Productimages.Add(new Productimage
                        {
                            ProductId = product.ProductId,
                            ImageUrl = "/uploads/" + fileName,
                            IsThumbnail = false
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return product.ProductId;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateProductAsync(int productId, ProductViewModel model)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var product = await _context.Products
                .Include(p => p.Productvariants)
                    .ThenInclude(v => v.Inventories)
                .Include(p => p.Productimages)
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null) throw new Exception("Sản phẩm không tồn tại");

            // Cập nhật thông tin sản phẩm
            product.ProductName = model.ProductName ?? string.Empty;
            product.CategoryId = model.CategoryId;
            product.Description = model.Description ?? string.Empty;

            // Chỉ thêm những variants mới, giữ nguyên variants cũ với inventory
            foreach (var sizeId in model.SizeIds)
            {
                foreach (var colorId in model.ColorIds)
                {
                    // Kiểm tra xem variant này đã tồn tại chưa
                    var existingVariant = product.Productvariants
                        .FirstOrDefault(v => v.SizeId == sizeId && v.ColorId == colorId);

                    if (existingVariant == null)
                    {
                        // Lấy thông tin Size và Color để tạo SKU
                        var size = await _context.Sizes.FindAsync(sizeId);
                        var color = await _context.Colors.FindAsync(colorId);

                        var variant = new Productvariant
                        {
                            ProductId = product.ProductId,
                            SizeId = sizeId,
                            ColorId = colorId,
                            Price = model.Price,
                            Sku = $"{product.ProductName?.Replace(" ", "")}-{size?.SizeName}-{color?.ColorName}"
                        };
                        _context.Productvariants.Add(variant);
                        await _context.SaveChangesAsync();

                        // Tồn kho mặc định = 0 cho variants mới
                        _context.Inventories.Add(new Inventory
                        {
                            VariantId = variant.VariantId,
                            Quantity = 0
                        });
                    }
                    else
                    {
                        // Cập nhật giá cho variant đã tồn tại
                        existingVariant.Price = model.Price;
                    }
                }
            }

            // Xóa những variants không còn được chọn
            var variantsToRemove = product.Productvariants
                .Where(v => !model.SizeIds.Contains(v.SizeId ?? 0) || !model.ColorIds.Contains(v.ColorId ?? 0))
                .ToList();

            if (variantsToRemove.Any())
            {
                // Xóa inventory trước khi xóa variants
                var inventoriesToRemove = variantsToRemove
                    .SelectMany(v => _context.Inventories.Where(i => i.VariantId == v.VariantId))
                    .ToList();
                _context.Inventories.RemoveRange(inventoriesToRemove);
                _context.Productvariants.RemoveRange(variantsToRemove);
            }

            // Upload ảnh mới nếu có
            if (model.Images != null && model.Images.Any())
            {
                foreach (var image in model.Images)
                {
                    if (image != null && image.Length > 0)
                    {
                        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                        var path = Path.Combine(_env.WebRootPath, "uploads", fileName);

                        // Tạo thư mục nếu chưa có
                        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }

                        _context.Productimages.Add(new Productimage
                        {
                            ProductId = product.ProductId,
                            ImageUrl = "/uploads/" + fileName,
                            IsThumbnail = false
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task DeleteProductAsync(int productId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var product = await _context.Products
                .Include(p => p.Productimages)
                .Include(p => p.Productvariants)
                    .ThenInclude(v => v.Inventories)
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null)
            {
                throw new Exception("Không tìm thấy sản phẩm");
            }

            // Xóa inventory trước
            foreach (var variant in product.Productvariants)
            {
                if (variant.Inventories != null && variant.Inventories.Any())
                {
                    _context.Inventories.RemoveRange(variant.Inventories);
                }
            }
            await _context.SaveChangesAsync();

            // Xóa variants
            if (product.Productvariants != null && product.Productvariants.Any())
            {
                _context.Productvariants.RemoveRange(product.Productvariants);
                await _context.SaveChangesAsync();
            }

            // Xóa ảnh vật lý và entity
            if (product.Productimages != null && product.Productimages.Any())
            {
                foreach (var img in product.Productimages)
                {
                    var filePath = Path.Combine(_env.WebRootPath, "uploads", img.ImageUrl.TrimStart('/').Split('/').Last());
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
                _context.Productimages.RemoveRange(product.Productimages);
                await _context.SaveChangesAsync();
            }

            // Cuối cùng xóa product
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
