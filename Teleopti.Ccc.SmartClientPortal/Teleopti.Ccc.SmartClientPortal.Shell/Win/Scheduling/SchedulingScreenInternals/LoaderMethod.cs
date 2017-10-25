using System;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SchedulingScreenInternals
{
	[RemoveMeWithToggle("Remove the two ILoaderDeciderResult", Toggles.ResourcePlanner_FasterLoading_46307)]
	internal class LoaderMethod
	{
		private readonly Action<IUnitOfWork, ISchedulerStateHolder, Action<ILoaderDeciderResult>, Func<ILoaderDeciderResult>> _action;
		private readonly string _statusStripString;

		public LoaderMethod(Action<IUnitOfWork, ISchedulerStateHolder, Action<ILoaderDeciderResult>, Func<ILoaderDeciderResult>> action,
							string statusStripString)
		{
			_action = action;
			_statusStripString = statusStripString;
		}

		public Action<IUnitOfWork, ISchedulerStateHolder, Action<ILoaderDeciderResult>, Func<ILoaderDeciderResult>> Action
		{
			get { return _action; }
		}

		public string StatusStripString
		{
			get { return _statusStripString; }
		}
	}
}
