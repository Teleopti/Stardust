using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
	public class BusinessRuleConfigProvider : IBusinessRuleConfigProvider
	{
		private readonly IBusinessRuleProvider _businessRuleProvider;
		private readonly IEnumerable<IShiftTradeSpecification> _shiftTradeSpecifications;
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;

		public BusinessRuleConfigProvider(IBusinessRuleProvider businessRuleProvider,
			IEnumerable<IShiftTradeSpecification> shiftTradeSpecifications,
			ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			_businessRuleProvider = businessRuleProvider;
			_shiftTradeSpecifications = shiftTradeSpecifications;
			_schedulingResultStateHolder = schedulingResultStateHolder;
		}

		public IEnumerable<IShiftTradeBusinessRuleConfig> GetDefaultConfigForShiftTradeRequest()
		{
			var allRules = _businessRuleProvider.GetBusinessRulesForShiftTradeRequest(_schedulingResultStateHolder, true);

			// Rules "removed" from NewBusinessRuleCollection will not be actual removed,
			// Only the "HaltModify" flag will be set to false if not IsMandatory
			var result = allRules.Where(x=> x.IsMandatory || x.HaltModify)
				.Select(x => new ShiftTradeBusinessRuleConfig
			{
				BusinessRuleType = x.GetType().FullName,
				FriendlyName = x.FriendlyName,
				Enabled = true,
				HandleOptionOnFailed = RequestHandleOption.Pending
			}).ToList();

			result.AddRange(_shiftTradeSpecifications.Select(specification => new ShiftTradeBusinessRuleConfig
				{
					BusinessRuleType = specification.GetType().FullName,
					// TODO: Give friendly name for specifications
					FriendlyName = specification.GetType().FullName,
					Enabled = true,
					HandleOptionOnFailed = RequestHandleOption.AutoDeny
				})
			);

			return result;
		}
	}
}