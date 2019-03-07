using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;


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
				new FakeSchedulingResultStateHolder_DoNotUse(), true);

			Assert.AreEqual(rules.Count, 14);

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

	[TestFixture]
	public class ConfigurableBusinessRuleProviderTest
	{
		private IGlobalSettingDataRepository _globalSettingDataRepository;

		[Test]
		public void ShouldGetAllEnabledBusinessRulesForShiftTradeRequest()
		{
			var stateHolder = new FakeSchedulingResultStateHolder_DoNotUse();
			switchRuleInSetting(new Dictionary<Type, bool>
			{
				{typeof(NewMaxWeekWorkTimeRule), false},
				{typeof(NewNightlyRestRule), true}
			});

			var target = new ConfigurableBusinessRuleProvider(_globalSettingDataRepository);
			var result = target.GetAllEnabledBusinessRulesForShiftTradeRequest(stateHolder, false);
			var count = target.GetBusinessRulesForShiftTradeRequest(stateHolder, false).Count;
			result.Count(x => x.GetType() == typeof(NewMaxWeekWorkTimeRule)).Should().Be.EqualTo(0);
			result.Count(x => x.GetType() == typeof(NewNightlyRestRule)).Should().Be.EqualTo(1);
			result.Count.Should().Be.EqualTo(count - 1);
		}

		[Test]
		public void ShouldGetAllBusinessRulesForShiftTradeRequestIfNotConfigured()
		{
			var stateHolder = new FakeSchedulingResultStateHolder_DoNotUse();

			setShiftTradeSetting(new ShiftTradeSettings {BusinessRuleConfigs = null});
			var target = new ConfigurableBusinessRuleProvider(_globalSettingDataRepository);
			var allRules = target.GetBusinessRulesForShiftTradeRequest(stateHolder, true);
			var enabledRules = target.GetAllEnabledBusinessRulesForShiftTradeRequest(stateHolder, true);

			enabledRules.Count.Should().Be.EqualTo(allRules.Count);
			foreach (var rule in allRules)
			{
				enabledRules.Item(rule.GetType()).Should().Not.Be(null);
			}
		}

		[Test]
		public void ShouldReturnResponseWhenAnyRuleSetAsAutoDeny()
		{
			setDummyRuleHanleOption(RequestHandleOption.AutoDeny);
			var ruleCollection = createBusinessRuleCollection();
			var ruleResponses = createBusinessRuleResponses(typeof(DummyRule));

			var target = new ConfigurableBusinessRuleProvider(_globalSettingDataRepository);
			var result = target.GetFirstDeniableResponse(ruleCollection, ruleResponses);

			result.Should().Not.Be(null);
		}

		[Test]
		public void ShouldReturnNothingWhenNoRuleSetAsAutoDeny()
		{
			setDummyRuleHanleOption(RequestHandleOption.Pending);
			var ruleCollection = createBusinessRuleCollection();
			var ruleResponses = createBusinessRuleResponses(typeof(DummyRule));

			var target = new ConfigurableBusinessRuleProvider(_globalSettingDataRepository);
			var result = target.GetFirstDeniableResponse(ruleCollection, ruleResponses);

			result.Should().Be(null);
		}

		[Test]
		public void ShouldReturnNothingWhenRuleNotFailed()
		{
			setDummyRuleHanleOption(RequestHandleOption.AutoDeny);
			var ruleCollection = createBusinessRuleCollection();
			var ruleResponses = createBusinessRuleResponses(typeof(AnotherDummyRule));

			var target = new ConfigurableBusinessRuleProvider(_globalSettingDataRepository);
			var result = target.GetFirstDeniableResponse(ruleCollection, ruleResponses);

			result.Should().Be(null);
		}

		[Test]
		public void ShouldReturnNothingWhenNoRuleConfig()
		{
			var ruleCollection = createBusinessRuleCollection();
			var ruleResponses = createBusinessRuleResponses(typeof(DummyRule));

			var target = new ConfigurableBusinessRuleProvider(new FakeGlobalSettingDataRepository());
			var result = target.GetFirstDeniableResponse(ruleCollection, ruleResponses);

			result.Should().Be(null);
		}

		[Test]
		public void ShouldReturnNothingWhenNoRuleResponse()
		{
			setDummyRuleHanleOption(RequestHandleOption.Pending);
			var ruleCollection = createBusinessRuleCollection();

			var target = new ConfigurableBusinessRuleProvider(_globalSettingDataRepository);
			var result = target.GetFirstDeniableResponse(ruleCollection, null);

			result.Should().Be(null);
		}

		private static List<IBusinessRuleResponse> createBusinessRuleResponses(Type ruleType)
		{
			var ruleResponses = new List<IBusinessRuleResponse>();
			var response = new BusinessRuleResponse(ruleType, "no go", false, false,
				new DateTimePeriod(), PersonFactory.CreatePersonWithId(),
				new DateOnlyPeriod(new DateOnly(2008, 12, 22), new DateOnly(2008, 12, 25)), "tjillevippen");
			ruleResponses.Add(response);
			return ruleResponses;
		}

		private static INewBusinessRuleCollection createBusinessRuleCollection()
		{
			var ruleCollection = NewBusinessRuleCollection.Minimum();
			ruleCollection.Add(new DummyRule(true));
			ruleCollection.Add(new AnotherDummyRule(false));
			return ruleCollection;
		}

		private void switchRuleInSetting(Dictionary<Type, bool> ruleDictionary)
		{
			var businessRuleConfigs = ruleDictionary.Select(x => new ShiftTradeBusinessRuleConfig
			{
				BusinessRuleType = x.Key.FullName,
				Enabled = x.Value,
				HandleOptionOnFailed = RequestHandleOption.Pending
			});

			var shiftTradeSetting = new ShiftTradeSettings
			{
				BusinessRuleConfigs = businessRuleConfigs.ToArray()
			};

			setShiftTradeSetting(shiftTradeSetting);
		}

		private void setDummyRuleHanleOption(RequestHandleOption option)
		{
			var shiftTradeSetting = new ShiftTradeSettings
			{
				BusinessRuleConfigs = new[]
				{
					new ShiftTradeBusinessRuleConfig
					{
						BusinessRuleType = typeof(DummyRule).FullName,
						Enabled = true,
						HandleOptionOnFailed = option
					},
					new ShiftTradeBusinessRuleConfig
					{
						BusinessRuleType = typeof(AnotherDummyRule).FullName,
						Enabled = true,
						HandleOptionOnFailed = RequestHandleOption.Pending
					}
				}
			};

			setShiftTradeSetting(shiftTradeSetting);
		}

		private void setShiftTradeSetting(ShiftTradeSettings shiftTradeSetting)
		{
			_globalSettingDataRepository = MockRepository.GenerateMock<IGlobalSettingDataRepository>();
			_globalSettingDataRepository.Stub(x => x.FindValueByKey(ShiftTradeSettings.SettingsKey, new ShiftTradeSettings()))
				.IgnoreArguments()
				.Return(shiftTradeSetting);
		}
	}
}