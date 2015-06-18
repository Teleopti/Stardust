﻿using System.Configuration;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Administration.Controllers;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.AdministrationTest.ControllerActions
{
	[TenantTest]
	public class GetVersionsTest
	{
		public UpgradeDatabasesController Target;

		[Test]
		public void ShouldReportOkIfSameVersion()
		{
			DataSourceHelper.CreateDataSource(new NoMessageSenders(), "TestData");
			
			var result = Target.GetVersions(new VersionCheckModel{AppConnectionString = ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString });

			result.Content.AppVersionOk.Should().Be.True();
		}
	}
}