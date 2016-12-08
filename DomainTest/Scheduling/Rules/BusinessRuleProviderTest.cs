using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

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

	[TestFixture]
	public class ConfigurableBusinessRuleProviderTest
	{
		private IGlobalSettingDataRepository _globalSettingDataRepository;

		[Test]
		public void ShouldGetAllEnabledBusinessRulesForShiftTradeRequest()
		{
			var stateHolder=new FakeSchedulingResultStateHolder();
			setGlobalSettingDataRepository(false, true);

			var target = new ConfigurableBusinessRuleProvider(_globalSettingDataRepository);
			var result = target.GetAllEnabledBusinessRulesForShiftTradeRequest(stateHolder,false);
			var count = target.GetBusinessRulesForShiftTradeRequest(stateHolder, false).Count;
			result.Count(x => x.GetType() == typeof(NewMaxWeekWorkTimeRule)).Should().Be.EqualTo(0);
			result.Count.Should().Be.EqualTo(count-1);
		}

		[Test]
		public void ShouldDenyWhenRuleConfigsHasAnyAutoDeny()
		{
			setGlobalSettingDataRepository(RequestHandleOption.AutoDeny);
			var ruleCollection = createBusinessRuleCollection();
			var ruleResponses = createBusinessRuleResponses();

			var target = new ConfigurableBusinessRuleProvider(_globalSettingDataRepository);
			var result = target.ShouldDeny(ruleCollection, ruleResponses);

			result.Should().Be.True();
		}

		[Test]
		public void ShouldNotDenyWhenRuleConfigsHasNoAutoDeny()
		{
			setGlobalSettingDataRepository(RequestHandleOption.Pending);
			var ruleCollection = createBusinessRuleCollection();
			var ruleResponses = createBusinessRuleResponses();

			var target = new ConfigurableBusinessRuleProvider(_globalSettingDataRepository);
			var result = target.ShouldDeny(ruleCollection, ruleResponses);

			result.Should().Be.False();
		}

		[Test]
		public void ShouldNotDenyWhenNoRuleConfig()
		{
			var ruleCollection = createBusinessRuleCollection();
			var ruleResponses = createBusinessRuleResponses();

			var target = new ConfigurableBusinessRuleProvider(new FakeGlobalSettingDataRepository());
			var result = target.ShouldDeny(ruleCollection, ruleResponses);

			result.Should().Be.False();
		}

		[Test]
		public void ShouldNotDenyWhenNoRuleResponse()
		{
			setGlobalSettingDataRepository(RequestHandleOption.Pending);
			var ruleCollection = createBusinessRuleCollection();

			var target = new ConfigurableBusinessRuleProvider(_globalSettingDataRepository);
			var result = target.ShouldDeny(ruleCollection, null);

			result.Should().Be.False();
		}

		private static List<IBusinessRuleResponse> createBusinessRuleResponses()
		{
			var ruleResponses = new List<IBusinessRuleResponse>();
			var response = new BusinessRuleResponse(typeof(NewBusinessRuleCollectionTest.dummyRule), "no go", false, false,
				new DateTimePeriod(), PersonFactory.CreatePersonWithId(),
				new DateOnlyPeriod(new DateOnly(2008, 12, 22), new DateOnly(2008, 12, 25)), "tjillevippen");
			ruleResponses.Add(response);
			return ruleResponses;
		}

		private static INewBusinessRuleCollection createBusinessRuleCollection()
		{
			var ruleCollection = NewBusinessRuleCollection.Minimum();
			ruleCollection.Add(new NewBusinessRuleCollectionTest.dummyRule(true));
			return ruleCollection;
		}

		private void setGlobalSettingDataRepository(bool isFirstEnabled,bool isSecondEnabled)
		{
			var shiftTradeSetting = new ShiftTradeSettings
			{
				BusinessRuleConfigs = new[]
				{
					new ShiftTradeBusinessRuleConfig()
					{
						BusinessRuleType = typeof(NewMaxWeekWorkTimeRule).FullName,
						Enabled = isFirstEnabled,
						HandleOptionOnFailed = RequestHandleOption.Pending
					},
					new ShiftTradeBusinessRuleConfig()
					{
						BusinessRuleType = typeof(NewNightlyRestRule).FullName,
						Enabled = isSecondEnabled,
						HandleOptionOnFailed = RequestHandleOption.Pending
					}
				}
			};

			_globalSettingDataRepository = MockRepository.GenerateMock<IGlobalSettingDataRepository>();
			_globalSettingDataRepository.Stub(x => x.FindValueByKey(ShiftTradeSettings.SettingsKey, new ShiftTradeSettings()))
				.IgnoreArguments()
				.Return(shiftTradeSetting);
		}

		private void setGlobalSettingDataRepository(RequestHandleOption option)
		{
			var shiftTradeSetting = new ShiftTradeSettings
			{
				BusinessRuleConfigs = new[]
				{
					new ShiftTradeBusinessRuleConfig()
					{
						BusinessRuleType = typeof(NewBusinessRuleCollectionTest.dummyRule).FullName,
						Enabled = true,
						HandleOptionOnFailed = option
					}
				}
			};

			_globalSettingDataRepository = MockRepository.GenerateMock<IGlobalSettingDataRepository>();
			_globalSettingDataRepository.Stub(x => x.FindValueByKey(ShiftTradeSettings.SettingsKey, new ShiftTradeSettings()))
				.IgnoreArguments()
				.Return(shiftTradeSetting);
		}
	}
}