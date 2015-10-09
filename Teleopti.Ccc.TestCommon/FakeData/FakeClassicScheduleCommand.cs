using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeClassicScheduleCommand : IClassicScheduleCommand
	{
		public void Execute(ISchedulingOptions schedulingOptions, IBackgroundWorkerWrapper backgroundWorker,
			IRequiredScheduleHelper requiredScheduleOptimizerHelper, IList<IScheduleDay> selectedSchedules, bool runWeeklyRestSolver)
		{
			throw new NotImplementedException();
		}
	}
}