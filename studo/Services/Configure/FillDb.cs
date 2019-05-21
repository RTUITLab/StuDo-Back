using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using studo.Data;
using studo.Models;
using studo.Models.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Configure.Models.Configure.Interfaces;

namespace studo.Services.Configure
{
    public class FillDb : IConfigureWork
    {
        private readonly IOptions<FillDbOptions> options;
        private readonly ILogger<FillDb> logger;
        private readonly UserManager<User> userManager;
        private readonly RoleManager<Role> roleManager;

        public FillDb(IOptions<FillDbOptions> options, ILogger<FillDb> logger,
            UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            this.options = options;
            this.logger = logger;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        public async Task Configure()
        {
            await AddDefaultUser();
            await AddRoles();
            await AddRolesToDefaultUser();
        }

        private async Task AddDefaultUser()
        {
            if (await userManager.FindByEmailAsync(options.Value.Email) != null)
            {
                logger.LogWarning($"User with '{options.Value.Email}' is already exists");
                return;
            }

            var result = await userManager.CreateAsync(new User
            {
                Firstname = options.Value.Firstname,
                Surname = options.Value.Surname,
                Email = options.Value.Email,
                UserName = options.Value.Email,
                EmailConfirmed = true
            }, options.Value.Password);
            logger.LogDebug($"Result of creating default user '{options.Value.Email}' is '{result}'");
        }

        private async Task AddRoles()
        {
            // Db has roles
            if (roleManager.Roles.Any())
            {
                string roles = "";
                foreach (var role in roleManager.Roles)
                    roles += role + ", ";
                logger.LogWarning($"Database has roles: {roles}");
                return;
            }

            var admin = await roleManager.CreateAsync(
                new Role
                {
                    Name = RolesConstants.Admin
                });
            var user = await roleManager.CreateAsync(
                new Role
                {
                    Name = RolesConstants.User
                });
            logger.LogDebug($"Result of creating roles: {RolesConstants.Admin} - {admin}, {RolesConstants.User} - {user}");
        }

        private async Task AddRolesToDefaultUser()
        {
            var user = await userManager.FindByEmailAsync(options.Value.Email);
            if (user == null)
            {
                logger.LogError($"Can't find user with email: '{options.Value.Email}'");
                return;
            }

            var userRoles = await userManager.GetRolesAsync(user);
            if (userRoles.Count != 0)
            {
                string roles = "";
                foreach (var role in userRoles)
                    roles += role + ", ";
                logger.LogWarning($"User '{options.Value.Email}' has roles: '{roles}'");
                return;
            }

            var adminRole = await roleManager.FindByNameAsync(RolesConstants.Admin);
            await userManager.AddToRoleAsync(user, adminRole.Name);
            logger.LogDebug($"To '{user.Email}' was added '{adminRole.Name}' role");
        }
    }
}
