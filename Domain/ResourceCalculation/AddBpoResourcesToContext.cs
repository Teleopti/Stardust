using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class AddBpoResourcesToContext
	{
		private readonly ISkillCombinationResourceBpoReader _skillCombinationResourceBpoReader;

		public AddBpoResourcesToContext(ISkillCombinationResourceBpoReader skillCombinationResourceBpoReader)
		{
			_skillCombinationResourceBpoReader = skillCombinationResourceBpoReader;
		}
		
		public void Execute(ResourceCalculationDataContainer resourceCalculationDataContatiner, IEnumerable<ISkill> skills, DateTimePeriod period)
		{
			var skillCombinationResources = _skillCombinationResourceBpoReader.Execute(period);
			var bpoResources = map_makeSeparateType(skillCombinationResources, skills);
			
			foreach (var bpoResource in bpoResources)
			{
				var tempAgent = bpoResource.CreateTempAgent();
				foreach (var resourceLayer in bpoResource.CreateResourceLayers())
				{
					resourceCalculationDataContatiner.AddResources(tempAgent, DateOnly.Today, resourceLayer);
				}
			}
		}

		private IEnumerable<BpoResource> map_makeSeparateType(IEnumerable<SkillCombinationResource> skillCombinationResources, IEnumerable<ISkill> skills)
		{
			return skillCombinationResources.Select(skillCombinationResource => new BpoResource(skillCombinationResource.Resource,
				skills.Where(x => skillCombinationResource.SkillCombination.Contains(x.Id.Value)),
				skillCombinationResource.Period()));
		}
	}
}