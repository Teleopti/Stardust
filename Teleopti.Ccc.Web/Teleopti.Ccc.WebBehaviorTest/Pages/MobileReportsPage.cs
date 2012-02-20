using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.WebBehaviorTest.Pages.jQuery;
using Teleopti.Interfaces.Domain;
using WatiN.Core;
using WatiN.Core.Constraints;
using WatiN.Core.Native;

namespace Teleopti.Ccc.WebBehaviorTest.Pages
{
	public class MobileReportsPage : Page
	{
		private readonly Constraint _datePickerContenainerCellConstraint = Find.ByClass("ui-datebox-griddate", false);
		                            // By("data-date", v => v != null);

		[FindBy(Id = "sel-report-GetAnsweredAndAbandoned")] public RadioButton ReportGetAnsweredAndAbandonedInput;
		[FindBy(Id = "report-graph-holder")] public Div ReportGraphContainer;
		[FindBy(Id = "report-graph-canvas")] public Element ReportGraph;
		[FindBy(Id = "sel-skill-button")] public Link ReportSkillSelectionOpener;
		[FindBy(Id = "report-table-holder")] public Div ReportTableContainer;
		[FindBy(Id = "report-settings-type-graph")] public CheckBox ReportTypeGraphInput;
		[FindBy(Id = "report-settings-type-table")] public CheckBox ReportTypeTableInput;
		[FindBy(Id = "report-view-date-nav-current")] public Link ReportViewNavDate;
		[FindBy(Id = "report-view-date-nav-next")] public Link ReportViewNextDateNavigation;

		[FindBy(Id = "report-view-date-nav-prev")] public Link ReportViewPrevDateNavigation;
		[FindBy(Id = "report-view-show-button")] public Link ReportViewShowButton;

		public RadioButtonCollection Reports
		{
			get { return Document.RadioButtons.Filter(Find.ByName("sel-report")); }
		}

		private DivCollection DatePickerContenainerDayDivs
		{
			get { return ReportSelectionDatePickerContainer.Divs.Filter(_datePickerContenainerCellConstraint); }
		}

		public IEnumerable<Div> DatePickerContenainerDays
		{
			get { return DatePickerContenainerDayDivs; }
		}

		public Div FirstDatePickerDay
		{
			get { return DatePickerContenainerDays.First(); }
		}

		public Div LastDatePickerDay
		{
			get { return DatePickerContenainerDays.Last(); }
		}

		public Div AnyDatePickerDay
		{
			get { return DatePickerContenainerDays.Skip(10).First(); }
		}

		public ListItem ThirdSkillInSkillList
		{
			get { return ReportSkillSelectionList.ListItems.Skip(2).First(); }
		}


		[FindBy(Id = "signout-button")]
		public Link SignoutButton { get; set; }


		// Setting controls
		[FindBy(Id = "sel-date")]
		public Element ReportSelectionDateInput { get; set; }


		public string ReportSelectionDateValue
		{
			get { return ReportSelectionDateInput.GetAttributeValue("value"); }
			set { ReportSelectionDateInput.SetAttributeValue("value", value); }
		}


		public Link ReportSelectionDateOpener
		{
			get { return ReportSelectionDateInput.NextSibling as Link; }
		}


		public Label ReportGetAnsweredAndAbandoned
		{
			get { return ReportGetAnsweredAndAbandonedInput.NextSibling as Label; }
		}

		public Div ReportTypeGraph
		{
			get { return ReportTypeGraphInput.Parent as Div; }
		}

		public Div ReportTypeTable
		{
			get { return ReportTypeTableInput.Parent as Div; }
		}

		[FindBy(Id = "sel-skill-menu")]
		public List ReportSkillSelectionList { get; set; }


		public Div ReportSkillSelectionContainer
		{
			get
			{
				return
					(ReportSkillSelectionList.Parent.ClassName.Contains("ui-selectmenu")
					 	? ReportSkillSelectionList.Parent
					 	: ReportSkillSelectionList.Parent.Parent) as Div;
			}
		}

		public Link ReportSkillSelectionCloseButton
		{
			// First A in header
			get
			{
				var headerDiv = ReportSkillSelectionContainer.ChildOfType<Div>(header => header.ClassName.Contains("ui-header"));
				return headerDiv.ChildOfType<Link>(l => true);
			}
		}

		[FindBy(ClassRegex = "^ui-datebox-container .*")]
		public Div ReportSelectionDatePickerContainer { get; set; }

		// Views
		[FindBy(Id = "home-view")]
		public Div HomeViewContainer { get; set; }

		[FindBy(Id = "report-settings-view")]
		public Div ReportsSettingsViewPageContainer { get; set; }

		[FindBy(Id = "report-view")]
		public Div ReportsViewPageContainer { get; set; }

		public string ReportCurrentDate
		{
			get
			{
				return ReportViewNavDate.Text;
			}
		}

		public string ThirdSkillName { get; set; }

		public void SetReportSettingsDate(DateOnly date)
		{
			new JQueryExpression().SelectById("sel-date").Trigger("datebox",
			                                                      string.Format(
			                                                      	"{{'method':'set', 'value': '{0:yyyy-MM-dd}', 'date': new Date({1}, {2} , {3})}}",
																	(DateTime)date, date.Year, date.Month - 1, date.Day)).Eval();
		}
	}
}