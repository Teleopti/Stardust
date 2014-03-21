using System;

namespace Teleopti.Ccc.Domain.SystemSetting.GlobalSetting
{
	[Serializable]
	public class AsmAlertTime : SettingValue
	{
		private int _secondsBeforeChange = 60;

		public int SecondsBeforeChange
		{
			get { return _secondsBeforeChange; }
			set { _secondsBeforeChange = value; }
		} 
	}
}