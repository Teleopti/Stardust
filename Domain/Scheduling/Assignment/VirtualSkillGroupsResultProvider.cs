using System;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class VirtualSkillGroupsResultProvider
	{
		private readonly Func<ISchedulingResultStateHolder> _scheduleResultStateHolder;
		private readonly VirtualSkillGroupsCreator _virtualSkillGroupsCreator;

		public VirtualSkillGroupsResultProvider(Func<ISchedulingResultStateHolder> scheduleResultStateHolder, VirtualSkillGroupsCreator virtualSkillGroupsCreator)
		{
			_scheduleResultStateHolder = scheduleResultStateHolder;
			_virtualSkillGroupsCreator = virtualSkillGroupsCreator;
		}

		public VirtualSkillGroupsCreatorResult Fetch(DateOnly date)
		{
			//första gången här per X
			return _virtualSkillGroupsCreator.GroupOnDate(date, _scheduleResultStateHolder().PersonsInOrganization);
		}
	}
}