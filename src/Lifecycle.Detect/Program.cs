using System;
using System.Linq;
using DotnetHwcBuildpack;

namespace Lifecycle.Supply
{
    class Program
    {
        static int Main(string[] args)
        {
            var argsWithCommand = new[] {"Detect"}.Concat(args).ToArray();
            return HwcDotnetBuildpack.Program.Main(argsWithCommand);
        }
    }
}