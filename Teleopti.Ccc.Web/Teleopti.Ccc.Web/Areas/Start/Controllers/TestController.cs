using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Start.Controllers
{
	public class TestController : Controller
	{
		private readonly ISessionSpecificDataProvider _sessionSpecificDataProvider;
		private readonly IAuthenticator _authenticator;
		private readonly IWebLogOn _logon;
		private readonly IBusinessUnitProvider _businessUnitProvider;

		public TestController(ISessionSpecificDataProvider sessionSpecificDataProvider, IAuthenticator authenticator, IWebLogOn logon, IBusinessUnitProvider businessUnitProvider)
		{
			_sessionSpecificDataProvider = sessionSpecificDataProvider;
			_authenticator = authenticator;
			_logon = logon;
			_businessUnitProvider = businessUnitProvider;
		}

		public ActionResult AfterScenario()
		{
			_sessionSpecificDataProvider.RemoveCookie();
			return new FilePathResult(@"~\Areas\Start\Views\Test\AfterScenario.html", "text/html");
		}

		// "SignIn.DataSourceName={0}&SignIn.UserName={1}&SignIn.Password={2}&X-Requested-With=XMLHttpRequest";
		// dataSourceName=TestData&businessUnitName=BusinessUnit&userName=1&password=1
		public ActionResult Login(string dataSourceName, string businessUnitName, string userName, string password)
		{
			var result = _authenticator.AuthenticateApplicationUser(dataSourceName, userName, password);
			var businessUnits = _businessUnitProvider.RetrieveBusinessUnitsForPerson(result.DataSource, result.Person);
			var businessUnit = (from b in businessUnits where b.Name == businessUnitName select b).Single();
			_logon.LogOn(businessUnit.Id.Value, dataSourceName, result.Person.Id.Value, AuthenticationTypeOption.Application);
			return Content("Signed in as " + result.Person.Name.ToString(), "text/html");
		}

		public EmptyResult ExpireMyCookie()
		{
			_sessionSpecificDataProvider.ExpireTicket();
			return new EmptyResult();
		}

		public EmptyResult CorruptMyCookie()
		{
			var wrong = Convert.ToBase64String(Convert.FromBase64String("Totally wrong"));
			_sessionSpecificDataProvider.MakeCookie("UserName", DateTime.Now, wrong);
			return new EmptyResult();
		}

		public EmptyResult NonExistingDatasourceCookie()
		{
			var data = new SessionSpecificData(Guid.NewGuid(), "datasource", Guid.NewGuid(), AuthenticationTypeOption.Windows);
			_sessionSpecificDataProvider.StoreInCookie(data);
			return new EmptyResult();
		}
	}
}
