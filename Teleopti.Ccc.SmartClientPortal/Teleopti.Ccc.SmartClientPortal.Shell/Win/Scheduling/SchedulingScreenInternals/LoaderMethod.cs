using System;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SchedulingScreenInternals
{
	internal class LoaderMethod
	{
		public LoaderMethod(Action<IUnitOfWork, ISchedulerStateHolder> action, string statusStripString)
		{
			Action = action;
			StatusStripString = statusStripString;
		}

		public Action<IUnitOfWork, ISchedulerStateHolder> Action { get; }

		public string StatusStripString { get; }
	}
}
