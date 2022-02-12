using CommandLine;
using System.Collections.Generic;

namespace IdleSharedLib
{
    public class Config
    {
        [Option('e', "executables", Required = true, HelpText = "Programms to execute")]
        public IEnumerable<string> inputExec { get; set; }

        [Option('i', "idle", Required = true, HelpText = "Idle in ms")]
        public long idle { get; set; }

        [Option('p', "port", Required = true, HelpText = "Port for service communication")]
        public int port { get; set; }
    }
}
