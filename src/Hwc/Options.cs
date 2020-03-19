using System;
using System.IO;
using CommandLine;

namespace Hwc
{
    public class Options
    {
        public Options()
        {

            var userProfileFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            ApplicationInstanceId = Guid.NewGuid().ToString();
            TempDirectory = Path.Combine(userProfileFolder, $"tmp{ApplicationInstanceId}");
            var configDirectory = Path.Combine(TempDirectory, "config");
            ApplicationHostConfigPath = Path.Combine(configDirectory, "ApplicationHost.config");
            WebConfigPath = Path.Combine(configDirectory, "Web.config");
            AspnetConfigPath = Path.Combine(configDirectory, "AspNet.config");
            if (int.TryParse(Environment.GetEnvironmentVariable("PORT"), out var port))
            {
                Port = port;
            }
        }
        [Option("appRootPath", Default = ".", HelpText = "app web root path", Required = false)]
        public string AppRootPath { get; set; } = Environment.CurrentDirectory;

        public string AppRootPathFull => Path.GetFullPath(AppRootPath);

        [Option("port", HelpText = "port for the application to listen with", Required = false)]
        public int Port { get; set; } = 8080;
        public string Protocol { get; set; } = "http";
        public string AspnetConfigPath { get; set; } = string.Empty;
        public string WebConfigPath { get; set; } = string.Empty;
        public string TempDirectory { get; set; } = string.Empty;
        public string ConfigDirectory => Path.Combine(TempDirectory, "config");
        public string ApplicationHostConfigPath { get; set; } = string.Empty;
        public string ApplicationInstanceId { get; set; } = Guid.NewGuid().ToString();
    }
}