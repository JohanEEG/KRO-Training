using Microsoft.EntityFrameworkCore;
using KROTraining.Models;
// Agregado para el manejo de la sesión y autenticación:
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// --- CONFIGURACIÓN DE LA BASE DE DATOS ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<KroTrainingContext>(options =>
    options.UseSqlServer(connectionString));
// ----------------------------------------

// --- CONFIGURACIÓN DE AUTENTICACIÓN POR COOKIES ---
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Cuenta/Login"; // <-- CORREGIDO: Apunta a CuentaController
        options.ExpireTimeSpan = TimeSpan.FromHours(8); // Tiempo que dura la sesión activa
    });
// -------------------------------------------------

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// --- ORDEN IMPORTANTE: La autenticación va SIEMPRE antes de la autorización ---
app.UseAuthentication();
app.UseAuthorization();
// --------------------------------------------------------------------------

app.MapStaticAssets();

// --- CORREGIDO: La aplicación arranca directo en el Login ---
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Cuenta}/{action=Login}/{id?}")
    .WithStaticAssets();

app.Run();