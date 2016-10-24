using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public class ShiftProjectionCachesForIntraInterval
	{
		private readonly ITeamBlockRoleModelSelector _roleModelSelector;
		private readonly ITeamBlockSingleDayScheduler _singleDayScheduler;

		public ShiftProjectionCachesForIntraInterval(ITeamBlockRoleModelSelector roleModelSelector, ITeamBlockSingleDayScheduler singleDayScheduler)
		{
			_roleModelSelector = roleModelSelector;
			_singleDayScheduler = singleDayScheduler;
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

			IShiftProjectionCache roleModelShift = _roleModelSelector.Select(teamBlockInfo, datePointer, selectedTeamMembers.First(),
				schedulingOptions, new EffectiveRestriction());

			if (roleModelShift == null)
				return resultList;

			resultList = _singleDayScheduler.GetShiftProjectionCaches(teamBlockInfo, schedulingOptions, datePointer,
				roleModelShift, schedulingResultStateHolder, person);

			return resultList;
		}
	}
}