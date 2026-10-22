using System.Globalization;
using Hotel.Web.Models;
using Hotel.Web.Repositorios;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    // Mensajes de enlace de datos en español
    var mensajes = options.ModelBindingMessageProvider;
    mensajes.SetValueMustNotBeNullAccessor(_ => "Este campo es obligatorio.");
    mensajes.SetAttemptedValueIsInvalidAccessor((valor, campo) => $"El valor '{valor}' no es válido.");
    mensajes.SetValueIsInvalidAccessor(valor => $"El valor '{valor}' no es válido.");
    mensajes.SetMissingBindRequiredValueAccessor(campo => "Este campo es obligatorio.");
});

builder.Services.AddDbContext<HotelContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("HotelDB")));

builder.Services.AddScoped<HabitacionRepositorio>();
builder.Services.AddScoped<HuespedRepositorio>();
builder.Services.AddScoped<ReservaRepositorio>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Cultura es-PE: fechas dd/MM/yyyy y montos con formato local
var cultura = new CultureInfo("es-PE");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(cultura),
    SupportedCultures = [cultura],
    SupportedUICultures = [cultura]
});

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
