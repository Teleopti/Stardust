using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public class ShiftProjectionCachesForIntraInterval
	{
		private readonly ITeamBlockRoleModelSelector _roleModelSelector;
		private readonly ITeamBlockSingleDayScheduler _singleDayScheduler;
		private readonly IWorkShiftSelector _workShiftSelector;

		public ShiftProjectionCachesForIntraInterval(ITeamBlockRoleModelSelector roleModelSelector, 
																								ITeamBlockSingleDayScheduler singleDayScheduler,
																								IWorkShiftSelector workShiftSelector)
		{
			_roleModelSelector = roleModelSelector;
			_singleDayScheduler = singleDayScheduler;
			_workShiftSelector = workShiftSelector;
		}

		public IList<IWorkShiftCalculationResultHolder> Execute(
			ITeamBlockInfo teamBlockInfo,
			IPerson person,
			DateOnly datePointer,
			ISchedulingOptions schedulingOptions,
			ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			IList<IWorkShiftCalculationResultHolder> resultList = new List<IWorkShiftCalculationResultHolder>();
			var teamInfo = teamBlockInfo.TeamInfo;
			var selectedTeamMembers = teamInfo.GroupMembers.Intersect(teamInfo.UnLockedMembers(datePointer)).ToList();
			if (selectedTeamMembers.IsEmpty())
				return resultList;

			var roleModelShift = _roleModelSelector.Select(schedulingResultStateHolder.Schedules, schedulingResultStateHolder.AllSkillDays(), _workShiftSelector, teamBlockInfo, datePointer, selectedTeamMembers.First(), schedulingOptions, new EffectiveRestriction());

			if (roleModelShift == null)
				return resultList;

			resultList = _singleDayScheduler.GetShiftProjectionCaches(teamBlockInfo, schedulingOptions, datePointer,
				roleModelShift, schedulingResultStateHolder, person);

			return resultList;
		}
	}
}