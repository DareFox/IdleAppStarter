using CommandLine;
using NLog;
using System.Collections.Generic;

namespace IdleSharedLib
{
    public class ParserCMD
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public static void ArgumentError(IEnumerable<Error> obj)
        {
            logger.Error("ARGUMENT ERROR.");
            foreach (var item in obj)
            {
                switch (item)
                {
                    case BadFormatConversionError:
                        logger.Error("Bad format: " + (item as BadFormatConversionError).NameInfo.NameText);
                        break;
                    case MissingRequiredOptionError:
                        logger.Error("Missing required option: " + (item as MissingRequiredOptionError).NameInfo.NameText);
                        break;
                    default:
                        logger.Error(item.ToString());
                        logger.Error("tag: " + item.Tag);
                        break;
                }
            }
        }
    }
}
