using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Win.Common.Configuration
{
	public interface IAlarmControlNamer
	{
		string TreeNodeName();
		string Title();
		string PanelHeader();
	}

	public class AlarmTypeNamer : IAlarmControlNamer
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

	public class RtaRuleNamer : IAlarmControlNamer
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
}