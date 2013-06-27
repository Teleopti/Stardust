using System.Linq;
using Teleopti.Ccc.WebBehaviorTest.Core.Legacy;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;
using WatiN.Core;
using WatiN.Core.Constraints;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;

namespace Teleopti.Ccc.WebBehaviorTest.Pages
{
	public class TeamSchedulePage : PortalPage, IDateRangeSelector
	{
		private readonly Constraint _agentConstraint = QuicklyFind.ByClass("teamschedule-agent-name");

		public static int PageSize = 20;

		public Div AgentByName(string name, bool eventualGet = true)
		{
			return eventualGet ? Document.Div(_agentConstraint && Find.ByText(name)).EventualGet() : Document.Div(_agentConstraint && Find.ByText(name));
		}

		public DivCollection Agents()
		{
			return Document.Divs.Filter(_agentConstraint);
		}

		public ListItem RowByAgentName(string name)
		{
			return AgentByName(name).Parent as ListItem;
		}

		public ListItemCollection LayersByAgentName(string name)
		{
			return RowByAgentName(name).ListItems.Filter(QuicklyFind.ByClass("layer"));
		}

		public ListItem DayOffByAgentName(string name)
		{
			return RowByAgentName(name).ListItem(QuicklyFind.ByClass("dayoff"));
		}




		public ListItemCollection TimeLineAll()
		{
			return Document.ListItems.Filter(QuicklyFind.ByClass("teamschedule-timeline-time"));
		}

		public ListItem FirstTimeLineItem()
		{
			return Document.ListItem(QuicklyFind.ByClass("teamschedule-timeline-time"));
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

		public void ClickNext()
		{
			Browser.Interactions.Click("#TeamScheduleDateRangeSelector button:last-of-type");
		}

		public void ClickPrevious()
		{
			Browser.Interactions.Click("#TeamScheduleDateRangeSelector button:first-of-type");
		}

		public Div ToolTipContainer()
		{
			return Document.Div(QuicklyFind.ByClass("tooltip-container"));
		}

		[FindBy(Id = "Team-Picker")]
		public Select2Box TeamPicker { get; set; }
	}
}
