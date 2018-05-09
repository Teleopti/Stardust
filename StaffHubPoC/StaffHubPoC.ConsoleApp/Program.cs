using System;

namespace StaffHubPoC.ConsoleApp
{
	class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			var clientId = System.Configuration.ConfigurationManager.AppSettings["ClientId"];
			var redirectUrl = new Uri(System.Configuration.ConfigurationManager.AppSettings["redirectUrl"]);
			const string resourceId = "aa580612-c342-4ace-9055-8edee43ccb89";
			var authority = "https://login.microsoftonline.com/teleopti.onmicrosoft.com/oauth2/token";
			var authorizeUri = "https://login.microsoftonline.com/teleopti.onmicrosoft.com/oauth2/authorize";

			var token = OAuthFlow.Flow(
				clientId,
				resourceId,
				redirectUrl,
				authority,
				authorizeUri,
				true,
				"KIxOydAoOQDBtkEk9WWWo8WqIH6K5h00AuJ0pqBLfOE=");

			var myTeam = Communicator.GetMyTeam(token);
			var members = Communicator.GetMembers(myTeam, token);
			
			//var amanda = members.First(x => x.firstName == "Amanda");
			//var breaks = new List<Break> {new Break {breakType = BreakType.Paid, duration = 15}};
			//Communicator.PostShift(amanda,new DateTime(2018,05,11,16,0,0), new DateTime(2018,05,11,23,0,0), breaks, "AM", myTeam, token);


		}
	}
}
