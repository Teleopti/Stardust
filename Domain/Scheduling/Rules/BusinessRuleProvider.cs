using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.SiteOpenHours;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
	public class BusinessRuleProvider : IBusinessRuleProvider
	{
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

		public virtual INewBusinessRuleCollection GetAllEnabledBusinessRulesForShiftTradeRequest(ISchedulingResultStateHolder schedulingResultStateHolder,
			bool enableSiteOpenHoursRule)
		{
			return GetBusinessRulesForShiftTradeRequest(schedulingResultStateHolder, enableSiteOpenHoursRule);
		}

		public virtual IBusinessRuleResponse GetFirstDeniableResponse(INewBusinessRuleCollection enabledRules, IList<IBusinessRuleResponse> ruleResponses)
		{
			return null;
		}
	}

	public class ConfigurableBusinessRuleProvider : BusinessRuleProvider
	{
		private readonly IGlobalSettingDataRepository _globalSettingDataRepository;

		public ConfigurableBusinessRuleProvider(IGlobalSettingDataRepository globalSettingDataRepository)
		{
			_globalSettingDataRepository = globalSettingDataRepository;
		}

		public override INewBusinessRuleCollection GetAllEnabledBusinessRulesForShiftTradeRequest(ISchedulingResultStateHolder schedulingResultStateHolder,
			bool enableSiteOpenHoursRule)
		{
			var rules = GetBusinessRulesForShiftTradeRequest(schedulingResultStateHolder, enableSiteOpenHoursRule);

			var ruleConfigs = _globalSettingDataRepository.FindValueByKey(ShiftTradeSettings.SettingsKey,
				new ShiftTradeSettings()).BusinessRuleConfigs;

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

		public override IBusinessRuleResponse GetFirstDeniableResponse(INewBusinessRuleCollection enabledRules, IList<IBusinessRuleResponse> ruleResponses)
		{
			if (ruleResponses == null || !ruleResponses.Any()) return null;

			var shiftTradeSetting = _globalSettingDataRepository.FindValueByKey(ShiftTradeSettings.SettingsKey, new ShiftTradeSettings());
			var ruleConfigs = shiftTradeSetting.BusinessRuleConfigs;
			if (ruleConfigs == null || !ruleConfigs.Any()) return null;

			var brokenRules = enabledRules.Where(rule => ruleResponses.Any(r => r.TypeOfRule == rule.GetType())).ToList();
			if (!brokenRules.Any()) return null;

			foreach (var rule in brokenRules)
			{
				var config = ruleConfigs.FirstOrDefault(c => c.BusinessRuleType == rule.GetType().FullName);
				if (config?.HandleOptionOnFailed != null && config.HandleOptionOnFailed.Value == RequestHandleOption.AutoDeny)
				{
					return ruleResponses.FirstOrDefault(r=>r.TypeOfRule == rule.GetType());
				}
			}
			return null;
		}
	}
}