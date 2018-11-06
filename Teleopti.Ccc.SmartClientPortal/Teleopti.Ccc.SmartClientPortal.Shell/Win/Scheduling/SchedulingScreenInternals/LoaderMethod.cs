using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SchedulingScreenInternals
{
	internal class LoaderMethod
	{
		public LoaderMethod(Action<IUnitOfWork, SchedulingScreenState> action, string statusStripString)
		{
			Action = action;
			StatusStripString = statusStripString;
		}

		public Action<IUnitOfWork, SchedulingScreenState> Action { get; }

		public string StatusStripString { get; }
	}
}
