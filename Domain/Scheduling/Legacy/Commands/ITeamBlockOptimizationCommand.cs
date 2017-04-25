using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public interface ITeamBlockOptimizationCommand
	{
		void Execute(ISchedulingProgress backgroundWorker, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons,
			IOptimizationPreferences optimizationPreferences, ISchedulePartModifyAndRollbackService rollbackService,
			IScheduleTagSetter tagSetter, SchedulingOptions schedulingOptions,
			IResourceCalculateDelayer resourceCalculateDelayer, IList<IScheduleDay> selectedSchedules, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider);
	}
}