using Teleopti.Ccc.WebBehaviorTest.Pages.jQuery;
using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Pages.Common
{
	public class ComboBox : Control<SelectList>
	{
		public TextField TextField { get { return Element.NextSibling as TextField; } }
		public Button Button { get { return Element.NextSibling.NextSibling as Button; } }

		public string Value
		{
			set
			{
				Element.JQuery()
					.WidgetOptions("combobox", new {value})
					.Eval();
			}
		}

		public bool Enabled { get { return TextField.Enabled; } }

	}
}