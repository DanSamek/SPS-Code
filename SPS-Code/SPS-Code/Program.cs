using Microsoft.EntityFrameworkCore;
using SPS_Code.Data;
using SPS_Code.Data.Models;
using SPS_Code.Helpers;

namespace SPS_Code;
public class Program
{
    public static void Main(string[] args)
    {
        // Every 30 minutes check tmp folder, and 
        Timer timer = new Timer(TmpFolder.DeleteOldFiles, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<CodeDbContext>(opt => opt.UseSqlServer(builder.Configuration["DatabaseConnection"]));

        builder.Services.AddSession(opt =>
        {
            opt.Cookie.Name = "cookie";
            opt.IdleTimeout = TimeSpan.FromHours(8);
            opt.Cookie.HttpOnly = true;
            opt.Cookie.IsEssential = true;
            opt.Cookie.MaxAge = TimeSpan.FromHours(8);
        });

        builder.Services.AddControllersWithViews();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseSession();

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();

    }
}