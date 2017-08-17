using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;

namespace Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades
{
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
					return new ShiftTradeRequestValidationResult(false, true, specification.DenyReason);
				}

				allSpecificationsSatisfied = false;
				if (firstDenyReason == null)
				{
					firstDenyReason = specification.PendingReason;
				}
			}

			return allSpecificationsSatisfied
				? new ShiftTradeRequestValidationResult(true, false, "")
				: new ShiftTradeRequestValidationResult(false, false, firstDenyReason);
		}
	}
}