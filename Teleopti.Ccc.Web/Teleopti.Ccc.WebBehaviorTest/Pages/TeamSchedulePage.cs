using System.Linq;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;
using WatiN.Core;
using WatiN.Core.Constraints;

namespace Teleopti.Ccc.WebBehaviorTest.Pages
{
	public class TeamSchedulePage : PortalPage, IDateRangeSelector
	{
		private readonly Constraint AgentConstraint = Find.ByClass("teamschedule-agent-name", false);

		public static int PageSize = 20;

		public Div AgentByName(string name)
		{
			return Document.Div(AgentConstraint && Find.ByText(name));
		}

		public DivCollection Agents()
		{
			return Document.Divs.Filter(AgentConstraint);
		}

		public ListItem RowByAgentName(string name)
		{
			return AgentByName(name).Parent as ListItem;
		}

		public ListItemCollection LayersByAgentName(string name)
		{
			return RowByAgentName(name).ListItems.Filter(Find.ByClass("layer", false));
		}

		public ListItem DayOffByAgentName(string name)
		{
			return RowByAgentName(name).ListItem(Find.ByClass("dayoff", false));
		}




		public ListItemCollection TimeLineAll()
		{
			return Document.ListItems.Filter(Find.ByClass("teamschedule-timeline-time", false));
		}

		public ListItem FirstTimeLineItem()
		{
			return Document.ListItem(Find.ByClass("teamschedule-timeline-time", false));
		}

		public ListItem LastTimeLineItem()
		{
			return TimeLineAll().Last();
		}

		[FindBy(Id = "TeamScheduleDateRangeSelector")]
		public Div DateRangeSelectorContainer { get; set; }

		[FindBy(Id = "TeamScheduleDatePicker")]
		public DatePicker DatePicker { get; set; }

		public Button NextPeriodButton
		{
			get
			{
				return DateRangeSelectorContainer.Buttons.Last();
			}
		}

		public Button PreviousPeriodButton
		{
			get
			{
				return DateRangeSelectorContainer.Buttons.First();
			}
		}

		public Div ToolTipContainer()
		{
			return Document.Div(Find.ByClass("tooltip-container", false));
		}

		[FindBy(Id = "TeamSchedule-TeamPicker-select-container")]
		public SelectBox TeamPicker { get; set; }
	}
}
