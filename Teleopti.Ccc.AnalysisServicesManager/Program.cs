using System;
using System.Threading;
using System.Collections.Generic;

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
				const string Create = "CreateDatabase.xmla";

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

					case Create:
						Console.WriteLine("Create Cube ...");
						repository.ExecuteAnyXmla(argument);
						break;

					default:
						Console.WriteLine("Running custom scripts from folder : " + argument.FilePath);
						var foo = new CustomizeServerObject(argument);
						foo.ApplyCustomization(argument);
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
