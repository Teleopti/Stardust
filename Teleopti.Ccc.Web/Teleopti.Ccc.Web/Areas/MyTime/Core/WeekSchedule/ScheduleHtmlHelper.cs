using System.Text;
using System.Web;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.PeriodSelection;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Schedule.Common;
using Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule
{
	public class ScheduleHtmlHelper
	{
		public string FormatDate(DateOnly dateOnly)
		{
			return dateOnly.ToFixedClientDateOnlyFormat();
		}

		public string FormatState(SpecialDateState state)
		{
			return ((int) state).ToString();
		}

		public string StyleClassToday(DayViewModelBase model)
		{
			if (model.Date.Equals(DateOnly.Today))
				return "today";
			return string.Empty;
		}

		public string PeriodSelectionAsJson(PeriodSelectionViewModel peroidSelection)
		{
			// Make this manual, JSonScriptSerializer isn't very helpfull anyway.
			var jsonResult = new StringBuilder();
			jsonResult.Append("{");
			addKeyValuePair("Date", peroidSelection.Date, jsonResult, true, true);
			addKeyValuePair("StartDate", peroidSelection.StartDate.Date.ToShortDateString(), jsonResult, true, true);
			addKeyValuePair("EndDate", peroidSelection.EndDate.Date.ToShortDateString(), jsonResult, true, true);
			addKeyValuePair("Display", peroidSelection.Display, jsonResult, true, true);
			addKeyValuePair("SelectedDateRange", PeriodDateRangeToJson(peroidSelection.SelectedDateRange), jsonResult, false,
			                true);
			addKeyValuePair("SelectableDateRange", PeriodDateRangeToJson(peroidSelection.SelectableDateRange), jsonResult, false,
			                true);
			addKeyValuePair("PeriodNavigation", PeriodNavigation(peroidSelection.PeriodNavigation), jsonResult, false, false);
			jsonResult.Append("}");
			return jsonResult.ToString();
		}

		protected string PeriodDateRangeToJson(PeriodDateRangeViewModel rangeSelection)
		{
			var jsonResult = new StringBuilder();
			jsonResult.Append("{");
			addKeyValuePair("MinDate", rangeSelection.MinDate, jsonResult, true, true);
			addKeyValuePair("MaxDate", rangeSelection.MaxDate, jsonResult, true, false);
			jsonResult.Append("}");
			return jsonResult.ToString();
		}

		protected string PeriodNavigation(PeriodNavigationViewModel peroidNavigation)
		{
			var jsonResult = new StringBuilder();
			jsonResult.Append("{");
			addKeyValuePair("NextPeriod", peroidNavigation.NextPeriod, jsonResult, true, true);
			addKeyValuePair("HasNextPeriod", formatBoolean(peroidNavigation.HasNextPeriod), jsonResult, false, true);
			addKeyValuePair("PrevPeriod", peroidNavigation.PrevPeriod, jsonResult, true, true);
			addKeyValuePair("HasPrevPeriod", formatBoolean(peroidNavigation.HasPrevPeriod), jsonResult, false, true);
			addKeyValuePair("CanPickPeriod", formatBoolean(peroidNavigation.CanPickPeriod), jsonResult, false, false);
			jsonResult.Append("}");
			return jsonResult.ToString();
		}

		private static string formatBoolean(bool value)
		{
			return value ? "true" : "false";
		}

		private static void addKeyValuePair(string key, string value, StringBuilder jsonResult, bool escapeValue,
		                                    bool appendWithComma)
		{
			jsonResult.AppendFormat("{0}: {1}{2}", HttpUtility.JavaScriptStringEncode(key, true), escapeValue
			                                                                                      	? HttpUtility.
			                                                                                      	  	JavaScriptStringEncode(
			                                                                                      	  		value, true)
			                                                                                      	: value,
			                        appendWithComma ? "," : string.Empty);
		}
	}
}