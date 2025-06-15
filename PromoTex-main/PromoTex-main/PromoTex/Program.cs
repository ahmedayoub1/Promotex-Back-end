
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PromoTex.Data_Access;
using PromoTex.Models;
using PromoTex.Services;
using PromoTex.Utility;
using System.Threading.Tasks;

namespace PromoTex
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddDbContext<ApplicationDbContext>(
                  options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
                );
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(
                option => option.SignIn.RequireConfirmedAccount = true
                
                )

              .AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
            builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromDays(1);
            });
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IEmailSenderService, SmtpEmailSender>();
            builder.Services.AddScoped<ITemplateRenderer, TemplateRenderer>();
            var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: MyAllowSpecificOrigins,
                                  policy =>
                                  {
                                      policy.WithOrigins("http://127.0.0.1:5500", "http://localhost:5500")
                                      .AllowAnyMethod().AllowAnyHeader().AllowCredentials();
                                  });
            });



            var app = builder.Build();



            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors(MyAllowSpecificOrigins);


            app.UseAuthentication();
            app.UseAuthorization();
            app.UseStatusCodePages();
            app.UseStaticFiles();


            app.MapControllers();

            //app.Run();
            await app.RunAsync();
        }
    }
}
