using System;
using System.IO;
using TS7S.Base;

namespace SubtitleTranslator.Application.Utils
{
    public static class CommandArgsHelper
    {
        public static string[] GetCommandArgs()
        {
            return Environment.GetCommandLineArgs();
        } 

        public static string GetFilePathArgument()
        {
            string retval = null;
            var commandArgs = GetCommandArgs();
            if(!commandArgs.IsNullOrEmpty())
            {
                if(commandArgs.Length >=2)
                {
                    retval = commandArgs[1];
                }
                else
                {
                    var applicationName = Path.GetFileName(typeof (IShell).Assembly.Location);
                    if(!string.IsNullOrEmpty(commandArgs[0]) && !Path.GetFileName(commandArgs[0]).Equals(applicationName))
                    {
                        retval = commandArgs[0];
                    }
                }
            }
            return retval;
        }
    }
}