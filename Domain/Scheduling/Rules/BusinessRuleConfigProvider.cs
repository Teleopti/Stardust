using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
	public class BusinessRuleConfigProvider : IBusinessRuleConfigProvider
	{
		private readonly IBusinessRuleProvider _businessRuleProvider;
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;

		public BusinessRuleConfigProvider(IBusinessRuleProvider businessRuleProvider,
			ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			_businessRuleProvider = businessRuleProvider;
			_schedulingResultStateHolder = schedulingResultStateHolder;
		}

		public IEnumerable<IShiftTradeBusinessRuleConfig> GetDefaultConfigForShiftTradeRequest()
		{
			var result = new List<IShiftTradeBusinessRuleConfig>();

			var allRules = _businessRuleProvider.GetBusinessRulesForShiftTradeRequest(_schedulingResultStateHolder, true);
			if (allRules.Any())
			{
				// Rules "removed" from NewBusinessRuleCollection will not be actual removed,
				// Only the "HaltModify" flag will be set to false if not IsMandatory
				result.AddRange(allRules.Where(x => x.IsMandatory || x.HaltModify)
					.Select(x => new ShiftTradeBusinessRuleConfig
					{
						BusinessRuleType = x.GetType().FullName,
						FriendlyName = x.Description,
						Enabled = true,
						HandleOptionOnFailed = RequestHandleOption.Pending
					}));
			}

			return result;
		}
	}
}