using log4net;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Support.Security
{
	internal class CrossDatabaseViewUpdate : ICommandLineCommand
	{
		private readonly IUpdateCrossDatabaseView _updateCrossDatabaseView;
		private static readonly ILog log = LogManager.GetLogger(typeof(CrossDatabaseViewUpdate));

		public CrossDatabaseViewUpdate(IUpdateCrossDatabaseView updateCrossDatabaseView)
		{
			_updateCrossDatabaseView = updateCrossDatabaseView;
		}

		public int Execute(UpgradeCommand commandLineArgument)
		{
			log.Debug("Link Analytics to Agg datatbase ...");

			_updateCrossDatabaseView.Execute(commandLineArgument.AnalyticsConnectionString, commandLineArgument.AggDatabase);
            log.Debug("Link Analytics to Agg datatbase. Done!");
			return 0;
		}
	}
}