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
			var unconfiguraableRules = new[]
			{
				typeof(DataPartOfAgentDay),
				typeof(NewPersonAccountRule),
				typeof(OpenHoursRule)
			};

			var ruleToRemove1 = typeof(MinWeeklyRestRule);
			var ruleToRemove2 = typeof(MinWeekWorkTimeRule);

			var stateHolder = new FakeSchedulingResultStateHolder();
			var businessRules = NewBusinessRuleCollection.All(new SchedulingResultStateHolder());
			businessRules.DoNotHaltModify(ruleToRemove1);
			businessRules.DoNotHaltModify(ruleToRemove2);

			var businessRuleProvider = MockRepository.GenerateMock<IBusinessRuleProvider>();
			businessRuleProvider.Stub(x => x.GetBusinessRulesForShiftTradeRequest(stateHolder, true))
				.IgnoreArguments().Return(businessRules);

			var configurableRules = businessRules.Where(x => !unconfiguraableRules.Contains(x.GetType())
					 && x.GetType() != ruleToRemove1 && x.GetType() != ruleToRemove2).ToArray();

			var target = new BusinessRuleConfigProvider(businessRuleProvider, stateHolder);
			var result = target.GetDefaultConfigForShiftTradeRequest().ToList();

			Assert.AreEqual(configurableRules.Length, result.Count);

			// Should not contain any unconfigurable rules and removed rules
			var rulesShouldNotExists = unconfiguraableRules.Select(x => x.FullName).ToList();
			rulesShouldNotExists.Add(ruleToRemove1.FullName);
			rulesShouldNotExists.Add(ruleToRemove2.FullName);
			Assert.IsTrue(result.All(x => !rulesShouldNotExists.Contains(x.BusinessRuleType)));

			foreach (var rule in configurableRules)
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