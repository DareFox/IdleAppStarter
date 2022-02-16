using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdleSharedLib
{
    public class ParserCMD
    {
        public static void ArgumentError(IEnumerable<Error> obj)
        {
            Logger.Log("ARGUMENT ERROR.");
            foreach (var item in obj)
            {
                switch (item)
                {
                    case BadFormatConversionError:
                        Logger.Log("Bad format: " + (item as BadFormatConversionError).NameInfo.NameText);
                        break;
                    case MissingRequiredOptionError:
                        Logger.Log("Missing required option: " + (item as MissingRequiredOptionError).NameInfo.NameText);
                        break;
                    default:
                        Logger.Log(item.ToString());
                        Logger.Log("tag: " + item.Tag);
                        break;
                }
            }
        }
    }
}
