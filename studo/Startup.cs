using System;
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
using Swashbuckle.AspNetCore.Swagger;
using studo.Filters;
using studo.Middlewares;
using studo.Services.Logs;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using RTUITLab.EmailService.Client;

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
            services.Configure<EmailSenderOptionsExtended>(Configuration.GetSection(nameof(EmailSenderOptionsExtended)));
            services.Configure<LogsOptions>(Configuration.GetSection(nameof(LogsOptions)));

            // cookie configuration
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            // JWT configuration
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
                        // do we need to validate time of existence
                        ValidateLifetime = true,

                        // setting security key
                        IssuerSigningKey = jwtOptions.SymmetricSecurityKey,
                        // validation of security key
                        ValidateIssuerSigningKey = true,

                        RequireExpirationTime = true,
                    };
                });

            // Add email service
            services.AddEmailSender(Configuration
                .GetSection(nameof(EmailSenderOptionsExtended))
                .Get<EmailSenderOptionsExtended>());
            // Add http client factory
            services.AddHttpClient<Services.Interfaces.IEmailSender, EmailSender>();

            // Add transients for interfaces
            services.AddTransient<IJwtFactory, JwtFactory>();
            services.AddTransient<IAdManager, AdManager>();
            services.AddTransient<IOrganizationManager, OrganizationManager>();
            services.AddTransient<Services.Interfaces.IEmailSender, EmailSender>();
            services.AddSingleton<ILogsWebSocketHandler>(LogsWebSocketHandler.Instance);

            // add database context
            string connection = Configuration.GetConnectionString("PostgreSQL");
            services.AddEntityFrameworkNpgsql()
                    .AddDbContext<DatabaseContext>(options =>
                        options.UseNpgsql(connection));
            //string connection = Configuration.GetConnectionString("DefaultConnection");
            //services.AddDbContext<DatabaseContext>(options =>
            //    options.UseSqlServer(connection));

            // add connection between Users and Roles
            services.AddIdentity<User, Role>(identityOptions =>
                {
                    // configure identity options
                    identityOptions.Password.RequireDigit = false;
                    identityOptions.Password.RequireLowercase = false;
                    identityOptions.Password.RequireUppercase = false;
                    identityOptions.Password.RequireNonAlphanumeric = false;
                    identityOptions.Password.RequiredLength = 6;

                    identityOptions.SignIn.RequireConfirmedEmail = true;

                    identityOptions.User.AllowedUserNameCharacters =
                        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_.@+";

                    identityOptions.User.RequireUniqueEmail = true;

                    identityOptions.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromSeconds(0);
                    identityOptions.Lockout.MaxFailedAccessAttempts = 100;
                })
                .AddEntityFrameworkStores<DatabaseContext>()
                .AddDefaultTokenProviders();

            // add cookie authentication
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    // Cookie settings
                    options.Cookie.HttpOnly = true;
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(5);

                    options.LoginPath = "/Authentication/SignIn";
                    //options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                    options.SlidingExpiration = true;
                    options.Events.OnRedirectToLogin = (context) =>
                        {
                            context.Response.StatusCode = 401;
                            return Task.CompletedTask;
                        };

                    options.Events.OnRedirectToAccessDenied = (context) =>
                        {
                            context.Response.StatusCode = 403;
                            return Task.CompletedTask;
                        };
                });

            // Cors
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
            });

            // swagger configuration
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1",
                    new Info
                    {
                        Title = "StuDo develop API",
                        Version = "v1"
                    });

                // Set comments path for Swagger JSON and UI
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            services.AddWebAppConfigure()
                .AddTransientConfigure<FillDb>(Configuration.GetValue<bool>("FILL_DB"))
                .AddTransientConfigure<ApplyMigration>(Configuration.GetValue<bool>("MIGRATE"));

            // it'll automatically scan for classes which inherit from "Profile"
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            // front files
            services.AddSpaStaticFiles(spa => spa.RootPath = "wwwroot");

            services.AddMvc(options =>
            {
                options.Filters.Add<ValidateModelAttribute>();
                options.Filters.Add<IgnoreAntiforgeryTokenAttribute>(1001);
                var policy = new AuthorizationPolicyBuilder()
                     .RequireAuthenticatedUser()
                     .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme, CookieAuthenticationDefaults.AuthenticationScheme)
                     .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
                options.MaxModelValidationErrors = 50;
            })
                .AddJsonOptions(opt =>
                {
                    opt.SerializerSettings.DateFormatString = "yyyy-MM-ddTHH:mm:ss.fff";
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
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

            app.UseCors("CorsPolicy");

            app.UseSwagger(c => { c.RouteTemplate = "api/{documentName}/swagger.json"; });
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/api/v1/swagger.json", "My API V1");
                c.RoutePrefix = "api";
            });

            app.UseWebSockets();

            var logsOptions = Configuration.GetSection(nameof(LogsOptions)).Get<LogsOptions>();
            app.UseLogsMiddleware("/api/logsStream", logsOptions.SecretKey);

            app.UseWebAppConfigure(); // locks the app, while functions isn't completed

            app.UseStaticFiles();
            app.UseAuthentication();

            app.UseCookiePolicy();
            app.UseMvc();

            app.UseSpaStaticFiles();
            app.UseSpa(spa => { });
        }
    }
}
