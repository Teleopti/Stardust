using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Islands.Legacy
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_SplitBigIslands_42049)]
	public class VirtualSkillContext : ISkillGroupContext
	{
		private readonly IVirtualSkillGroupsCreator _virtualSkillGroupsCreator;

		public VirtualSkillContext(IVirtualSkillGroupsCreator virtualSkillGroupsCreator)
		{
			_virtualSkillGroupsCreator = virtualSkillGroupsCreator;
		}

		[ThreadStatic]
		private static VirtualSkillGroupsCreatorResult _virtualSkillGroupResult;

		public static VirtualSkillGroupsCreatorResult VirtualSkillGroupResult
		{
			get { return _virtualSkillGroupResult; }
		}

		public IDisposable Create(IEnumerable<IPerson> personsInOrganization, DateOnlyPeriod period)
		{
			if (_virtualSkillGroupResult != null)
				throw new NotSupportedException("Nested virtualSkillGroupResult context.");
			_virtualSkillGroupResult = _virtualSkillGroupsCreator.GroupOnDate(period.StartDate, personsInOrganization);
			return new GenericDisposable(() => { _virtualSkillGroupResult = null; });
		}
	}
}