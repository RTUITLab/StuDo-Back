using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using studo.Data;
using studo.Models;
using studo.Models.Options;
using studo.Services.Configure;
using WebApp.Configure.Models;
using WebApp.Configure.Models.Invokations;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using studo.Services.Autorize;
using studo.Services;
using studo.Services.Interfaces;

namespace studo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // add options to project
            services.Configure<FillDbOptions>(Configuration.GetSection(nameof(FillDbOptions)));
            services.Configure<JwtOptions>(Configuration.GetSection(nameof(JwtOptions)));

            // 
            var jwtOptions = Configuration.GetSection(nameof(JwtOptions)).Get<JwtOptions>();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        // do we need to validate publisher while token is validating
                        ValidateIssuer = true,
                        // publisher
                        ValidIssuer = jwtOptions.Issuer,

                        // do we need to validate consumer
                        ValidateAudience = true,
                        // setting consumer token
                        ValidAudience = jwtOptions.Audience,
                        // будет ли валидироваться время существования
                        ValidateLifetime = true,

                        // setting security key
                        IssuerSigningKey = jwtOptions.SymmetricSecurityKey,
                        // validation of security key
                        ValidateIssuerSigningKey = true,
                    };
                });

            services.AddTransient<IJwtFactory, JwtFactory>();
            services.AddTransient<IAdManager, AdManager>();

            // add database context
            string connection = Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<DatabaseContext>(options =>
                options.UseSqlServer(connection));

            // add connection between Users and Roles
            services.AddIdentity<User, Role>(identityOptions =>
                {
                    // configure identity options
                    identityOptions.Password.RequireDigit = false;
                    identityOptions.Password.RequireLowercase = false;
                    identityOptions.Password.RequireUppercase = false;
                    identityOptions.Password.RequireNonAlphanumeric = false;
                    identityOptions.Password.RequiredLength = 6;
                })
                .AddEntityFrameworkStores<DatabaseContext>()
                .AddDefaultTokenProviders();

            services.AddWebAppConfigure()
                .AddTransientConfigure<FillDb>(Configuration.GetValue<bool>("FILL_DB"));

            // it'll automatically scan for classes which inherit from "Profile"
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddMvc(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                     .RequireAuthenticatedUser()
                     .AddAuthenticationSchemes("Bearer")
                     .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseWebAppConfigure(); // locks the app, while function isn't completed
            app.UseAuthentication();

            app.UseStaticFiles();

            app.UseMvc();
        }
    }
}
