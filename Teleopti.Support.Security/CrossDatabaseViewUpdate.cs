using log4net;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Support.Security
{
	internal class CrossDatabaseViewUpdate : ICommandLineCommand
	{
		private readonly IUpdateCrossDatabaseView _updateCrossDatabaseView;
		private static readonly ILog log = LogManager.GetLogger(typeof(Program));

		public CrossDatabaseViewUpdate(IUpdateCrossDatabaseView updateCrossDatabaseView)
		{
			_updateCrossDatabaseView = updateCrossDatabaseView;
		}

		public int Execute(IDatabaseArguments commandLineArgument)
		{
			log.Debug("Link Analytics to Agg datatbase ...");

			_updateCrossDatabaseView.Execute(commandLineArgument.AnalyticsDbConnectionString, commandLineArgument.AggDatabase);
            log.Debug("Link Analytics to Agg datatbase. Done!");
			return 0;
		}
	}
}