using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades
{
	[DisabledBy(Toggles.Wfm_Requests_Configuarable_ShiftTradeTargetTimeSpecification_42450)]
	public class SpecificationChecker : ISpecificationChecker
	{
		private readonly IEnumerable<IShiftTradeSpecification> _shiftTradeSpecifications;

		public SpecificationChecker(IEnumerable<IShiftTradeSpecification> shiftTradeSpecifications)
		{
			_shiftTradeSpecifications = shiftTradeSpecifications;
		}

		public ShiftTradeRequestValidationResult Check(IList<IShiftTradeSwapDetail> swapDetails)
		{
			foreach (var specification in _shiftTradeSpecifications)
			{
				var result = specification.Validate(swapDetails);
				if (!result.IsOk)
				{
					return new ShiftTradeRequestValidationResult(result.IsOk, true, result.DenyReason);
				}
			}
			return new ShiftTradeRequestValidationResult(true);
		}
	}

	[EnabledBy(Toggles.Wfm_Requests_Configuarable_ShiftTradeTargetTimeSpecification_42450)]
	public class SpecificationCheckerWithConfig : ISpecificationChecker
	{
		private readonly IEnumerable<IShiftTradeSpecification> _shiftTradeSpecifications;
		private readonly IGlobalSettingDataRepository _globalSettingDataRepository;

		public SpecificationCheckerWithConfig(IEnumerable<IShiftTradeSpecification> shiftTradeSpecifications,
			IGlobalSettingDataRepository globalSettingDataRepository)
		{
			_shiftTradeSpecifications = shiftTradeSpecifications;
			_globalSettingDataRepository = globalSettingDataRepository;
		}

		public ShiftTradeRequestValidationResult Check(IList<IShiftTradeSwapDetail> swapDetails)
		{
			var businessRuleConfigs = _globalSettingDataRepository.FindValueByKey(ShiftTradeSettings.SettingsKey,
				new ShiftTradeSettings()).BusinessRuleConfigs;

			var allSpecificationsSatisfied = true;
			string firstDenyReason = null;
			foreach (var specification in _shiftTradeSpecifications)
			{
				var ruleConfig =
					businessRuleConfigs?.FirstOrDefault(config => config.BusinessRuleType == specification.GetType().FullName);

				// Specification was disabled by user
				if (ruleConfig != null && !ruleConfig.Enabled) continue;

				var result = specification.Validate(swapDetails);
				if (result.IsOk) continue;

				if (ruleConfig == null ||
					(ruleConfig.HandleOptionOnFailed != null && ruleConfig.HandleOptionOnFailed.Value == RequestHandleOption.AutoDeny))
				{
					return new ShiftTradeRequestValidationResult(false, true, result.DenyReason);
				}

				allSpecificationsSatisfied = false;
				if (firstDenyReason == null)
				{
					firstDenyReason = result.DenyReason;
				}
			}

			return allSpecificationsSatisfied
				? new ShiftTradeRequestValidationResult(true, false, "")
				: new ShiftTradeRequestValidationResult(false, false, firstDenyReason);
		}
	}
}