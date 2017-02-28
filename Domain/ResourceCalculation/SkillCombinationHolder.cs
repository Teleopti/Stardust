using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class SkillCombinationHolder
	{
		private List<SkillCombinationResource> _skillCombinationResources = new List<SkillCombinationResource>();

		public void Add(SkillCombinationResource skillCombinationResource)
		{
			_skillCombinationResources.Add(skillCombinationResource);
		}

		public void StartRecodingValuesWithShrinkage()
		{
			_skillCombinationResources = new List<SkillCombinationResource>();
		}

		public IEnumerable<SkillCombinationResource> SkillCombinationResources => _skillCombinationResources;
	}
}