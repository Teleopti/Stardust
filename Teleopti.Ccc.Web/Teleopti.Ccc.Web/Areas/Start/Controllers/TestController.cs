using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teleopti.Ccc.Web.Core.RequestContext;

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

	}
}
