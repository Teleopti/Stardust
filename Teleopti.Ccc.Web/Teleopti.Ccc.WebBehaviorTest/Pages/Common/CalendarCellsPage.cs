using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.WebBehaviorTest.Core.Legacy;
using WatiN.Core;
using WatiN.Core.Constraints;

namespace Teleopti.Ccc.WebBehaviorTest.Pages.Common
{
	internal class CalendarCellsPage : PortalPage
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

		public static string DateSelector(string formattedDate) { return "li[data-mytime-date='" + formattedDate + "']"; }
		
		public static string DateSelector(DateTime date) { return DateSelector(date.ToString("yyyy-MM-dd")); }

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

		public void SelectCalendarCellByClick(ListItem cell) { cell.Click(); }
	}
}