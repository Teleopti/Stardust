using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Islands
{
	public class SkillSetContext
	{
		private readonly CreateSkillGroups _createSkillGroups;

		public SkillSetContext(CreateSkillGroups createSkillGroups)
		{
			_createSkillGroups = createSkillGroups;
		}

		[ThreadStatic]
		private static SkillSets _skillSets;

		public static SkillSets SkillSets => _skillSets;

		public IDisposable Create(IEnumerable<IPerson> personsInOrganization, DateOnlyPeriod period)
		{
			if (_skillSets != null)
				throw new NotSupportedException("Nested virtualSkillGroupResult context.");
			_skillSets = _createSkillGroups.Create(personsInOrganization, period.StartDate);
			return new GenericDisposable(() => { _skillSets = null; });
		}
	}
}