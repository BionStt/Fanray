using AutoMapper;
using Fan.Accounts;
using Fan.Accounts.Data;
using Fan.Accounts.Models;
using Fan.Accounts.Services;
using Fan.Blogs.Data;
using Fan.Blogs.Helpers;
using Fan.Blogs.MetaWeblog;
using Fan.Blogs.Services;
using Fan.Data;
using Fan.Emails;
using Fan.Exceptions;
using Fan.Medias;
using Fan.Models;
using Fan.Settings;
using Fan.Shortcodes;
using Fan.Web.Middlewares;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Fan.Web
{
    public class Startup
    {
        private ILogger<Startup> _logger;

        public Startup(IConfiguration configuration, IHostingEnvironment env, ILogger<Startup> logger)
        {
            HostingEnvironment = env;
            Configuration = configuration;
            _logger = logger;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment HostingEnvironment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // Db 
            /**
             * AddDbContextPool is an EF Core 2.0 performance enhancement https://docs.microsoft.com/en-us/ef/core/what-is-new/
             * unfortunately it has limitations and cannot be used here.  
             * 1. It interferes with dbcontext implicit transactions when events are raised and event handlers call SaveChangesAsync
             * 2. Multiple dbcontexts will fail https://github.com/aspnet/EntityFrameworkCore/issues/9433
             * 3. To use AddDbContextPool, FanDbContext can only have a single public constructor accepting a single parameter of type DbContextOptions
             */
            services.AddDbContext<FanDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            // Cors
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
            });

            // Identity
            services.AddIdentity<User, Role>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 4;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
            })
            .AddEntityFrameworkStores<FanDbContext>()
            .AddDefaultTokenProviders();

            // Caching
            services.AddDistributedMemoryCache();

            // Mapper
            services.AddAutoMapper();
            services.AddSingleton(BlogUtil.Mapper);

            // Mediatr
            services.AddMediatR();

            // Repos & Services
            services.AddScoped<IMetaRepository, SqlMetaRepository>();
            services.AddScoped<IMediaRepository, SqlMediaRepository>();
            services.AddScoped<IPostRepository, SqlPostRepository>();
            services.AddScoped<ICategoryRepository, SqlCategoryRepository>();
            services.AddScoped<ITagRepository, SqlTagRepository>();
            services.AddScoped<ITokenRepository, SqlTokenRepository>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<ISettingService, SettingService>();
            services.AddScoped<IMediaService, MediaService>();
            services.AddScoped<IEmailSender, EmailSender>();
            services.AddScoped<IBlogService, BlogService>();
            services.AddScoped<IXmlRpcHelper, XmlRpcHelper>();
            services.AddScoped<IMetaWeblogService, MetaWeblogService>();
            services.AddScoped<IHttpWwwRewriter, HttpWwwRewriter>();
            var appSettingsConfigSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsConfigSection);
            var appSettings = appSettingsConfigSection.Get<AppSettings>();
            if (appSettings.MediaStorageType == EMediaStorageType.AzureBlob)
                services.AddScoped<IStorageProvider, AzureBlobStorageProvider>();
            else
                services.AddScoped<IStorageProvider, FileSysStorageProvider>();
            var shortcodeService = new ShortcodeService();
            shortcodeService.Add<SourceCodeShortcode>(tag: "code");
            shortcodeService.Add<YouTubeShortcode>(tag: "youtube");
            services.AddSingleton<IShortcodeService>(shortcodeService);
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<ApiExceptionFilter>();

            // Bearer appsettings
            services.Configure<BearerTokensOptions>(options => Configuration.GetSection("BearerTokens").Bind(options));
            // Only needed for custom roles.
            services.AddAuthorization(options =>
            {
                options.AddPolicy(SystemRoles.Admininistrator, policy => policy.RequireRole(SystemRoles.Admininistrator));
                options.AddPolicy(SystemRoles.Editor, policy => policy.RequireRole(SystemRoles.Editor));
            });
            // Needed for jwt auth.
            services
                /** 
                 * https://wildermuth.com/2017/08/19/Two-AuthorizationSchemes-in-ASP-NET-Core-2
                 * if don't define Defaults here because we want to have both cookie and token auth, it'll default to cookie, so we need to specify 
                 * "AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme" to each individaul controller that needs bearer token.
                 */
                .AddAuthentication()
                .AddCookie(cfg => cfg.SlidingExpiration = true)
                .AddJwtBearer(cfg =>
                {
                    cfg.RequireHttpsMetadata = false;
                    cfg.SaveToken = true;
                    cfg.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = Configuration["BearerTokens:Issuer"], // site that makes the token
                        ValidateIssuer = true, // TODO: change this to avoid forwarding attacks
                        ValidAudience = Configuration["BearerTokens:Audience"], // site that consumes the token
                        ValidateAudience = true, // TODO: change this to avoid forwarding attacks
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["BearerTokens:Key"])),
                        ValidateIssuerSigningKey = true, // verify signature to avoid tampering
                        ValidateLifetime = true, // validate the expiration
                        ClockSkew = TimeSpan.Zero // tolerance for the expiration date
                    };
                    cfg.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(JwtBearerEvents));
                            logger.LogError("Authentication failed.", context.Exception);
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            var tokenValidatorService = context.HttpContext.RequestServices.GetRequiredService<ITokenService>();
                            return tokenValidatorService.ValidateTokenContextAsync(context);
                        },
                        OnMessageReceived = context =>
                        {
                            return Task.CompletedTask;
                        },
                        OnChallenge = context =>
                        {
                            var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(JwtBearerEvents));
                            logger.LogError("OnChallenge error", context.Error, context.ErrorDescription);
                            return Task.CompletedTask;
                        }
                    };
                });

            // Mvc
            services.AddMvc();

            // AppInsights
            services.AddApplicationInsightsTelemetry(Configuration);

            // Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Fanray API", Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseHsts();
            app.UseHttpWwwRewrite();
            app.MapWhen(context => context.Request.Path.ToString().Equals("/olw"), appBuilder => appBuilder.UseMetablog());
            app.UseStatusCodePagesWithReExecute("/Home/ErrorCode/{0}"); // needs to be after hsts and rewrite
            app.UseStaticFiles();
            app.UseAuthentication(); // UseIdentity is obsolete, UseAuth is recommended
            app.UseMvc(routes => RegisterRoutes(routes, app));
            app.UseSwagger(); // view the generated Swagger JSON at "/swagger/v1/swagger.json"
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fanray API V1");
            }); // interactive docs at "/swagger"

            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var db = serviceScope.ServiceProvider.GetService<FanDbContext>();
                db.Database.Migrate();
            }
        }

        private void RegisterRoutes(IRouteBuilder routes, IApplicationBuilder app)
        {
            routes.MapRoute("Home", "", new { controller = "Blog", action = "Index" });
            routes.MapRoute("Setup", "setup", new { controller = "Home", action = "Setup" });
            routes.MapRoute("Admin", "admin", new { controller = "Home", action = "Admin" });
            routes.MapRoute("About", "about", new { controller = "Home", action = "About" });
            routes.MapRoute("Contact", "contact", new { controller = "Home", action = "Contact" });

            routes.MapRoute("Login", "login", new { controller = "Account", action = "Login" });


            BlogRoutes.RegisterRoutes(routes);

            routes.MapRoute(name: "Default", template: "{controller=Home}/{action=Index}/{id?}");
        }
    }
}
