using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace VirtualStudio.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                HostConfig.CertificateFileLocation = "./Certificate/virtualstudiotest.pfx";
                HostConfig.CertificatePassword = "geheim";
                HostConfig.HttpPort = 5000;
                HostConfig.HttpsPort = 5001;
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureKestrel(options =>
                {
                    var cert = new X509Certificate2(HostConfig.CertificateFileLocation, HostConfig.CertificatePassword, X509KeyStorageFlags.UserKeySet);
                    options.ListenAnyIP(HostConfig.HttpsPort, listenOpt => listenOpt.UseHttps(cert));
                    options.ListenAnyIP(HostConfig.HttpPort);
                });
                webBuilder.UseStartup<Startup>();
            });
    }

    public static class HostConfig
    {
        public static string CertificateFileLocation { get; set; }
        public static string CertificatePassword { get; set; }
        public static int HttpPort { get; set; }
        public static int HttpsPort { get; set; }
    }
}
