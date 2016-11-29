using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
{
	[TestFixture]
	public class BusinessRuleConfigProviderTest
	{
		[Test]
		public void ShouldReturnCorrectDefaultBusinessRulesConfig()
		{
			var ruleToRemove1 = typeof(NewPersonAccountRule);
			var ruleToRemove2 = typeof(OpenHoursRule);

			var stateHolder = new FakeSchedulingResultStateHolder();
			var businessRules = NewBusinessRuleCollection.All(new SchedulingResultStateHolder());
			businessRules.Remove(ruleToRemove1);
			businessRules.Remove(ruleToRemove2);

			var businessRuleProvider = MockRepository.GenerateMock<IBusinessRuleProvider>();
			businessRuleProvider.Stub(x => x.GetBusinessRulesForShiftTradeRequest(stateHolder, true))
				.IgnoreArguments().Return(businessRules);
			
			var target = new BusinessRuleConfigProvider(businessRuleProvider, stateHolder);
			var result = target.GetDefaultConfigForShiftTradeRequest().ToList();

			Assert.AreEqual(businessRules.Count - 2, result.Count);
			Assert.IsNull(result.FirstOrDefault(x => x.BusinessRuleType == ruleToRemove1.FullName));
			Assert.IsNull(result.FirstOrDefault(x => x.BusinessRuleType == ruleToRemove2.FullName));

			foreach (var rule in businessRules.Where(x=>x.GetType() != ruleToRemove1 && x.GetType() != ruleToRemove2))
			{
				var config = result.FirstOrDefault(x => x.BusinessRuleType == rule.GetType().FullName);
				Assert.IsNotNull(config);
				Assert.IsTrue(string.CompareOrdinal(config.FriendlyName, rule.Description) == 0);
				Assert.IsTrue(config.Enabled);
				Assert.IsTrue(config.HandleOptionOnFailed == RequestHandleOption.Pending);
			}
		}
	}
}