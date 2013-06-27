using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core.Legacy;
using Teleopti.Ccc.WebBehaviorTest.Pages.jQuery;
using WatiN.Core;
using WatiN.Core.Constraints;

namespace Teleopti.Ccc.WebBehaviorTest.Pages.Common
{
	public class CalendarCellsPage : PortalPage
	{
		private Constraint CalendarCellConstraint = Find.By("data-mytime-date", v => v != null);
		
		private ListItemCollection CalendarCellsListItems { get { return Document.ListItems.Filter(CalendarCellConstraint); } }

		public IEnumerable<ListItem> CalendarCells { get { return CalendarCellsListItems; } }
		
		public ListItem FirstCalendarCell { get { return Document.ListItem(CalendarCellConstraint).EventualGet(); } }
		
		public ListItem LastCalendarCell { get { return CalendarCellsListItems.Last(); } }

		public ListItem CalendarCellForDate(string formattedDate)
		{
			return Document.ListItem(Find.BySelector(DateSelector(formattedDate))).EventualGet();
		}

		public ListItem CalendarCellForDate(DateTime date)
		{
			return Document.ListItem(Find.BySelector(DateSelector(date))).EventualGet();
		}

		public static string DateSelector(string formattedDate) { return "[data-mytime-date=" + formattedDate + "]"; }
		
		public static string DateSelector(DateTime date) { return DateSelector(date.ToString("yyyy-MM-dd")); }

		public Div CalendarCellDataForDate(DateTime date, string className)
		{
			var selector = DateSelector(date) + " ." + className;
			return Document.Div(Find.BySelector(selector));
		}

		public string FirstCalendarCellDate
		{
			get
			{
				var cell = FirstCalendarCell;
				return cell == null ? null : cell.GetAttributeValue("data-mytime-date");
			}
		}

		public string LastCalendarCellDate
		{
			get
			{
				var cell = LastCalendarCell;
				return cell == null ? null : cell.GetAttributeValue("data-mytime-date");
			}
		}

		public void SelectCalendarCellForDateByClick(DateTime date) { SelectCalendarCellByClick(CalendarCellForDate(date)); }

		public void SelectCalendarCellByClick(ListItem cell) { cell.Click(); }

		public void SelectCalendarCellByClass(DateTime date)
		{
			EventualAssert.That(() => CalendarCellForDate(date).ClassName, Is.StringContaining("ui-selectee"));
			JQuery.SelectByElementAttribute("li", "data-mytime-date", date.ToString("yyyy-MM-dd"))
				.AddClass("ui-selected")
				.Eval();
			EventualAssert.That(() => CalendarCellForDate(date).ClassName, Is.StringContaining("ui-selected"));
		}
	}

}