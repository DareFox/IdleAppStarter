using CommandLine;

namespace IdleSharedLib
{
    public class ServiceConfig : Config
    {
        [Option('t', "timeout", Default = (long)45000, HelpText = "Connection timeout between client and server in milliseconds")]
        public long connectionTimeoutMS { get; set; }
    }
}
