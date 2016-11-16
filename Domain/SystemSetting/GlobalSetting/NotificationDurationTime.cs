using System;

namespace Teleopti.Ccc.Domain.SystemSetting.GlobalSetting
{
	[Serializable]
	public class NotificationDurationTime : SettingValue
	{
		private int _durationInSecond = 30;
		public int DurationInSecond
		{
			get { return _durationInSecond; }
			set { _durationInSecond = value; }
		}
	}
}