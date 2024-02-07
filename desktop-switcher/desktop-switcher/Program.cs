using DesktopManagerLib;
using StackExchange.Redis;

//ConnectionMultiplexer redis = await ConnectionMultiplexer.ConnectAsync("192.168.1.9");
//IDatabase db = redis.GetDatabase();
////await db.StringSetAsync("llll", "ooooo");
////Console.WriteLine(await db.StringGetAsync("llll"));
//var subscriber = redis.GetSubscriber();
//var rc = new RedisChannel("ccs3/test", RedisChannel.PatternMode.Literal);
//var cmq = await subscriber.SubscribeAsync(rc);
//cmq.OnMessage(msg => {
//    Console.WriteLine(msg);
//});
//var rmsg = new RedisValue("test message");
//await subscriber.PublishAsync(rc,rmsg);


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
