using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Add MudBlazor services
builder.Services.AddMudServices();


// Add HttpClient for API calls
// ?? IMPORTANT: Point to your LibraryAPI URL

//builder.Services.AddScoped(sp => new HttpClient
//{
//    BaseAddress = new Uri("https://localhost:7163/api/")  
//});

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("https://library-api-app-bth6dbgshuatgbfp.southindia-01.azurewebsites.net/")
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();