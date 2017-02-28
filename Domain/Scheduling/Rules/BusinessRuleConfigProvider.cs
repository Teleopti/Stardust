using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
	public class BusinessRuleConfigProvider : IBusinessRuleConfigProvider
	{
		private readonly IBusinessRuleProvider _businessRuleProvider;
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly IEnumerable<IShiftTradeSpecification> _shiftTradeSpecifications;

		public BusinessRuleConfigProvider(IBusinessRuleProvider businessRuleProvider,
			ISchedulingResultStateHolder schedulingResultStateHolder, IEnumerable<IShiftTradeSpecification> shiftTradeSpecifications)
		{
			_businessRuleProvider = businessRuleProvider;
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_shiftTradeSpecifications = shiftTradeSpecifications;
			_schedulingResultStateHolder.UseMinWeekWorkTime = true;
		}

		public IEnumerable<IShiftTradeBusinessRuleConfig> GetDefaultConfigForShiftTradeRequest()
		{
			var result = new List<IShiftTradeBusinessRuleConfig>();

			var allRules = _businessRuleProvider.GetBusinessRulesForShiftTradeRequest(_schedulingResultStateHolder, true);
			if (!allRules.Any()) return result;

			var configurableRules = allRules.Where(x => x.Configurable && (x.IsMandatory || x.HaltModify));
			result.AddRange(configurableRules.Select(x => new ShiftTradeBusinessRuleConfig
			{
				BusinessRuleType = x.GetType().FullName,
				FriendlyName = x.Description,
				Enabled = true,
				HandleOptionOnFailed = RequestHandleOption.Pending
			}));

			result.AddRange(_shiftTradeSpecifications.Where(x => x.Configurable).Select(x => new ShiftTradeBusinessRuleConfig
			{
				BusinessRuleType = x.GetType().FullName,
				FriendlyName = x.Description,
				Enabled = true,
				HandleOptionOnFailed = RequestHandleOption.AutoDeny
			}));

			return result;
		}
	}

	public class BusinessRuleConfigProvider25635ToggleOff : BusinessRuleConfigProvider
	{
		public BusinessRuleConfigProvider25635ToggleOff(IBusinessRuleProvider businessRuleProvider,
			ISchedulingResultStateHolder schedulingResultStateHolder,
			IEnumerable<IShiftTradeSpecification> shiftTradeSpecifications)
			: base(businessRuleProvider, schedulingResultStateHolder, shiftTradeSpecifications)
		{
			schedulingResultStateHolder.UseMinWeekWorkTime = false;
		}
	}
}