namespace Ccs3ClientApp;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("CCS3 Client App started at {0}", DateTime.Now);
        Console.WriteLine("Username {0}", Environment.UserName);
        while (true) {
            Console.WriteLine(DateTime.Now);
            Thread.Sleep(TimeSpan.FromSeconds(1));
        }
    }
}
