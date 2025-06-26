// Punto de entrada principal de la aplicación ASP.NET Core
// Configuración minimalista para conectar con Supabase

var builder = WebApplication.CreateBuilder(args);

// Configuración de servicios esenciales
builder.Services.AddControllersWithViews();  // Habilita el patrón MVC
builder.Services.AddHttpClient();            // Registra HttpClient para llamadas a APIs

var app = builder.Build();

// Configuración del pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");  // Manejo de errores en producción
}

// Middleware esencial para la aplicación
app.UseStaticFiles();   // Sirve archivos estáticos (CSS, JS, imágenes)
app.UseRouting();       // Habilita el enrutamiento de URLs

// Configuración de rutas por defecto
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");  // Ruta: /Home/Index

app.Run();  // Inicia la aplicación web