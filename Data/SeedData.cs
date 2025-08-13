using Microsoft.AspNetCore.Identity;

namespace GestionAnticiposApp.Data
{
    public static class SeedData
    {
        private static readonly string[] roles = new[] { "Admin", "Usuario", "Aprobador", "Lector" };

        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // 1. Crear todos los roles si no existen
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Crear usuario administrador por defecto si no existe
            string adminEmail = "admin@dominio.com";
            string adminPassword = "Admin123*"; // cámbiala después de iniciar

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true // evitar confirmación manual
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (!result.Succeeded)
                {
                    throw new Exception("Error creando el usuario admin: " +
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }

            // 3. Asegurar que el usuario esté en el rol Admin
            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}