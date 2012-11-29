using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using Teleopti.Ccc.WebBehaviorTest.Pages.jQuery;
using Teleopti.Interfaces.Domain;
using WatiN.Core;
using WatiN.Core.Constraints;

namespace Teleopti.Ccc.WebBehaviorTest.Pages
{
	public class MobileReportsPage : Page
	{
		private readonly Constraint _datePickerContenainerCellConstraint = QuicklyFind.ByClass("ui-datebox-griddate");

		[FindBy(Id = "sel-report-GetAnsweredAndAbandoned")] public RadioButton ReportGetAnsweredAndAbandonedInput;
		[FindBy(Id = "report-graph-holder")] public Div ReportGraphContainer;
		[FindBy(Id = "report-graph-canvas")] public Element ReportGraph;
		[FindBy(Id = "sel-skill-button")] public Link ReportSkillSelectionOpener;
		[FindBy(Id = "report-table-holder")] public Div ReportTableContainer;
		public Table ReportTable{get { return Document.Table(QuicklyFind.ByClass("report-table")); }}
		[FindBy(Id = "report-settings-interval-week")]
		public RadioButton ReportIntervalWeekInput;
		[FindBy(Id = "report-settings-type-graph")] public CheckBox ReportTypeGraphInput;
		[FindBy(Id = "report-settings-type-table")] public CheckBox ReportTypeTableInput;
		[FindBy(Id = "report-view-date-nav-current")] public Link ReportViewNavDate;
		[FindBy(Id = "report-view-date-nav-next")] public Link ReportViewNextDateNavigation;

		[FindBy(Id = "report-view-date-nav-prev")] public Link ReportViewPrevDateNavigation;
		[FindBy(Id = "report-view-show-button")] public Link ReportViewShowButton;

		public TableCell ReportTableFirstDataCell
		{
			get
			{
				ReportTable.WaitUntilExists();
				var firstCell = ReportTable.TableCell(Find.First());
				firstCell.WaitUntilExists();
				return firstCell;
			}
		}

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

		public Link ReportSkillSelectionCloseButton
		{
			get
			{
				// First A in header
				//var headerDiv = ReportSkillSelectionContainer.ChildOfType<Div>(header => header.ClassName.Contains("ui-header"));
				//return headerDiv.ChildOfType<Link>(l => true);

				// no, first A in popup container
				return Document.Link(Find.BySelector(".ui-popup-container a"));
			}
		}

		public Div ReportSelectionDatePickerContainer { get { return Document.Div(QuicklyFind.ByClass("ui-datebox-container")); } }

		// Views
		[FindBy(Id = "home-view")]
		public Div HomeViewContainer { get; set; }

		[FindBy(Id = "report-settings-view")]
		public Div ReportsSettingsViewPageContainer { get; set; }

		[FindBy(Id = "report-view")]
		public Div ReportsViewPageContainer { get; set; }

		public void SetReportSettingsDate(DateOnly date, CultureInfo culture)
		{
			var dateString = date.Date.ToShortDateString(culture);
			// this cant be correct. trigger an avent named datebox?!
			new JQueryExpression()
				.SelectById("sel-date")
				.Trigger("datebox",
				         string.Format(
				         	"{{'method':'set', 'value': '{0}', 'date': new Date({1}, {2} , {3})}}",
				         	dateString, date.Year, date.Month - 1, date.Day))
				.Eval();
		}

		public void SelectSkillByName(string name)
		{
			ReportSkillSelectionList.ListItem(Find.ByText(s => s.Contains(name))).EventualClick();
		}

	}
}