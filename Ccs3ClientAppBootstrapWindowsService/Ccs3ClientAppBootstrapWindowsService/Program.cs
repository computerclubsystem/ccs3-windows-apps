namespace Ccs3ClientAppBootstrapWindowsService
{
    public class Program {
        public static void Main(string[] args) {
            var builder = CreateAppBuilder(args);
            var host = builder.Build();
            Directory.SetCurrentDirectory(builder.Environment.ContentRootPath);
            host.Run();
        }

        private static HostApplicationBuilder CreateAppBuilder(string[] args) {
            HostApplicationBuilderSettings settings = new() {
                Args = args,
                ContentRootPath = AppContext.BaseDirectory,
            };
            var builder = Host.CreateApplicationBuilder(settings);

            builder.Services.AddWindowsService(options => {
                options.ServiceName = "Ccs3ClientAppBootstrapWindowsService";
            });
            builder.Services.AddSingleton<HttpDownloader>();
            builder.Services.AddHostedService<Worker>();
            return builder;
        }
    }
}