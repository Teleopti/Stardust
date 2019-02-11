using System;
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

			var rulesToRemove = new List<Type> { ruleToRemove1, ruleToRemove2 };

			var stateHolder = new FakeSchedulingResultStateHolder_DoNotUse();
			var businessRules = NewBusinessRuleCollection.All(new SchedulingResultStateHolder());
			businessRules.DoNotHaltModify(ruleToRemove1);
			businessRules.DoNotHaltModify(ruleToRemove2);

			var businessRuleProvider = MockRepository.GenerateMock<IBusinessRuleProvider>();
			businessRuleProvider.Stub(x => x.GetBusinessRulesForShiftTradeRequest(stateHolder, true))
				.IgnoreArguments().Return(businessRules);

			var configurableRules = businessRules.Where(x => x.Configurable && (x.IsMandatory || x.HaltModify)
															 && !rulesToRemove.Contains(x.GetType())).ToArray();

			var target = new BusinessRuleConfigProvider(businessRuleProvider, stateHolder, new IShiftTradeSpecification[] { });
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
			var stateHolder = new FakeSchedulingResultStateHolder_DoNotUse();
			var businessRules = NewBusinessRuleCollection.All(stateHolder);

			var businessRuleProvider = MockRepository.GenerateMock<IBusinessRuleProvider>();
			businessRuleProvider.Stub(x => x.GetBusinessRulesForShiftTradeRequest(stateHolder, true))
				.IgnoreArguments().Return(businessRules);

			var target = new BusinessRuleConfigProvider(businessRuleProvider, stateHolder, new IShiftTradeSpecification[] { });
			var result = target.GetDefaultConfigForShiftTradeRequest().ToList();
			Assert.IsTrue(result.Any(r => r.BusinessRuleType == typeof(MinWeekWorkTimeRule).FullName));
		}

		[Test]
		public void ShouldReturnMaximumWorkdayRuleConfig()
		{
			var stateHolder = new FakeSchedulingResultStateHolder_DoNotUse();
			stateHolder.UseMaximumWorkday = true;
			var businessRules = NewBusinessRuleCollection.All(stateHolder);

			var businessRuleProvider = MockRepository.GenerateMock<IBusinessRuleProvider>();
			businessRuleProvider.Stub(x => x.GetBusinessRulesForShiftTradeRequest(stateHolder, true))
				.IgnoreArguments().Return(businessRules);

			var target = new BusinessRuleConfigProvider(businessRuleProvider, stateHolder, new IShiftTradeSpecification[] { });
			var result = target.GetDefaultConfigForShiftTradeRequest().ToList();
			Assert.IsTrue(result.Any(r => r.BusinessRuleType == typeof(MaximumWorkdayRule).FullName));
		}

		[Test]
		public void ShouldSetDefaultFalseForMaximumWorkdayRuleConfig()
		{
			var stateHolder = new FakeSchedulingResultStateHolder_DoNotUse();
			stateHolder.UseMaximumWorkday = true;
			var businessRules = NewBusinessRuleCollection.All(stateHolder);

			var businessRuleProvider = MockRepository.GenerateMock<IBusinessRuleProvider>();
			businessRuleProvider.Stub(x => x.GetBusinessRulesForShiftTradeRequest(stateHolder, true))
				.IgnoreArguments().Return(businessRules);

			var target = new BusinessRuleConfigProvider(businessRuleProvider, stateHolder, new IShiftTradeSpecification[] { });
			var result = target.GetDefaultConfigForShiftTradeRequest().ToList();
			Assert.IsTrue(result.Any(r => r.BusinessRuleType == typeof(MaximumWorkdayRule).FullName));
			foreach (var shiftTradeBusinessRuleConfig in result)
			{
				if (shiftTradeBusinessRuleConfig.BusinessRuleType == typeof(MaximumWorkdayRule).FullName)
					Assert.IsTrue(shiftTradeBusinessRuleConfig.Enabled == false);
			}
		}

		[Test]
		public void ShouldNotReturnMaximumWorkdayRuleConfig()
		{
			var stateHolder = new FakeSchedulingResultStateHolder_DoNotUse();
			stateHolder.UseMaximumWorkday = false;
			var businessRules = NewBusinessRuleCollection.All(stateHolder);

			var businessRuleProvider = MockRepository.GenerateMock<IBusinessRuleProvider>();
			businessRuleProvider.Stub(x => x.GetBusinessRulesForShiftTradeRequest(stateHolder, true))
				.IgnoreArguments().Return(businessRules);

			var target = new BusinessRuleConfigProvider(businessRuleProvider, stateHolder, new IShiftTradeSpecification[] { });
			var result = target.GetDefaultConfigForShiftTradeRequest().ToList();
			Assert.IsFalse(result.Any(r => r.BusinessRuleType == typeof(MaximumWorkdayRule).FullName));
		}

		[Test]
		public void ShouldReturnShiftTradeTargetTimeSpecificationRuleConfig()
		{
			var stateHolder = new FakeSchedulingResultStateHolder_DoNotUse();
			var businessRules = NewBusinessRuleCollection.All(stateHolder);

			var businessRuleProvider = MockRepository.GenerateMock<IBusinessRuleProvider>();
			businessRuleProvider.Stub(x => x.GetBusinessRulesForShiftTradeRequest(stateHolder, false))
				.IgnoreArguments().Return(businessRules);

			var shiftTradeSpecifications = new IShiftTradeSpecification[]
			{
				new ShiftTradeTargetTimeSpecification(() => new SchedulerStateHolder(stateHolder, null),
					null, null, new FakeTimeZoneGuard())
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
			var stateHolder = new FakeSchedulingResultStateHolder_DoNotUse();
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

			var configurableShiftTradeSpecificationRuleConfig = result.FirstOrDefault(r => r.FriendlyName == "specification2");
			Assert.IsNotNull(configurableShiftTradeSpecificationRuleConfig);
			var nonConfigurableShiftTradeSpecificationRuleConfig = result.FirstOrDefault(r => r.FriendlyName == "specification1");
			Assert.IsNull(nonConfigurableShiftTradeSpecificationRuleConfig);
		}

		[Test(Description = "Verify solution for bug #43527: Strange order of shift trade request settings in Options")]
		public void ShouldReturnDefaultBusinessRulesConfigInSpecifyOrder()
		{
			var stateHolder = new FakeSchedulingResultStateHolder_DoNotUse();
			var businessRules = NewBusinessRuleCollection.All(stateHolder);
			businessRules.Add(new NonMainShiftActivityRule());

			var businessRuleProvider = MockRepository.GenerateMock<IBusinessRuleProvider>();
			businessRuleProvider.Stub(x => x.GetBusinessRulesForShiftTradeRequest(stateHolder, true))
				.IgnoreArguments().Return(businessRules);

			var shiftTradeSpecifications = new IShiftTradeSpecification[]
			{
				new ShiftTradeTargetTimeSpecification(() => new SchedulerStateHolder(stateHolder, null), null, null, new FakeTimeZoneGuard())
			};

			var target = new BusinessRuleConfigProvider(businessRuleProvider, stateHolder, shiftTradeSpecifications);
			var result = target.GetDefaultConfigForShiftTradeRequest().ToList();

			var ruleOrder = new Dictionary<string, int>();
			for (var i = 0; i < result.Count; i++)
			{
				ruleOrder.Add(result[i].BusinessRuleType, i);
			}

			var businessRuleTypes = new[]
			{
				typeof(NewShiftCategoryLimitationRule).FullName,
				typeof(WeekShiftCategoryLimitationRule).FullName,
				typeof(NewNightlyRestRule).FullName,
				typeof(MinWeeklyRestRule).FullName,
				typeof(NewMaxWeekWorkTimeRule).FullName,
				typeof(MinWeekWorkTimeRule).FullName,
				typeof(ShiftTradeTargetTimeSpecification).FullName,
				typeof(NewDayOffRule).FullName,
				typeof(NotOverwriteLayerRule).FullName,
				typeof(NonMainShiftActivityRule).FullName
			};

			for (var i = 0; i < businessRuleTypes.Length - 1; i++)
			{
				Assert.IsTrue(ruleOrder[businessRuleTypes[i]] < ruleOrder[businessRuleTypes[i + 1]]);
			}
		}
	}
}