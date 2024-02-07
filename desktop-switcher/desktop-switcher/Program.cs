using SwitchToDefaultDesktop;

var builder = WebApplication.CreateBuilder(args);

DesktopManager desktopManager = new();
bool securedDesktopCreated = desktopManager.CreateSecureDesktop("CCS3-Secured");

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSingleton(desktopManager);

var app = builder.Build();


// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors();

app.MapControllers();

app.Run();
