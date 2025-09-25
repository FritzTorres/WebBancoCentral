using Microsoft.EntityFrameworkCore;
using BancoCentralWeb.Data;
using BancoCentralWeb.Data.Repositories;
using BancoCentralWeb.Services;
using System.IO;
using Microsoft.AspNetCore.DataProtection;
using BancoCentralWeb.Filters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Configurar NewtonsoftJson para manejo de JSON
builder.Services.AddControllersWithViews()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    });

// Configurar HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Configurar HttpClientFactory para llamadas API
builder.Services.AddHttpClient();

// Configurar Entity Framework Core y base de datos
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar Data Protection para persistir claves (comentado temporalmente si hay problemas)
// builder.Services.AddDataProtection()
//     .PersistKeysToFileSystem(new DirectoryInfo("DataProtection-Keys"))
//     .SetApplicationName("BancoCentralWeb");

// Registrar repositorios
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<ICuentaRepository, CuentaRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<ISesionRepository, SesionRepository>();

// Registrar servicios
builder.Services.AddScoped<IApiService, ApiService>();
builder.Services.AddScoped<IAuthService>(provider => 
    new AuthService(
        provider.GetRequiredService<IApiService>(),
        provider.GetRequiredService<IConfiguration>(),
        provider.GetRequiredService<IHttpClientFactory>()
    )
);
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<ICuentaService, CuentaService>();
builder.Services.AddScoped<ITransaccionService, TransaccionService>();
builder.Services.AddScoped<IInstitucionService, InstitucionService>();
builder.Services.AddScoped<ICertificadoService, CertificadoService>();

// Registrar el filtro de autorización de sesión
builder.Services.AddTransient<SessionAuthorizeAttribute>();

// Configurar sesión
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
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