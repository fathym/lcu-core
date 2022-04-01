using LCU.Hosting;
using LCU.Hosting.Test.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

await LCUHostBuilder<Startup>.StartWebHost(args);

//namespace LCU.Hosting.Test.Web
//{
//    public class Program
//    {
//        public static void Main(string[] args)
//        {
//            CreateHostBuilder(args).Build().Run();
//        }

//        public static IHostBuilder CreateHostBuilder(string[] args) =>
//            Host.CreateDefaultBuilder(args)
//                .ConfigureWebHostDefaults(webBuilder =>
//                {
//                    webBuilder.UseStartup<Startup>();
//                });
//    }
//}
