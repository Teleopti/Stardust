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
		public void Execute(ResourceCalculationDataContainer resourceCalculationDataContatiner, IEnumerable<BpoResource> bpoResources)
		{
			if (bpoResources == null)
				return;

			foreach (var bpoResource in bpoResources)
			{
				var tempAgent = new Person();
				var period = new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team());
				bpoResource.Skills.ForEach(x => period.AddPersonSkill(new PersonSkill(x, new Percent(1))));
				tempAgent.AddPersonPeriod(period);		
				var resLayer = new ResourceLayer
				{
					PayloadId = bpoResource.Skills.First().Activity.Id.Value,
					Period = bpoResource.Period,
					Resource = bpoResource.Resources
				};

				resourceCalculationDataContatiner.AddResources(tempAgent, DateOnly.Today, resLayer);
				
			}
		}
	}
}