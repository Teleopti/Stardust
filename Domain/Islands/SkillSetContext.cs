using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Islands
{
	public class SkillSetContext
	{
		private readonly CreateSkillSets _createSkillSets;

		public SkillSetContext(CreateSkillSets createSkillSets)
		{
			_createSkillSets = createSkillSets;
		}

		[ThreadStatic]
		private static SkillSets _skillSets;

		public static SkillSets SkillSets => _skillSets;

		public IDisposable Create(IEnumerable<IPerson> personsInOrganization, DateOnlyPeriod period)
		{
			if (_skillSets != null)
				throw new NotSupportedException("Nested virtualSkillGroupResult context.");
			_skillSets = _createSkillSets.Create(personsInOrganization, period.StartDate);
			return new GenericDisposable(() => { _skillSets = null; });
		}
	}
}