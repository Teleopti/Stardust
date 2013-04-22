namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserInteractions
{
	public interface IBrowserInteractions
	{
		object Eval(string javascript);
		void GoTo(string uri);
		void Click(string selector);
		void AssertExists(string selector);
		void AssertNotExists(string existsSelector, string notExistsSelector);
		void AssertContains(string selector, string text);
		void AssertUrlContains(string url);
	}
}