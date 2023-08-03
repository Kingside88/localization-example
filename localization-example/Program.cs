using localization_example;
using localization_example.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.OpenApi.Models;
using System.Globalization;
using System.Reflection;
using Localization = localization_example.Localization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Localization Example WebApi", Version = "v1", });
    c.OperationFilter<LocalizationHeaderSwaggerAttribute>();
});

// Localization
builder.Services.AddDistributedMemoryCache();
builder.Services.AddTransient<IStringLocalizer, Localization.StringLocalizer<Resource>>();

//var qq = builder.Services.BuildServiceProvider().GetService<IStringLocalizer>();
//var ww = qq.GetAllStrings().ToDictionary(x => x.Name, x => x.Value);

builder.Services.AddLocalization();
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
                    new CultureInfo("en"),
                    new CultureInfo("de")
        };

    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

/* Localization */
string[] supportedCultures = new[] { "en", "de" };

var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

localizationOptions.ApplyCurrentCultureToResponseHeaders = true;

app.UseRequestLocalization(localizationOptions);

// Debug Middleware
app.Use(async (context, next) =>
{
    CultureInfo.CurrentUICulture = CultureInfo.CurrentUICulture;
    CultureInfo.CurrentCulture = CultureInfo.CurrentCulture;
    await next();
});

app.MapGet("/local/all-strings/default", ([FromServices] IStringLocalizer sharedResourceLocalizer) =>
{
    var allStrings = sharedResourceLocalizer.GetAllStrings();
    var dict = allStrings.ToDictionary(x => x.Name, x => x.Value);
    return Results.Ok(dict);
})
.WithName("local");

app.Run();
