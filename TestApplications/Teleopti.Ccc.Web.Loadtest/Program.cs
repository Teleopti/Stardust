using System;
using System.Configuration;
using System.Linq;
using Teleopti.Ccc.Web.TestApplicationsCommon;

namespace Teleopti.Ccc.Web.Loadtest
{
	class Program
	{
		static void Main(string[] args)
		{
			RunReportsTest();
		}

		private static void RunReportsTest()
		{
			var baseUrl = ConfigurationManager.AppSettings["BaseUrl"];
			var businessUnitName = ConfigurationManager.AppSettings["BusinessUnitName"];
			var users = UserData.GenerateTestUsers(100);
			var reportsTest = new LoadReportsTest(baseUrl, businessUnitName);
			var result = reportsTest.RunAsync(users, 100).Result;
			var loginTotalElapsedMilliseconds = result.Sum(x => x.Logon);
			var totalElapsedMilliseconds = result.Sum(x => x.Reports);
			Console.WriteLine("Total users: {0}", users.Count);
			Console.WriteLine("Total spend logging in: {0} ms", loginTotalElapsedMilliseconds);
			Console.WriteLine("Average/login: {0} ms", loginTotalElapsedMilliseconds / users.Count);
			Console.WriteLine("Total spend loading reports: {0} ms", totalElapsedMilliseconds);
			Console.WriteLine("Average/report: {0} ms", totalElapsedMilliseconds / users.Count);
			Console.ReadKey();
		}
	}
}
