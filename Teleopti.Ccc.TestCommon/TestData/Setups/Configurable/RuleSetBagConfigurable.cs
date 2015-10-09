using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class RuleSetBagConfigurable : IDataSetup
	{
		public string Name { get; set; }
		public string RuleSet { get; set; }

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var allRuleSets = new WorkShiftRuleSetRepository(currentUnitOfWork).LoadAll();
			var ruleSetNames = RuleSet.Split(',').Select(s => s.Trim());
			var ruleSets = from s in ruleSetNames select allRuleSets.Single(x => x.Description.Name == s);

			var ruleSetBag = new RuleSetBag {Description = new Description(Name)};
			ruleSets.ForEach(ruleSetBag.AddRuleSet);

			new RuleSetBagRepository(currentUnitOfWork).Add(ruleSetBag);
		}
	}
}