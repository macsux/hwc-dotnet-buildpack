using System;
using System.IO;

namespace DotnetHwcBuildpack
{
    public class DotnetHwcBuildpack : FinalBuildpack //SupplyBuildpack 
    {
        public override bool Detect(string buildPath)
        {
            return false;
        }

        protected override void Apply(string buildPath, string cachePath, string depsPath, int index)
        {
            var myDependenciesDirectory = Path.Combine(depsPath, index.ToString()); // store any runtime dependencies not belonging to the app in this directory
            
            Console.WriteLine($"===Applying {nameof(DotnetHwcBuildpack)}===");
            
            EnvironmentalVariables["MY_SETTING"] = "value"; // set any environmental variables for the app (staging phase)
            
        }
/*
        protected override void PreStartup(string buildPath, string depsPath, int index)
        {
            Console.WriteLine("Application is about to start...");
            EnvironmentalVariables["MY_SETTING"] = "value"; // can set env vars before app starts running
        }
*/
        public override string GetStartupCommand(string buildPath)
        {
            return "test.exe";
        }
    }
}
