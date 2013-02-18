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
	}
}