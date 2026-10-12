using Kestrel.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Kestrel.Web.Data;

public static class SeedData
{
    public static async Task RunAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        await db.Database.MigrateAsync();

        // admin user
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        if (!await roles.RoleExistsAsync("Admin"))
            await roles.CreateAsync(new IdentityRole("Admin"));

        var email = config["SeedAdmin:Email"];
        var pass = config["SeedAdmin:Password"];
        if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(pass) && await users.FindByEmailAsync(email) == null)
        {
            var admin = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
            if ((await users.CreateAsync(admin, pass)).Succeeded)
                await users.AddToRoleAsync(admin, "Admin");
        }

        if (await db.Categories.AnyAsync()) return;

        var residential = new Category { Name = "Residential" };
        var commercial = new Category { Name = "Commercial" };
        var infra = new Category { Name = "Infrastructure" };
        var industrial = new Category { Name = "Industrial" };
        var renovation = new Category { Name = "Renovation" };
        db.Categories.AddRange(residential, commercial, infra, industrial, renovation);

        db.Projects.AddRange(
            new Project
            {
                Title = "Marlow Court Apartments", Location = "Leeds, UK", Year = 2023, ClientName = "Marlow Homes",
                AreaM2 = 14200, Category = residential, IsFeatured = true,
                Description = "Structural design for a 9-storey reinforced concrete apartment block with a shared podium car park. We worked with the architect to cut column count on the ground floor without increasing slab depth."
            },
            new Project
            {
                Title = "Eastgate Retail Hub", Location = "Rotterdam, NL", Year = 2022, ClientName = "Eastgate Developments",
                AreaM2 = 8600, Category = commercial, IsFeatured = true,
                Description = "Steel frame retail building with long-span roof trusses over the central atrium. Site supervision and structural inspections during the whole build."
            },
            new Project
            {
                Title = "Riverside Footbridge", Location = "Cork, IE", Year = 2021, ClientName = "Cork City Council",
                AreaM2 = null, Category = infra, IsFeatured = true,
                Description = "42 m pedestrian and cycle bridge. Design covered foundations on soft ground, the steel deck and a vibration check against pedestrian loading."
            },
            new Project
            {
                Title = "Hallam Cold Storage", Location = "Sheffield, UK", Year = 2022, ClientName = "Hallam Logistics",
                AreaM2 = 5400, Category = industrial,
                Description = "Warehouse extension with insulated floor slab and racking loads up to 2.5 t per bay."
            },
            new Project
            {
                Title = "Old Mill Conversion", Location = "Bergen, NO", Year = 2020, ClientName = "Private client",
                AreaM2 = 2300, Category = renovation,
                Description = "Condition survey and strengthening of a 19th century masonry mill converted into offices. Timber floors were replaced with a lighter composite system."
            },
            new Project
            {
                Title = "Quayside Parking Structure", Location = "Gdansk, PL", Year = 2024, ClientName = "Quayside Estates",
                AreaM2 = 11800, Category = infra,
                Description = "Precast concrete car park for 480 vehicles. We checked the design for chloride exposure because of the nearby harbour."
            });

        await db.SaveChangesAsync();
    }
}
