using System;
using Teleopti.Ccc.WebBehaviorTest.Pages.jQuery;
using WatiN.Core;
using WatiN.Core.Native;
using WatiN.Core.Native.InternetExplorer;

namespace Teleopti.Ccc.WebBehaviorTest.Core.Legacy
{
	internal static class ElementExtensions
	{
		public static bool PositionedOnScreen(this Element element)
		{
			int pos;
			var  topPosition = element.Style.GetAttributeValue("top");
			var tryParse = int.TryParse(topPosition.Replace("px", string.Empty), out pos);
			return !(tryParse && pos < 0);
		}

		public static bool IsDisplayed(this Element element)
		{
			if (!element.Exists || string.Equals(element.Style.Display, "none"))
			{
				return false;
			}
			if (element.Parent != null)
			{
				return IsDisplayed(element.Parent);
			}
			return true;
		}

		public static bool DisplayVisible(this Element element) { return element.Style.Display != "none"; }
		public static bool DisplayHidden(this Element element) { return element.Style.Display == "none"; }

		public static void ChangeValue(this TextField element, string value)
		{
			element.EventualWait();
			element.Value = value;
			element.FireEventNoWait("change");
			element.JQueryChange();
		}

		public static bool JQueryVisible(this Element element)
		{
			var result = JQuery.SelectById(GetIdForJQuerySelection(element))
				.Is(":visible")
				.Eval();
			return bool.Parse(result);
		}

		private static string GetIdForJQuerySelection(Element element)
		{
			var id = element.Id;
			if (id == null)
				throw new ArgumentException("Element has to have an Id to be able to use jQuery's is(':visible') method");
			return id;
		}

		public static bool JQueryHidden(this Element element) { return !element.JQueryVisible(); }

		public static bool DisplayVisible<T>(this Control<T> control) where T : Element { return control.Element.DisplayVisible(); }
		public static bool DisplayHidden<T>(this Control<T> control) where T : Element { return control.Element.DisplayHidden(); }

		public static bool JQueryVisible<T>(this Control<T> control) where T : Element { return control.Element.JQueryVisible(); }
		public static bool JQueryHidden<T>(this Control<T> control) where T : Element { return control.Element.JQueryHidden(); }

		public static void JQueryChange(this Element element)
		{
			JQuery.SelectById(GetIdForJQuerySelection(element))
				.Change()
				.Eval();
		}
	}
}
