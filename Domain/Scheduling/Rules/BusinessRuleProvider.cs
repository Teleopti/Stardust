using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.SiteOpenHours;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
	public class BusinessRuleProvider : IBusinessRuleProvider
	{
		public INewBusinessRuleCollection GetAllBusinessRules(ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			return NewBusinessRuleCollection.All(schedulingResultStateHolder);
		}

		public INewBusinessRuleCollection GetBusinessRulesForShiftTradeRequest(ISchedulingResultStateHolder schedulingResultStateHolder,
			bool enableSiteOpenHoursRule)
		{
			var rules = NewBusinessRuleCollection.All(schedulingResultStateHolder);
			rules.DoNotHaltModify(typeof(NewPersonAccountRule));
			rules.DoNotHaltModify(typeof(OpenHoursRule));
			rules.Add(new NonMainShiftActivityRule());
			if (enableSiteOpenHoursRule)
				rules.Add(new SiteOpenHoursRule(new SiteOpenHoursSpecification()));

			return rules;
		}

		public INewBusinessRuleCollection GetAllEnabledBusinessRulesForShiftTradeRequest(ISchedulingResultStateHolder schedulingResultStateHolder,
			bool enableSiteOpenHoursRule)
		{
			return GetBusinessRulesForShiftTradeRequest(schedulingResultStateHolder, enableSiteOpenHoursRule);
		}

		public bool ShouldDeny(INewBusinessRuleCollection enabledRules, IList<IBusinessRuleResponse> ruleResponses)
		{
			return false;
		}
	}

	public class ConfigurableBusinessRuleProvider : BusinessRuleProvider
	{
		private readonly IGlobalSettingDataRepository _globalSettingDataRepository;
		public ConfigurableBusinessRuleProvider(IGlobalSettingDataRepository globalSettingDataRepository)
		{
			_globalSettingDataRepository = globalSettingDataRepository;
		}

		public new INewBusinessRuleCollection GetAllEnabledBusinessRulesForShiftTradeRequest(ISchedulingResultStateHolder schedulingResultStateHolder,
			bool enableSiteOpenHoursRule)
		{
			var rules = GetBusinessRulesForShiftTradeRequest(schedulingResultStateHolder, enableSiteOpenHoursRule);

			var shiftTradeSetting = _globalSettingDataRepository.FindValueByKey(ShiftTradeSettings.SettingsKey,
				new ShiftTradeSettings());
			var ruleConfigs = shiftTradeSetting.BusinessRuleConfigs;

			var enabledRules = ruleConfigs != null && ruleConfigs.Any()
				? rules.Where(r =>
				{
					var ruleConfig = ruleConfigs.FirstOrDefault(c => c.BusinessRuleType == r.GetType().FullName);
					return ruleConfig == null || ruleConfig.Enabled;
				})
				: rules;

			var result = NewBusinessRuleCollection.Minimum();
			foreach (var rule in enabledRules)
			{
				if(result.All(x => x.GetType() != rule.GetType())) result.Add(rule);
			}

			return result;
		}

		public new bool ShouldDeny(INewBusinessRuleCollection enabledRules, IList<IBusinessRuleResponse> ruleResponses)
		{
			if (ruleResponses == null || !ruleResponses.Any()) return false;

			var shiftTradeSetting = _globalSettingDataRepository.FindValueByKey(ShiftTradeSettings.SettingsKey, new ShiftTradeSettings());
			var ruleConfigs = shiftTradeSetting.BusinessRuleConfigs;
			if (ruleConfigs == null || !ruleConfigs.Any()) return false;

			foreach (var rule in enabledRules)
			{
				var config = ruleConfigs.FirstOrDefault(c => c.BusinessRuleType == rule.GetType().FullName);
				var handleOption = config?.HandleOptionOnFailed;
				if (handleOption != null && handleOption.Value == RequestHandleOption.AutoDeny) return true;
			}
			return false;
		}
	}
}