using System;
using System.IO;
using System.Reflection;

namespace DotnetHwcBuildpack
{
    public class HwcDotnetBuildpack : FinalBuildpack
    {
        public override bool Detect(string buildPath)
        {
            return false;
        }

        protected override void Apply(string buildPath, string cachePath, string depsPath, int index)
        {
            var srcHwc = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "hwc");
            var targetHwc = Path.Combine(buildPath, ".cloudfoundry");
            if(!Directory.Exists(targetHwc))
                Directory.CreateDirectory(targetHwc);
            foreach(var file in Directory.EnumerateFiles(srcHwc))
                File.Copy(file, Path.Combine(targetHwc, Path.GetFileName(file)));
            
        }

        public override string GetStartupCommand(string buildPath)
        {
            return $".cloudfoundry\\hwc.exe";
        }
    }
}
