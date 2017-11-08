using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class AddBpoResourcesToContext
	{
		private readonly ISkillCombinationResourceBpoReader _skillCombinationResourceBpoReader;
		private readonly SkillCombinationToBpoResourceMapper _skillCombinationToBpoResourceMapper;

		public AddBpoResourcesToContext(ISkillCombinationResourceBpoReader skillCombinationResourceBpoReader, SkillCombinationToBpoResourceMapper skillCombinationToBpoResourceMapper)
		{
			_skillCombinationResourceBpoReader = skillCombinationResourceBpoReader;
			_skillCombinationToBpoResourceMapper = skillCombinationToBpoResourceMapper;
		}
		
		public void Execute(ResourceCalculationDataContainer resourceCalculationDataContatiner, IEnumerable<ISkill> skills, DateTimePeriod period)
		{
			var skillCombinationResources = _skillCombinationResourceBpoReader.Execute(period);
			var bpoResources = _skillCombinationToBpoResourceMapper.Execute(skillCombinationResources, skills);
			
			foreach (var bpoResource in bpoResources)
			{
				var tempAgent = bpoResource.CreateTempAgent();
				foreach (var resourceLayer in bpoResource.CreateResourceLayers())
				{
					resourceCalculationDataContatiner.AddResources(tempAgent, DateOnly.Today, resourceLayer);
				}
			}
		}
	}
}