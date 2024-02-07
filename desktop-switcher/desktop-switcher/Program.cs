using SwitchToDefaultDesktop;

var builder = WebApplication.CreateBuilder(args);

DesktopManager desktopManager = new();
bool securedDesktopCreated = desktopManager.CreateSecureDesktop("CCS3-Secured");

// Add services to the container.
const string AllowAllOriginsPolicyName = "allow-all-origins-policy";

builder.Services.AddControllers();
builder.Services.AddSingleton(desktopManager);
builder.Services.AddCors(options => {
    options.AddPolicy(AllowAllOriginsPolicyName, policy => {
        policy.AllowAnyOrigin();
        policy.AllowAnyMethod();
        policy.AllowAnyHeader();
        policy.SetPreflightMaxAge(TimeSpan.FromMinutes(30));
    });
});

var app = builder.Build();


// Configure the HTTP request pipeline.

app.UseCors(AllowAllOriginsPolicyName);

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
