using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
	public class BusinessRuleConfigProvider : IBusinessRuleConfigProvider
	{
		private readonly IBusinessRuleProvider _businessRuleProvider;
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly IEnumerable<IShiftTradeSpecification> _shiftTradeSpecifications;

		// Refer to bug #43527: Strange order of shift trade request settings in Options
		private readonly Dictionary<Type, int> ruleOrders = new Dictionary<Type, int>
			{
				{typeof(NewShiftCategoryLimitationRule), 0},
				{typeof(WeekShiftCategoryLimitationRule), 10},
				{typeof(NewNightlyRestRule), 20},
				{typeof(MinWeeklyRestRule), 30},
				{typeof(NewMaxWeekWorkTimeRule), 40},
				{typeof(MinWeekWorkTimeRule), 50},
				{typeof(ShiftTradeTargetTimeSpecification), 60},
				{typeof(NewDayOffRule), 70},
				{typeof(NotOverwriteLayerRule), 80},
				{typeof(NonMainShiftActivityRule), 90}
			};

		public BusinessRuleConfigProvider(IBusinessRuleProvider businessRuleProvider,
			ISchedulingResultStateHolder schedulingResultStateHolder, IEnumerable<IShiftTradeSpecification> shiftTradeSpecifications)
		{
			_businessRuleProvider = businessRuleProvider;
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_shiftTradeSpecifications = shiftTradeSpecifications;
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
				HandleOptionOnFailed = RequestHandleOption.Pending,
				Order = getOrder(x.GetType())
			}));
			foreach (var shiftTradeBusinessRuleConfig in result)
			{
				if (shiftTradeBusinessRuleConfig.BusinessRuleType == typeof(MaximumWorkdayRule).FullName)
				{
					shiftTradeBusinessRuleConfig.Enabled = false;
					break;
				}
			}

			result.AddRange(_shiftTradeSpecifications.Where(x => x.Configurable).Select(x => new ShiftTradeBusinessRuleConfig
			{
				BusinessRuleType = x.GetType().FullName,
				FriendlyName = x.Description,
				Enabled = true,
				HandleOptionOnFailed = RequestHandleOption.AutoDeny,
				Order = getOrder(x.GetType())
			}));

			return result.OrderBy(r => r.Order);
		}

		private int getOrder(Type ruleType)
		{
			return ruleOrders.ContainsKey(ruleType) ? ruleOrders[ruleType] : int.MaxValue;
		}
	}
}