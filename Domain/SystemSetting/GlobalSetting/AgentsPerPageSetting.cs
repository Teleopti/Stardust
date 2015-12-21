using System;

namespace Teleopti.Ccc.Domain.SystemSetting.GlobalSetting
{
	[Serializable]
	public class AgentsPerPageSetting : SettingValue
	{
		public int AgentsPerPage { get; set; }
	}
}
