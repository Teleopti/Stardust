﻿using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Anywhere.Controllers;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Controllers
{
	[TestFixture]
	public class ReportsControllerTest
	{

		[Test]
		public void ShouldGetReports()
		{
			var reportItems = new List<ReportItem>();
			var reportItemsProvider = MockRepository.GenerateMock<IReportItemsProvider>();
			reportItemsProvider.Stub(x => x.GetReportItems()).Return(reportItems);

			var target = new ReportsController(reportItemsProvider);
			var result = target.GetReports();

			result.Data.Should().Be.SameInstanceAs(reportItems);
		}
	}
}
