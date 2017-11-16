using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class BpoResourcesProvider
	{
		private readonly ISkillCombinationResourceBpoReader _skillCombinationResourceBpoReader;
		private readonly SkillCombinationToBpoResourceMapper _skillCombinationToBpoResourceMapper;

		public BpoResourcesProvider(ISkillCombinationResourceBpoReader skillCombinationResourceBpoReader, 
			SkillCombinationToBpoResourceMapper skillCombinationToBpoResourceMapper)
		{
			_skillCombinationResourceBpoReader = skillCombinationResourceBpoReader;
			_skillCombinationToBpoResourceMapper = skillCombinationToBpoResourceMapper;
		}
		
		public IEnumerable<BpoResource> Fetch(IEnumerable<ISkill> skills, DateTimePeriod period)
		{
			var skillCombinationResources = _skillCombinationResourceBpoReader.Execute(period);
			return _skillCombinationToBpoResourceMapper.Execute(skillCombinationResources, skills);
		}
	}
}