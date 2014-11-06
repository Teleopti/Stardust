using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Anywhere.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Reports.DataProvider;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Controllers
{
	[TestFixture]
	public class ReportsControllerTest
	{

		[Test]
		public void ShouldGetReports()
		{
			var sessionSpecificDataProvider = MockRepository.GenerateMock<ISessionSpecificDataProvider>();
			sessionSpecificDataProvider.Stub(x => x.GrabFromCookie())
				.Return(new SessionSpecificData(Guid.NewGuid(), Guid.NewGuid().ToString(), Guid.NewGuid()));
			var reportsProvider = MockRepository.GenerateMock<IReportsProvider>();
			reportsProvider.Stub(x => x.GetReports()).Return(new List<IApplicationFunction>());

			var target = new ReportsController(reportsProvider, sessionSpecificDataProvider);
			
			var result = target.GetReports();

			result.Should().Be(new List<ReportItem>());
		}
	}
}
