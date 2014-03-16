using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Controls;

namespace Teleopti.Ccc.Win.Scheduling.SkillResult
{
	public class SkillResultGridControlBase : TeleoptiGridControl, IHelpContext
	{
		public bool HasHelp
		{
			get
			{
				return true;
			}
		}

		public string HelpId
		{
			get
			{
				return Name;
			}
		}
	}
}