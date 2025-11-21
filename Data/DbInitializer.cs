using Microsoft.AspNetCore.Identity;
using QL_NhaThuoc.Models;

namespace QL_NhaThuoc.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            context.Database.EnsureCreated();

            // Seed Roles
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }
            if (!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(new IdentityRole("User"));
            }

            // Seed Admin User
            if (await userManager.FindByEmailAsync("admin@nhathuoc.com") == null)
            {
                var admin = new User
                {
                    UserName = "admin@nhathuoc.com",
                    Email = "admin@nhathuoc.com",
                    FullName = "Administrator",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(admin, "Admin@123");
                await userManager.AddToRoleAsync(admin, "Admin");
            }

            // Seed Categories
            if (!context.Categories.Any())
            {
                context.Categories.AddRange(
                    new Category { Name = "Thuốc giảm đau", Description = "Các loại thuốc giảm đau" },
                    new Category { Name = "Thuốc cảm cúm", Description = "Thuốc điều trị cảm cúm" },
                    new Category { Name = "Thuốc tiêu hóa", Description = "Thuốc hỗ trợ tiêu hóa" }
                );
                await context.SaveChangesAsync();
            }

            // Seed Countries
            if (!context.Countries.Any())
            {
                context.Countries.AddRange(
                    new Country { Name = "Việt Nam" },
                    new Country { Name = "Hoa Kỳ" },
                    new Country { Name = "Nhật Bản" }
                );
                await context.SaveChangesAsync();
            }

            // Seed Brands
            if (!context.Brands.Any())
            {
                context.Brands.AddRange(
                    new Brand { Name = "Traphaco", Description = "Công ty dược phẩm Traphaco" },
                    new Brand { Name = "Domesco", Description = "Công ty dược phẩm Domesco" }
                );
                await context.SaveChangesAsync();
            }

            // Seed UsageObjects
            if (!context.UsageObjects.Any())
            {
                context.UsageObjects.AddRange(
                    new UsageObject { Name = "Người lớn", Description = "Dành cho người lớn" },
                    new UsageObject { Name = "Trẻ em", Description = "Dành cho trẻ em" }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}
