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
		private readonly IEnumerable<IShiftTradeLightSpecification> _shiftTradeLightSpecifications;
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;

		public BusinessRuleConfigProvider(IBusinessRuleProvider businessRuleProvider,
			IEnumerable<IShiftTradeSpecification> shiftTradeSpecifications,
			IEnumerable<IShiftTradeLightSpecification> shiftTradeLightSpecifications,
			ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			_businessRuleProvider = businessRuleProvider;
			_shiftTradeSpecifications = shiftTradeSpecifications;
			_shiftTradeLightSpecifications = shiftTradeLightSpecifications;
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

			if (_shiftTradeSpecifications.Any())
			{
				result.AddRange(_shiftTradeSpecifications.Select(specification => new ShiftTradeBusinessRuleConfig
				{
					BusinessRuleType = specification.GetType().FullName,
					// TODO: Should apply friendly name for specifications
					FriendlyName = specification.GetType().FullName,
					Enabled = true,
					HandleOptionOnFailed = RequestHandleOption.AutoDeny
				})
				);
			}

			if (_shiftTradeLightSpecifications.Any())
			{
				result.AddRange(_shiftTradeLightSpecifications.Select(specification => new ShiftTradeBusinessRuleConfig
				{
					BusinessRuleType = specification.GetType().FullName,
					// TODO: Should apply friendly name for light specifications
					FriendlyName = specification.GetType().FullName,
					Enabled = true,
					HandleOptionOnFailed = RequestHandleOption.AutoDeny
				}));
			}

			return result;
		}
	}
}