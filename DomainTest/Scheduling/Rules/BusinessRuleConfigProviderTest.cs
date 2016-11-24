using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
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

			var specifications = new List<IShiftTradeSpecification>
			{
				new ShiftTradeAbsenceSpecification(),
				new ShiftTradeDateSpecification(),
				new ShiftTradeMeetingSpecification()
			};

			var target = new BusinessRuleConfigProvider(businessRuleProvider, specifications, stateHolder);
			var result = target.GetDefaultConfigForShiftTradeRequest().ToList();

			Assert.AreEqual(businessRules.Count + specifications.Count - 2, result.Count);
			Assert.IsNull(result.FirstOrDefault(x => x.BusinessRuleType == ruleToRemove1.FullName));
			Assert.IsNull(result.FirstOrDefault(x => x.BusinessRuleType == ruleToRemove2.FullName));

			foreach (var rule in businessRules.Where(x=>x.GetType() != ruleToRemove1 && x.GetType() != ruleToRemove2))
			{
				var config = result.FirstOrDefault(x => x.BusinessRuleType == rule.GetType().FullName);
				Assert.IsNotNull(config);
				Assert.IsTrue(string.CompareOrdinal(config.FriendlyName, rule.FriendlyName) == 0);
				Assert.IsTrue(config.Enabled);
				Assert.IsTrue(config.HandleOptionOnFailed == RequestHandleOption.Pending);
			}

			foreach (var specification in specifications)
			{
				var config = result.FirstOrDefault(x => x.BusinessRuleType == specification.GetType().FullName);
				Assert.IsNotNull(config);
				// TODO: Set friendly name for specifications
				//Assert.IsTrue(string.CompareOrdinal(config.FriendlyName, specification.FriendlyName) == 0);
				Assert.IsTrue(config.Enabled);
				Assert.IsTrue(config.HandleOptionOnFailed == RequestHandleOption.AutoDeny);
			}
		}
	}
}