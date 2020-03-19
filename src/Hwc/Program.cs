using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml.Linq;
using System.Xml.XPath;
using CommandLine;
using DotLiquid;

namespace Hwc
{
    class Program
    {
        private static Options _options = new Options();

        private static readonly ManualResetEvent _exitWaitHandle = new ManualResetEvent(false);

        static int Main(string[] args)
        {
            var helpWriter = new StringWriter ();
            var parser = new Parser (with => with.HelpWriter = helpWriter);
            parser.ParseArguments<Options> (args)
                .WithParsed(x => _options = x)
                .WithNotParsed (DisplayHelp);
            
            void DisplayHelp (IEnumerable<Error> errs) 
            {
                if (errs.IsVersion() || errs.IsHelp())
                {
                    Console.WriteLine(helpWriter.ToString());
                    Environment.Exit(0);
                }
                else
                {
                    Console.Error.WriteLine(helpWriter.ToString());
                    Environment.Exit(1);
                }
            }
            SystemEvents.SetConsoleEventHandler(ConsoleEventCallback);
            
            try
            {
                var appConfigTemplate = Template.Parse(ReadResource("ApplicationHostConfig.liquid"));
                var appConfigText = appConfigTemplate.Render(Hash.FromAnonymousObject(_options));
                ValidateRequiredDllDependencies(appConfigText);
                var webConfigText = Template.Parse(ReadResource("WebConfig.liquid")).Render(Hash.FromAnonymousObject(_options));
                var aspNetText = Template.Parse(ReadResource("AspNetConfig.liquid")).Render(Hash.FromAnonymousObject(_options));

                Directory.CreateDirectory(_options.TempDirectory);
                Directory.CreateDirectory(_options.ConfigDirectory);
                File.WriteAllText(_options.ApplicationHostConfigPath, appConfigText);
                File.WriteAllText(_options.WebConfigPath, webConfigText);
                File.WriteAllText(_options.AspnetConfigPath, aspNetText);

                

                // Console.WriteLine("Activating HWC with following settings:");
                try
                {
                    // Console.WriteLine($"ApplicationHost.config: {_options.ApplicationHostConfigPath}");
                    // Console.WriteLine($"Web.config: {_options.WebConfigPath}");
                    // Console.WriteLine($"App folder: {_options.AppRootPath}");
                    HostableWebCore.Activate(_options.ApplicationHostConfigPath, _options.WebConfigPath, _options.ApplicationInstanceId);
                }
                catch (UnauthorizedAccessException)
                {
                    Console.Error.WriteLine("Access denied starting hostable web core. Start the application as administrator");
                    Console.WriteLine("===========================");
                    throw;
                }


                Console.WriteLine($"Server ID {_options.ApplicationInstanceId} started");
                Console.WriteLine($"Server root set to {_options.AppRootPath}");
                // we gonna read on different thread here because Console.ReadLine is not the only way the program can end
                // we're also listening to the system events where the app is ordered to shutdown. exitWaitHandle is used to
                // hook up both of these events
                
                new Thread(() =>
                {
                    Console.ReadLine();
                    _exitWaitHandle.Set();
                }).Start();
                _exitWaitHandle.WaitOne();
                return 0;
            }

            catch (ValidationException ve)
            {
                Console.Error.WriteLine(ve.Message);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
            finally
            {
                Shutdown();
            }
            return 1;
        }

        public static string ReadResource(string name)
        {
            // Determine path
            var assembly = Assembly.GetExecutingAssembly();
            // Format: "{Namespace}.{Folder}.{filename}.{Extension}"
            var resourcePath = assembly.GetManifestResourceNames()
                .Single(str => str.EndsWith(name));

            using (Stream stream = assembly.GetManifestResourceStream(resourcePath))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
        private static void Shutdown()
        {
            
                try
                {
                    HostableWebCore.Shutdown(true);
                   
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Hostable webcore didn't shut down cleanly:");
                    Console.Error.WriteLine(ex);
                }
                if (Directory.Exists(_options.TempDirectory))
                {
                    for (var i = 0; i < 5; i++)
                    {
                        try
                        {
                            Directory.Delete(_options.TempDirectory, true);
                            break;
                        }
                        catch (UnauthorizedAccessException) // just make sure all locks are released, cuz hwc may not shutdown instantly
                        {
                            Thread.Sleep(500);
                        }
                    }
                }
            
        }

        static bool ConsoleEventCallback(CtrlEvent eventType)
        {
            _exitWaitHandle.Set();
            return true;
        }


        public static void ValidateRequiredDllDependencies(string applicationHostConfigText)
        {
            var doc = XDocument.Parse(applicationHostConfigText);

            var missingDlls = doc.XPathSelectElements("//configuration/system.webServer/globalModules/add")
                .Select(x => Environment.ExpandEnvironmentVariables(x.Attribute("image").Value))
                .Where(x => !File.Exists(x))
                .ToList();
            if (missingDlls.Any())
            {
                throw new ValidationException($"Missing required ddls:\n{string.Join("\n", missingDlls)}");
            }
        }
    }
}
