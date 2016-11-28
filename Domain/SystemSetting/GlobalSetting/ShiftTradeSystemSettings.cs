using System;
using System.Linq;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.SystemSetting.GlobalSetting
{
	[Serializable]
	public class ShiftTradeBusinessRuleConfig : IShiftTradeBusinessRuleConfig
	{
		// Full name of business rule class, use string since Type could not be serialized
		public string BusinessRuleType { get; set; }

		public string FriendlyName { get; set; }

		public bool Enabled { get; set; }
		public RequestHandleOption HandleOptionOnFailed { get; set; }
	}

	[Serializable]
	public class ShiftTradeSettings : SettingValue
	{
		public const string SettingsKey = "ShiftTradeSettings";

		private int _maxSeatsValidationSegmentLength = 15;
		private bool _maxSeatsValidationEnabled;

		public bool MaxSeatsValidationEnabled
		{
			get { return _maxSeatsValidationEnabled || isEnabledInBusinessRuleConfigs(); }
			set { _maxSeatsValidationEnabled = value; }
		}

		public int MaxSeatsValidationSegmentLength
		{
			get { return _maxSeatsValidationSegmentLength; }
			set { _maxSeatsValidationSegmentLength = value; }
		}

		public ShiftTradeBusinessRuleConfig[] BusinessRuleConfigs { get; set; }

		private bool isEnabledInBusinessRuleConfigs()
		{
			if (BusinessRuleConfigs == null || !BusinessRuleConfigs.Any())
				return false;

			var shiftTradeMaxSeatsSpecificationRule
				= BusinessRuleConfigs.FirstOrDefault(b => b.BusinessRuleType == typeof(ShiftTradeMaxSeatsSpecification).FullName);

			return shiftTradeMaxSeatsSpecificationRule != null && shiftTradeMaxSeatsSpecificationRule.Enabled;
		}
	}
}