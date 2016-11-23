using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
{
	[TestFixture]
	public class BusinessRuleProviderTest
	{
		[Test]
		public void ShouldReturnCorrectBusinessRulesForShiftTrade()
		{
			var provider = new BusinessRuleProvider();
			var rules = provider.GetBusinessRulesForShiftTradeRequest(
				new FakeSchedulingResultStateHolder(), true);

			Assert.AreEqual(rules.Count, 12);

			Assert.IsTrue(rules.Count(x => x.GetType() == typeof(NewPersonAccountRule)) == 1);
			Assert.IsTrue(rules.Count(x => x.GetType() == typeof(OpenHoursRule)) == 1);
			Assert.IsTrue(rules.Count(x => x.GetType() == typeof(NonMainShiftActivityRule)) == 1);
			Assert.IsTrue(rules.Count(x => x.GetType() == typeof(SiteOpenHoursRule)) == 1);

			var removedRules = rules.Where(r => r.GetType() == typeof(NewPersonAccountRule)
												|| r.GetType() == typeof(OpenHoursRule));
			Assert.IsTrue(removedRules.All(r => r.IsMandatory || !r.HaltModify));
		}
	}
}