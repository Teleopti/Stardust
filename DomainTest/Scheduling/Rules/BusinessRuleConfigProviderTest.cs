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
			var stateHolder = new FakeSchedulingResultStateHolder();
			var businessRules = NewBusinessRuleCollection.All(new SchedulingResultStateHolder());
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

			Assert.AreEqual(businessRules.Count + specifications.Count, result.Count);

			foreach (var rule in businessRules)
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