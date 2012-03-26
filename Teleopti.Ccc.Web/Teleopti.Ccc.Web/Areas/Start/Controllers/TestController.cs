using System;
using System.Web.Mvc;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Start.Controllers
{
	public class TestController : Controller
	{
		private readonly ISessionSpecificDataProvider _sessionSpecificDataProvider;

		public TestController(ISessionSpecificDataProvider sessionSpecificDataProvider)
		{
			_sessionSpecificDataProvider = sessionSpecificDataProvider;
		}

		public EmptyResult ExpireMyCookie()
		{
			_sessionSpecificDataProvider.ExpireCookie();
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
			_sessionSpecificDataProvider.Store(data);
			return new EmptyResult();
		}
	}
}
