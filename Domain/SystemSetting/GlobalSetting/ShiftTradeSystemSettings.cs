using System;

namespace Teleopti.Ccc.Domain.SystemSetting.GlobalSetting
{
	public enum RequestHandleType
	{
		AutoDeny = 0,
		SetToPending = 1
	}

	[Serializable]
	public class ShiftTradeBusinessRuleConfig
	{
		// Full name of business rule class, use string since Type could not be serialized
		public string BusinessRuleType { get; set; }

		public string FriendlyName { get; set; }

		public bool Enabled { get; set; }
		public RequestHandleType HandleTypeOnBroken { get; set; }
	}

	[Serializable]
	public class ShiftTradeSettings : SettingValue
	{
		public const string SettingsKey = "ShiftTradeSettings";

		private int _maxSeatsValidationSegmentLength = 15;
		private bool _maxSeatsValidationEnabled = false;

		public bool MaxSeatsValidationEnabled
		{
			get { return _maxSeatsValidationEnabled; }
			set { _maxSeatsValidationEnabled = value; }
		}

		public int MaxSeatsValidationSegmentLength
		{
			get { return _maxSeatsValidationSegmentLength; }
			set { _maxSeatsValidationSegmentLength = value; }
		}

		public ShiftTradeBusinessRuleConfig[] BusinessRuleConfigs { get; set; }
	}
}