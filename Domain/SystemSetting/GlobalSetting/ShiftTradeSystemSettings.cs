using System;

namespace Teleopti.Ccc.Domain.SystemSetting.GlobalSetting
{
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


		

	}
}