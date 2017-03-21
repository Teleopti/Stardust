using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.SystemSetting.GlobalSetting
{
	[Serializable]
	public class ShiftTradeBusinessRuleConfig : IShiftTradeBusinessRuleConfig
	{
		/// <summary>
		/// Full name of business rule class, use string since Type could not be serialized.
		/// </summary>
		public string BusinessRuleType { get; set; }

		public string FriendlyName { get; set; }

		public bool Enabled { get; set; }

		public RequestHandleOption? HandleOptionOnFailed { get; set; }

		public int Order { get; set; }
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