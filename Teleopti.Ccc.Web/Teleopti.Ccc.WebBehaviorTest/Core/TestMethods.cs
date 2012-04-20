using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Teleopti.Ccc.WebBehaviorTest.Core
{
	public static class TestMethods
	{

		public static void CreateCorruptCookie()
		{
			Navigation.GoTo("Test/CorruptMyCookie");
		}

		public static void CreateNonExistingDatabaseCookie()
		{
			Navigation.GoTo("Test/NonExistingDatasourceCookie");
		}

		public static void ExpireMyCookie()
		{
			// doing this twice because IE fails to grab the cookie after the first one sometimes..
			// probably depending on how quickly the next request takes places.
			// making a second request seems to enforce the cookie somehow..
			Browser.Current.Eval("Teleopti.MyTimeWeb.Test.ExpireMyCookie();");
			EventualAssert.That(() => Browser.Current.Text, Is.StringContaining("Cookie is expired!"));

			Browser.Current.Eval("Teleopti.MyTimeWeb.Test.ExpireMyCookie();");
			EventualAssert.That(() =>
			                    	{
			                    		var text = Browser.Current.Text;
			                    		var regex = new Regex("Cookie is expired!");
			                    		return regex.Matches(text).Count;
			                    	}, Is.EqualTo(2));
		}

		public static void PageLog(string message)
		{
			Browser.Current.Eval("Teleopti.MyTimeWeb.Test.PageLog('" + message + "');");
		}
	}
}