using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Win.Common.Configuration
{
	public interface IRtaControlNamer
	{
		string TreeNodeName();
		string Title();
		string PanelHeader();
	}
	
	public class AlarmTypeNamer : IRtaControlNamer
	{
		public string TreeNodeName()
		{
			return Resources.AlarmTypes;
		}

		public string Title()
		{
			return Resources.ManageAlarmTypes;
		}

		public string PanelHeader()
		{
			return Resources.AlarmTypes;
		}
	}

	public class RtaRuleNamer : IRtaControlNamer
	{
		public string TreeNodeName()
		{
			return Resources.Rules;
		}

		public string Title()
		{
			return Resources.ManageRules;
		}

		public string PanelHeader()
		{
			return Resources.Rules;
		}
	}

	public class RtaMapNamer : IRtaControlNamer
	{
		public string TreeNodeName()
		{
			return Resources.Mappings;
		}

		public string Title()
		{
			return Resources.ManageMappings;
		}

		public string PanelHeader()
		{
			return Resources.Mappings;
		}
	}

	public class AlarmsNamer : IRtaControlNamer
	{
		public string TreeNodeName()
		{
			return Resources.Alarms;
		}

		public string Title()
		{
			return Resources.ManageAlarms;
		}

		public string PanelHeader()
		{
			return Resources.Alarms;
		}
	}
}