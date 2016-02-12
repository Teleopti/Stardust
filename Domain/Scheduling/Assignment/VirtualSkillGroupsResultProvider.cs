using System;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class VirtualSkillGroupsResultProvider
	{
		private readonly Func<ISchedulingResultStateHolder> _scheduleResultStateHolder;
		private readonly VirtualSkillGroupsCreator _virtualSkillGroupsCreator;
		private VirtualSkillGroupsCreatorResult _fetchedResult;

		public VirtualSkillGroupsResultProvider(Func<ISchedulingResultStateHolder> scheduleResultStateHolder, VirtualSkillGroupsCreator virtualSkillGroupsCreator)
		{
			_scheduleResultStateHolder = scheduleResultStateHolder;
			_virtualSkillGroupsCreator = virtualSkillGroupsCreator;
		}

		public VirtualSkillGroupsCreatorResult Fetch(DateOnly date)
		{
			return _fetchedResult ??
						 (_fetchedResult = _virtualSkillGroupsCreator.GroupOnDate(date, _scheduleResultStateHolder().PersonsInOrganization));
		}
	}
}