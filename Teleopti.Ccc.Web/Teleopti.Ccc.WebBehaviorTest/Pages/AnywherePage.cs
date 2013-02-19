using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Pages
{
	public class AnywherePage : Page
	{
		[FindBy(Id = "content-placeholder")] public Element Placeholder;

		public Table ScheduleTable
		{
			get { return Document.Table(QuicklyFind.ByClass("table")); }
		}

		public Element RowByPerson(string personName)
		{
			//$('.myclass .myotherclass').
			// Find.BySelector(".myclass .myotherclass")
			//$('.agent ')
			return Document.Element(Find.BySelector(".agent:contains('" + personName + "')"));
		}
	}
}