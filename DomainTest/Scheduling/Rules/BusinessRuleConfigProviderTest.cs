﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.UserTexts;

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

			var target = new BusinessRuleConfigProvider(businessRuleProvider, stateHolder, new IShiftTradeSpecification[] {});
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

		[Test]
		public void ShouldReturnMinWeekWorkTimeRuleConfig()
		{
			var stateHolder = new FakeSchedulingResultStateHolder {UseMinWeekWorkTime = true};
			var businessRules = NewBusinessRuleCollection.All(stateHolder);

			var businessRuleProvider = MockRepository.GenerateMock<IBusinessRuleProvider>();
			businessRuleProvider.Stub(x => x.GetBusinessRulesForShiftTradeRequest(stateHolder, true))
				.IgnoreArguments().Return(businessRules);

			var target = new BusinessRuleConfigProvider(businessRuleProvider, stateHolder, new IShiftTradeSpecification[] { });
			var result = target.GetDefaultConfigForShiftTradeRequest().ToList();
			Assert.IsTrue(result.Any(r => r.BusinessRuleType == typeof(MinWeekWorkTimeRule).FullName));
		}

		[Test]
		public void ShouldReturnShiftTradeTargetTimeSpecificationRuleConfig()
		{
			var stateHolder = new FakeSchedulingResultStateHolder();
			var businessRules = NewBusinessRuleCollection.All(stateHolder);

			var businessRuleProvider = MockRepository.GenerateMock<IBusinessRuleProvider>();
			businessRuleProvider.Stub(x => x.GetBusinessRulesForShiftTradeRequest(stateHolder, false))
				.IgnoreArguments().Return(businessRules);

			var shiftTradeSpecifications = new IShiftTradeSpecification[]
			{
				new ShiftTradeTargetTimeSpecification(() => new SchedulerStateHolder(stateHolder, null, new FakeTimeZoneGuard()),
					null, null)
			};

			var target = new BusinessRuleConfigProvider(businessRuleProvider, stateHolder, shiftTradeSpecifications);
			var result = target.GetDefaultConfigForShiftTradeRequest().ToList();
			var shiftTradeTargetTimeSpecificationRuleConfig =
				result.FirstOrDefault(r => r.FriendlyName == Resources.DescriptionOfShiftTradeTargetTimeSpecification);
			Assert.IsNotNull(shiftTradeTargetTimeSpecificationRuleConfig);
			Assert.IsTrue(shiftTradeTargetTimeSpecificationRuleConfig.BusinessRuleType ==
						  typeof(ShiftTradeTargetTimeSpecification).FullName);
			Assert.IsTrue(shiftTradeTargetTimeSpecificationRuleConfig.HandleOptionOnFailed == RequestHandleOption.AutoDeny);
		}


		[Test]
		public void ShouldOnlyReturnConfigurableShiftTradeSpecificationRuleConfig()
		{
			var stateHolder = new FakeSchedulingResultStateHolder();
			var businessRules = NewBusinessRuleCollection.All(stateHolder);

			var businessRuleProvider = MockRepository.GenerateMock<IBusinessRuleProvider>();
			businessRuleProvider.Stub(x => x.GetBusinessRulesForShiftTradeRequest(stateHolder, false))
				.IgnoreArguments().Return(businessRules);

			var specification1 = MockRepository.GenerateMock<IShiftTradeSpecification>();
			specification1.Stub(x => x.Configurable).Return(false);
			specification1.Stub(x => x.Description).Return("specification1");
			var specification2 = MockRepository.GenerateMock<IShiftTradeSpecification>();
			specification2.Stub(x => x.Configurable).Return(true);
			specification2.Stub(x => x.Description).Return("specification2");

			var shiftTradeSpecifications = new[]
			{
				specification1,
				specification2
			};

			var target = new BusinessRuleConfigProvider(businessRuleProvider, stateHolder, shiftTradeSpecifications);
			var result = target.GetDefaultConfigForShiftTradeRequest().ToList();
			var configurableShiftTradeSpecificationRuleConfig = result.FirstOrDefault(r=>r.FriendlyName == "specification2");
			Assert.IsNotNull(configurableShiftTradeSpecificationRuleConfig);
			var nonConfigurableShiftTradeSpecificationRuleConfig = result.FirstOrDefault(r => r.FriendlyName == "specification1");
			Assert.IsNull(nonConfigurableShiftTradeSpecificationRuleConfig);
		}
	}
}