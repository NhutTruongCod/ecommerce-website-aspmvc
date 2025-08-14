using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using webbanhang.Models;
using webbanhang.Data;
using webbanhang.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace webbanhang.Controllers
{
    public class AccountController : Controller
    {
        private readonly MyDbContext _context;

        public AccountController(MyDbContext context)
        {
            _context = context;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.Password != model.ConfirmPassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp";
                return View(model);
            }

            if (_context.Users.Any(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email đã được sử dụng");
                return View(model);
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

            var user = new User
            {
                FullName = model.FullName,
                Email = model.Email,
                PasswordHash = passwordHash,
                Phone = model.Phone,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không đúng");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.RoleId == 1 ? "Admin" : "User")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            if (user.RoleId == 1)
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            else
                return RedirectToAction("Index", "Home");
        }

        [Authorize(Roles = "User")]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return RedirectToAction("Login");
            var model = new webbanhang.ViewModels.ProfileViewModel
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                CreatedAt = user.CreatedAt
            };
            return View(model);
        }

        [Authorize(Roles = "User")]
        [HttpPost]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return RedirectToAction("Login");

            if (!ModelState.IsValid)
            {
                // Giữ lại các trường không nhập lại được
                model.Email = user.Email;
                model.CreatedAt = user.CreatedAt;
                return View(model);
            }

            // Cập nhật thông tin cá nhân
            user.FullName = model.FullName;
            user.Phone = model.Phone;
            user.Address = model.Address;

            // Xử lý đổi mật khẩu nếu có nhập
            if (!string.IsNullOrEmpty(model.CurrentPassword) || !string.IsNullOrEmpty(model.NewPassword) || !string.IsNullOrEmpty(model.ConfirmNewPassword))
            {
                if (string.IsNullOrEmpty(model.CurrentPassword) || string.IsNullOrEmpty(model.NewPassword) || string.IsNullOrEmpty(model.ConfirmNewPassword))
                {
                    ModelState.AddModelError("CurrentPassword", "Vui lòng nhập đầy đủ thông tin đổi mật khẩu.");
                    model.Email = user.Email;
                    model.CreatedAt = user.CreatedAt;
                    return View(model);
                }
                if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash))
                {
                    ModelState.AddModelError("CurrentPassword", "Mật khẩu hiện tại không đúng.");
                    model.Email = user.Email;
                    model.CreatedAt = user.CreatedAt;
                    return View(model);
                }
                if (model.NewPassword != model.ConfirmNewPassword)
                {
                    ModelState.AddModelError("NewPassword", "Mật khẩu mới và xác nhận không khớp.");
                    model.Email = user.Email;
                    model.CreatedAt = user.CreatedAt;
                    return View(model);
                }
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            }

            _context.Update(user);
            await _context.SaveChangesAsync();
            TempData["Message"] = "Cập nhật thông tin thành công!";
            return RedirectToAction("Profile");
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["Success"] = "Đăng xuất thành công!";
            return RedirectToAction("Login");
        }
    }
}