using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Pages.jQuery;
using WatiN.Core;
using WatiN.Core.Constraints;

namespace Teleopti.Ccc.WebBehaviorTest.Pages.Common
{
	public class CalendarCellsPage : PortalPage
	{
		[FindBy(Id = "friendly-message")] public Div FriendlyMessage;

		private Constraint CalendarCellConstraint = Find.By("data-mytime-date", v => v != null);
		private ListItemCollection CalendarCellsListItems { get { return Document.ListItems.Filter(CalendarCellConstraint); } }
		public IEnumerable<ListItem> CalendarCells { get { return CalendarCellsListItems; } }
		public ListItem FirstCalendarCell { get { return Document.ListItem(CalendarCellConstraint).EventualGet(); } }
		public ListItem LastCalendarCell { get { return CalendarCellsListItems.Last(); } }

		public ListItem CalendarCellForDate(string formattedDate) { return Document.ListItem(Find.By("data-mytime-date", v => v == formattedDate)).EventualGet(); }
		public ListItem CalendarCellForDate(DateTime date) { return CalendarCellForDate(date.ToString("yyyy-MM-dd")); }

		public Div CalendarCellDataForDate(DateTime date, string className)
		{
			var selector = "[data-mytime-date='" + date.ToString("yyyy-MM-dd") + "'] ." + className;
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
			CalendarCellForDate(date).WaitUntilExists();
			JQuery.SelectByElementAttribute("li", "data-mytime-date", date.ToString("yyyy-MM-dd"))
				.AddClass("ui-selected")
				.Eval();
			EventualAssert.WhenElementExists(CalendarCellForDate(date), c => c.ClassName, Is.StringContaining("ui-selected"));
		}
	}

	public class CalendarCell : Control<ListItem>
	{
		public override WatiN.Core.Constraints.Constraint ElementConstraint
		{
			get
			{
				return Find.By("data-mytime-date", s => s != null);
			}
		}
	}

	public class SelectableCalendarCell :  Control<ListItem>
	{
		public override WatiN.Core.Constraints.Constraint ElementConstraint
		{
			get
			{
				return Find.By("data-mytime-date", s => s != null)
					.And(Find.ByClass(s => s.Contains("ui-selectee")));
			}
		}

		public string Date { get { return GetAttributeValue("data-mytime-date"); } }

		public void Select()
		{
			JQuery.SelectByElementAttribute("li", "data-mytime-date", Date)
				.AddClass("ui-selected")
				.EvalIn(Element.DomContainer);
		}
	}
}