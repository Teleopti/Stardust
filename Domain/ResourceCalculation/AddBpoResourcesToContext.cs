using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
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
				var tempAgent = new Person();
				var personPeriod = new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team());
				bpoResource.Skills.ForEach(x => personPeriod.AddPersonSkill(new PersonSkill(x, new Percent(1))));
				tempAgent.AddPersonPeriod(personPeriod);
				var uniqueActivities = bpoResource.Skills.Select(x => x.Activity).Distinct();
				var numberOfActivities = uniqueActivities.Count();
				foreach (var activity in uniqueActivities)
				{
					var resLayer = new ResourceLayer
					{
						PayloadId = activity.Id.Value,
						Period = bpoResource.Period,
						Resource = bpoResource.Resources / numberOfActivities
					};

					resourceCalculationDataContatiner.AddResources(tempAgent, DateOnly.Today, resLayer);
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