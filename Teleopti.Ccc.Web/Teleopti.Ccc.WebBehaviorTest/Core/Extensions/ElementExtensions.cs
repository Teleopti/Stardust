using System;
using Teleopti.Ccc.WebBehaviorTest.Pages.jQuery;
using WatiN.Core;
using WatiN.Core.Native;
using WatiN.Core.Native.InternetExplorer;

namespace Teleopti.Ccc.WebBehaviorTest.Core.Extensions
{
	public static class ElementExtensions
	{
		public static bool PositionedOnScreen(this Element element)
		{
			int pos;
			var  topPosition = element.Style.GetAttributeValue("top");
			var tryParse = int.TryParse(topPosition.Replace("px", string.Empty), out pos);
			return !(tryParse && pos < 0);
		}

		public static bool DisplayVisible(this Element element) { return element.Style.Display != "none"; }
		public static bool DisplayHidden(this Element element) { return element.Style.Display == "none"; }

		public static bool JQueryVisible(this Element element)
		{
			var id = element.Id;
			if (id == null)
				throw new ArgumentException("Element has to have an Id to be able to use jQuery's is(':visible') method");
			var result = JQuery.SelectById(id).Is(":visible").Eval();
			return bool.Parse(result);
		}
		public static bool JQueryHidden(this Element element) { return !element.JQueryVisible(); }

		public static bool DisplayVisible<T>(this Control<T> control) where T : Element { return control.Element.DisplayVisible(); }
		public static bool DisplayHidden<T>(this Control<T> control) where T : Element { return control.Element.DisplayHidden(); }

		public static bool JQueryVisible<T>(this Control<T> control) where T : Element { return control.Element.JQueryVisible(); }
		public static bool JQueryHidden<T>(this Control<T> control) where T : Element { return control.Element.JQueryHidden(); }

		public static void ScrollIntoView(this Element element)
		{
			if (element.NativeElement is IEElement)
			{
				// doing this through reflection so I dont have to reference the browser api assembly:
				// (element.NativeElement as IEElement).AsHtmlElement.scrollIntoView();

				var nativeElement = element.NativeElement as IEElement;
				var nativeElementType = nativeElement.GetType();
				var htmlElementProperty = nativeElementType.GetProperty("AsHtmlElement");

				var htmlElement = htmlElementProperty.GetValue(nativeElement, null);
				var htmlElementType = htmlElementProperty.PropertyType;
				var method = htmlElementType.GetMethod("scrollIntoView");

				method.Invoke(htmlElement, new object[] {null});
			}
			else if (element.NativeElement is JSElement)
			{
				var nativeElement = element.NativeElement as JSElement;
				nativeElement.ExecuteMethod("scrollIntoView");
			}
			else
			{
				throw new Exception("Unknown browser");
			}
		}
	}
}
