using System;
using System.Collections.Generic;
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
			var ruleToRemove1 = typeof(MinWeeklyRestRule);
			var ruleToRemove2 = typeof(MinWeekWorkTimeRule);

			var rulesToRemove = new List<Type> {ruleToRemove1, ruleToRemove2};

			var stateHolder = new FakeSchedulingResultStateHolder();
			var businessRules = NewBusinessRuleCollection.All(new SchedulingResultStateHolder());
			businessRules.DoNotHaltModify(ruleToRemove1);
			businessRules.DoNotHaltModify(ruleToRemove2);

			var businessRuleProvider = MockRepository.GenerateMock<IBusinessRuleProvider>();
			businessRuleProvider.Stub(x => x.GetBusinessRulesForShiftTradeRequest(stateHolder, true))
				.IgnoreArguments().Return(businessRules);

			var configurableRules = businessRules.Where(x => x.Configurable && (x.IsMandatory || x.HaltModify)
															 && !rulesToRemove.Contains(x.GetType())).ToArray();

			var target = new BusinessRuleConfigProvider(businessRuleProvider, stateHolder);
			var result = target.GetDefaultConfigForShiftTradeRequest().ToList();

			Assert.AreEqual(configurableRules.Length, result.Count);

			// Should not contins unconfigurable rules and removed rules
			var rulesShouldNotExists = businessRules
				.Where(r => !r.Configurable || (!r.IsMandatory && !r.HaltModify))
				.Select(r => r.GetType().FullName).ToList();
			rulesShouldNotExists.AddRange(rulesToRemove.Select(r => r.FullName));

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