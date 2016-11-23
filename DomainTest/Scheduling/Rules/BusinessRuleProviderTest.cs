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

			Assert.IsTrue(rules.Item(typeof(NewPersonAccountRule)) != null);
			Assert.IsTrue(rules.Item(typeof(OpenHoursRule)) != null);
			Assert.IsTrue(rules.Item(typeof(NonMainShiftActivityRule)) != null);
			Assert.IsTrue(rules.Item(typeof(SiteOpenHoursRule)) != null);

			Assert.IsTrue(new[]
			{
				rules.Item(typeof(NewPersonAccountRule)),
				rules.Item(typeof(OpenHoursRule))
			}.All(r => r.IsMandatory || !r.HaltModify));
		}
	}
}