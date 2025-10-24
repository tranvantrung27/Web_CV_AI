using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CV_AI.Models;

namespace CV_AI.Data
{
    public static class AdminSeeder
    {
        public static async Task SeedAdminUser(UserManager<User> userManager, ApplicationDbContext context)
        {
            try
            {
                var adminEmail = "admin@gmail.com";
                var adminExists = await userManager.FindByEmailAsync(adminEmail);
                
                if (adminExists == null)
                {
                    Console.WriteLine("🔄 Đang tạo tài khoản Admin...");
                    
                    var adminUser = new User
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        FullName = "Administrator",
                        Role = "Admin",
                        IsActive = true,
                        EmailConfirmed = true,
                        NormalizedUserName = adminEmail.ToUpper(),
                        NormalizedEmail = adminEmail.ToUpper(),
                        SecurityStamp = Guid.NewGuid().ToString()
                    };

                    var result = await userManager.CreateAsync(adminUser, "Admin@123");

                    if (result.Succeeded)
                    {
                        Console.WriteLine("════════════════════════════════════════════════");
                        Console.WriteLine("✅ TÀI KHOẢN ADMIN ĐÃ ĐƯỢC TẠO THÀNH CÔNG!");
                        Console.WriteLine("════════════════════════════════════════════════");
                        Console.WriteLine($"   📧 Email:    {adminEmail}");
                        Console.WriteLine($"   🔑 Password: Admin@123");
                        Console.WriteLine($"   👤 Role:     Admin");
                        Console.WriteLine($"   ✔️  Active:   true");
                        Console.WriteLine("════════════════════════════════════════════════");
                        Console.WriteLine("⚠️  LƯU Ý: Hãy đổi mật khẩu sau lần đăng nhập đầu!");
                        Console.WriteLine("════════════════════════════════════════════════");
                    }
                    else
                    {
                        Console.WriteLine("❌ LỖI KHI TẠO TÀI KHOẢN ADMIN:");
                        foreach (var error in result.Errors)
                        {
                            Console.WriteLine($"   - {error.Code}: {error.Description}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("════════════════════════════════════════════════");
                    Console.WriteLine("ℹ️  Tài khoản Admin đã tồn tại");
                    Console.WriteLine($"   📧 Email: {adminEmail}");
                    Console.WriteLine($"   👤 Role: {adminExists.Role}");
                    Console.WriteLine($"   ✔️  Active: {adminExists.IsActive}");
                    Console.WriteLine("════════════════════════════════════════════════");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ EXCEPTION khi seed Admin: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}

