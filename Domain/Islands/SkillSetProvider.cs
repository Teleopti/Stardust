using System;

namespace Teleopti.Ccc.Domain.Islands
{
	public class SkillSetProvider
	{
		public SkillSets Fetch()
		{
			var current = SkillSetContext.SkillSets;
			if (current == null)
				throw new NotSupportedException("SkillSets not in context.");

			return current;
		}
	}
}