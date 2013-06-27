using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using Teleopti.Interfaces.Domain;
using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Pages.Common
{
	public class DatePicker : Control<TextField>
	{
		public DateTime? Date
		{
			get
			{
				if (Element.Text == null) return null;

				var date = Eval("{0}('getDate').toUTCString()", ElementReference);

				return DateTime.Parse(date);
			}
			set
			{
				var javascriptDate = "null";
				if (value.HasValue)
				{
					var date = value.Value;
					javascriptDate = string.Format("new Date({0}, {1}, {2})", date.Year, date.Month - 1, date.Day);
				}

				CallDatepicker("'setDate', {0}", javascriptDate);
			}
		}

		public string DateFormat
		{
			get { return GetOption("dateFormat"); }
		}

		public bool Enabled
		{
			get
			{
				var isDisabled = CallDatepicker("'isDisabled'");
				return !bool.Parse(isDisabled);
			}
		}

		public string GetOption(string option)
		{
			return CallDatepicker("'option', '{0}'", option);
		}

		public void Show()
		{
			CallDatepicker("'show'");
		}

		public Div Calendar { get { return Element.DomContainer.Div(Find.ById("ui-datepicker-div")); } }
		public Table CalendarTable { get { return Calendar.Table(QuicklyFind.ByClass("ui-datepicker-calendar")); } }

		public void OpenProcedure()
		{
			EventualAssert.That(() => Exists, Is.True);
			EventualAssert.That(() => Calendar.Exists, Is.True);

			Show();
			EventualAssert.That(() => Calendar.Exists, Is.True);
			EventualAssert.That(IsOpen, Is.True);
			//EventualAssert.That(() => Calendar.Style.Display, Is.Not.EqualTo("none"));
		}

		public bool IsOpen() { return Calendar.Style.Display != "none"; }
		public bool IsClosed() { return Calendar.Style.Display == "none"; }

		public IEnumerable<TableRow> CalendarTableRows { get { return CalendarTable.TableRows.Skip(1); } }
		public IEnumerable<TableCell> CalendarTableFirstCells { get { return from r in CalendarTableRows select r.TableCells.First(); } }

		public IEnumerable<int> CalendarFirstDayNumbers
		{
			get
			{
				return from c in CalendarTableFirstCells
				       let hasLink = c.Links.Count > 0
				       where hasLink
				       select int.Parse(c.Links.First().Text);
			}
		}

		public void ClickDay(DateOnly date)
		{
			var dateLink = Element.DomContainer.Link(Find.ByText(date.Day.ToString()));
			dateLink.EventualClick();
		}

		private string ElementReference { get { return string.Format("window.jQuery({0}).datepicker", Element.GetJavascriptElementReference()); } }

		private string CallDatepicker(string script, params object[] args)
		{
			var theScript = string.Format(script, args);
			return Eval("{0}({1})", ElementReference, theScript);
		}

		private string Eval(string script, params object[] args)
		{
			return Element.DomContainer.Eval(string.Format(script, args));
		}
	}
}