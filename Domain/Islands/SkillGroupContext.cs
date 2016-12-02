using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Islands
{
	public class SkillGroupContext : ISkillGroupContext
	{
		private readonly CreateSkillGroups _createSkillGroups;

		public SkillGroupContext(CreateSkillGroups createSkillGroups)
		{
			_createSkillGroups = createSkillGroups;
		}

		[ThreadStatic]
		private static SkillGroups _skillGroups;

		public static SkillGroups SkillGroups => _skillGroups;

		public IDisposable Create(IEnumerable<IPerson> personsInOrganization, DateOnlyPeriod period)
		{
			if (_skillGroups != null)
				throw new NotSupportedException("Nested virtualSkillGroupResult context.");
			_skillGroups = _createSkillGroups.Create(personsInOrganization, period.StartDate);
			return new GenericDisposable(() => { _skillGroups = null; });
		}
	}
}