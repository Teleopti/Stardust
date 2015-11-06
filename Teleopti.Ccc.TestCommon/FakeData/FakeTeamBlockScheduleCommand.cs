using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeTeamBlockScheduleCommand : ITeamBlockScheduleCommand
	{
		public IWorkShiftFinderResultHolder Execute(ISchedulingOptions schedulingOptions, IBackgroundWorkerWrapper backgroundWorker,
			IList<IPerson> selectedPersons, IList<IScheduleDay> selectedSchedules, ISchedulePartModifyAndRollbackService rollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			throw new NotImplementedException();
		}
	}
}