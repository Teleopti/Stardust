using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
			
			var jsonResult =  HttpSender.Get("api/beta/users/me/teams", token);
			var teams = JsonConvert.DeserializeObject<TeamResult>(jsonResult);
			foreach (var team in teams.teams)
			{
				Console.WriteLine(team.id);
				Console.WriteLine(team.eTag);
				Console.WriteLine(team.name);
				Console.WriteLine(team.provisioningDomain);
			}
		}
	}
}
