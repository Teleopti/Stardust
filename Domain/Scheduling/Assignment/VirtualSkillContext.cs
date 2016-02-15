using System;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	//change to shared "scheduling context" or similar later?
	public class VirtualSkillContext
	{
		private readonly Func<ISchedulingResultStateHolder> _scheduleResultStateHolder;
		private readonly VirtualSkillGroupsCreator _virtualSkillGroupsCreator;

		public VirtualSkillContext(Func<ISchedulingResultStateHolder> scheduleResultStateHolder, VirtualSkillGroupsCreator virtualSkillGroupsCreator)
		{
			_scheduleResultStateHolder = scheduleResultStateHolder;
			_virtualSkillGroupsCreator = virtualSkillGroupsCreator;
		}

		[ThreadStatic]
		private static VirtualSkillGroupsCreatorResult _virtualSkillGroupResult;

		public static VirtualSkillGroupsCreatorResult VirtualSkillGroupResult
		{
			get { return _virtualSkillGroupResult; }
		}

		public IDisposable Create(DateOnlyPeriod period)
		{
			if (_virtualSkillGroupResult != null)
				throw new NotSupportedException("Nested virtualSkillGroupResult context.");
			_virtualSkillGroupResult = _virtualSkillGroupsCreator.GroupOnDate(period.StartDate, _scheduleResultStateHolder().PersonsInOrganization);
			return new GenericDisposable(() => { _virtualSkillGroupResult = null; });
		}
	}
}