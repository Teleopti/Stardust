using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;
using WatiN.Core;
using WatiN.Core.Constraints;

namespace Teleopti.Ccc.WebBehaviorTest.Pages
{
	public class TeamSchedulePage : PortalPage, IDateRangeSelector
	{
		private readonly Constraint AgentConstraint = QuicklyFind.ByClass("teamschedule-agent-name");

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

		public Div ToolTipContainer()
		{
			return Document.Div(QuicklyFind.ByClass("tooltip-container"));
		}

		[FindBy(Id = "Team-Picker")]
		public TextField TeamPickerInput { get; set; }

		[FindBy(Id = "s2id_Team-Picker")]
		public Div TeamPickerSelectDiv { get; set; }

		[FindBy(Class = "team-select2-dropdown")]
		public Div TeamPickerDropDown { get; set; }

		public IEnumerable<string> TeamPickerSelectTexts()
		{
			var items = Document.ListItems.Filter(QuicklyFind.ByClass("select2-result-selectable"));
			return from item in items select item.Div(QuicklyFind.ByClass("select2-result-label")).Text;
		}

		public Link TeamPickerSelectLink()
		{
			return TeamPickerSelectDiv.Link(QuicklyFind.ByClass("select2-choice"));
		}
	}
}
