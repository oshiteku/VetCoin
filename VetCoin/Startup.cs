using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureADB2C.UI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using VetCoin.Data;
using VetCoin.Services;
using VetCoin.Services.Chat;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.Cookies;
using VetCoin.Services.HostedServices;

namespace VetCoin
{
    public class CustomOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.ApiDescription.ActionDescriptor is ControllerActionDescriptor controller)
            {
                operation.OperationId =
                    $"{controller.ActionName}";
            }
        }
    }

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
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));


            services.AddRazorPages()
                .AddRazorPagesOptions(options =>
                {
                    options.Conventions.AllowAnonymousToPage("/Account/Login");
                    options.Conventions.AllowAnonymousToPage("/Index");
                    options.Conventions.AuthorizeFolder("/");

                })
                .AddRazorRuntimeCompilation();

            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
                c.OperationFilter<CustomOperationFilter>();
            });


            services.AddHttpClient();
            services.AddCors();
            services.AddHttpContextAccessor();

            services.AddScoped<CoreService>();
            services.AddScoped<SuperChatService>();
            services.AddScoped<ReactionSendService>();

            services.AddScoped<UrlQueryService>();
            

            services.AddTransient<DiscordService>();
            services.AddTransient<ScheduledExecutionService>();



#if !DEBUG
            services.AddHostedService<Services.HostedServices.DbSeedHostedService>();
            services.AddHostedService<Services.HostedServices.DbMigrationHostedService<Data.ApplicationDbContext>>();
            services.AddHostedService<Services.HostedServices.ScheduledExecutionHostedService<ScheduledExecutionService>>();
            services.AddHostedService<Services.HostedServices.VetCoinBotHostedService>();
#endif


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseCors(builder =>
                 //builder.AllowAnyOrigin()
                 //.AllowAnyMethod()
                 //.AllowAnyHeader()

                 builder.WithOrigins("http://localhost:8080")
                 .AllowAnyHeader()
                .WithMethods("GET", "POST")
                .AllowCredentials()
            );


            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });

            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");


            });


        }
    }
}
