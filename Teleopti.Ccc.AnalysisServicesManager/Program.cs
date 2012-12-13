using System;
using System.Threading;

namespace AnalysisServicesManager
{
    class Program
    {
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "DropDatabase"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xmla")]
		static int Main(string[] args)
        {
			
            try
            {
				//Avoid changing WISE, instead: use xmla scriptname as an "argument"
				const string Drop="DropDatabase.xmla";
				const string Process="ProcessDatabase.xmla";

                var argument = new CommandLineArgument(args);

				var repository = new Repository(argument);

				switch (argument.FilePath)
				{
					case Drop:
						Console.WriteLine("Drop cube database ...");
						repository.DropDatabase();
						break;

					case Process:
						Console.WriteLine("Process Cube ...");
						repository.ProcessCube();
						break;

					//Create, or any other custom xmla 
					default:
						Console.WriteLine("Run custom xmla script: [" + argument.FilePath + "] ...");
						repository.ExecuteAnyXmla(argument);
						break;
				}

				Console.WriteLine("action completed successfully");
				Thread.Sleep(2000);
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
