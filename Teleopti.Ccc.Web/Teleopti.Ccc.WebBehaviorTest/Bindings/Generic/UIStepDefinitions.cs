using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using WatiN.Core;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class UIStepDefinitions
	{
		// I click 'add full day absence' -> klass
		[When(@"I click '(.*)'")]
		public void WhenIClass(string classText)
		{
			var className = classText.ToLower().Replace(" ", "-");
			Browser.Current.Element(Find.BySelector(string.Format(".{0}", className))).EventualClick();
		}

		// I click agent 'mathias stenbom' -> klass, text
		[When(@"I click (.*) '(.*)'")]
		public void WhenIClick(string className, string text)
		{
			Browser.Current.Element(Find.BySelector(string.Format(".{0}:contains('{1}')", className, text))).EventualClick();
		}

		// I should see message 'an error message -> klass, resource
		// ... later?
	}
}