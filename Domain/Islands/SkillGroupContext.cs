using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Islands
{
	public class SkillGroupContext : ISkillGroupContext
	{
		private readonly Func<ISchedulingResultStateHolder> _scheduleResultStateHolder;
		private readonly CreateSkillGroups _createSkillGroups;

		public SkillGroupContext(Func<ISchedulingResultStateHolder> scheduleResultStateHolder, CreateSkillGroups createSkillGroups)
		{
			_scheduleResultStateHolder = scheduleResultStateHolder;
			_createSkillGroups = createSkillGroups;
		}

		[ThreadStatic]
		private static SkillGroups _skillGroups;

		public static SkillGroups SkillGroups => _skillGroups;

		public IDisposable Create(DateOnlyPeriod period)
		{
			if (_skillGroups != null)
				throw new NotSupportedException("Nested virtualSkillGroupResult context.");
			_skillGroups = _createSkillGroups.Create(_scheduleResultStateHolder().PersonsInOrganization, period.StartDate);
			return new GenericDisposable(() => { _skillGroups = null; });
		}
	}
}