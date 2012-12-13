using System;
using System.Threading;

namespace AnalysisServicesManager
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                var argument = new CommandLineArgument(args);

                string pre = new FileHandler(argument.FilePath).FileAsString;

                string post = new CubeSourceFormat(pre).FindAndReplace(argument);

                new Repository(argument).Execute(post);
				return 0; //success
            }
            catch (Exception e)
            {
                Console.WriteLine("Error message:");
                Console.Error.WriteLine(e.Message);
                Console.WriteLine("");

                FileHandler.LogError(e.Message, e.StackTrace);
				Thread.Sleep(4000);
				return -1; //failed
            }
            
        }
    }
}
