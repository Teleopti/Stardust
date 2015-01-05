using System;
using System.Threading;
using log4net;
using log4net.Config;

namespace AnalysisServicesManager
{
    class Program
    {
	    private static ILog Logger;

	    private const int Success = 0;
	    private const int Failure = -1;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "DropDatabase"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xmla")]
		static int Main(string[] args)
		{
			XmlConfigurator.Configure();
			Logger = LogManager.GetLogger(typeof(Program));

            try
            {
				//Avoid changing WISE, instead: use xmla scriptname as an "argument"
				const string Drop="DropDatabase.xmla";
				const string Process="ProcessDatabase.xmla";
				const string Create = "CreateDatabase.xmla";

                var argument = new CommandLineArgument(args);

	            var repository =
		            new Repository(new CubeSourceFormat(argument.AnalysisConnectionInfo, argument.SqlConnectionInfo),
			            argument.AnalysisConnectionInfo);

				switch (argument.FolderInformation.FilePath)
				{
					case Drop:
						Logger.Info("Drop cube database ...");
						repository.DropDatabase();
						break;

					case Process:
						Logger.Info("Process Cube ...");
						repository.ProcessCube();
						break;

					case Create:
						Logger.Info("Create Cube ...");
						repository.ExecuteAnyXmla(argument.FolderInformation.CurrentDir + "\\" + Create);
						var foo = new CustomizeServerObject(repository, argument.AnalysisConnectionInfo,argument.SqlConnectionInfo,argument.FolderInformation);
						foo.ApplyCustomization();
						break;
				}

				Logger.Info("Action completed successfully");
				return Success;
            }
            catch (Exception e)
            {
				Logger.Error("Error message:",e);
				Thread.Sleep(4000);
				return Failure;
            }
        }
    }
}
