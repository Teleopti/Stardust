using System;

namespace AnalysisServicesManager
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var argument = new CommandLineArgument(args);

                string pre = new FileHandler(argument.FilePath).FileAsString;

                string post = new CubeSourceFormat(pre).FindAndReplace(argument);

                new Repository(argument).Execute(post);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error message:");
                Console.Error.WriteLine(e.Message);
                Console.WriteLine("");

                FileHandler.LogError(e.Message, e.StackTrace);

                Console.WriteLine("");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey(false);
            }
            
        }
    }
}
