using System;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Scheduling.SchedulingScreenInternals
{
	internal class LoaderMethod
	{
		private readonly Action<IUnitOfWork, ISchedulerStateHolder, IPeopleAndSkillLoaderDecider> _action;
		private readonly string _statusStripString;

		public LoaderMethod(Action<IUnitOfWork, ISchedulerStateHolder, IPeopleAndSkillLoaderDecider> action,
							string statusStripString)
		{
			_action = action;
			_statusStripString = statusStripString;
		}

		public Action<IUnitOfWork, ISchedulerStateHolder, IPeopleAndSkillLoaderDecider> Action
		{
			get { return _action; }
		}

		public string StatusStripString
		{
			get { return _statusStripString; }
		}
	}
}
