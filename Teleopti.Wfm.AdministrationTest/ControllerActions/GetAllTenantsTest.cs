﻿using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Administration.Controllers;

namespace Teleopti.Wfm.AdministrationTest.ControllerActions
{
	[TenantTest]
	public class GetAllTenantsTest
	{
		public HomeController Target;
		public ITenantUnitOfWork TenantUnitOfWork;

		[Test]
		public void ShouldNotBeNull()
		{
			//create database
			DataSourceHelper.CreateDataSource(new NoMessageSenders(), "TestData");

			Target.GetAllTenants().Should().Not.Be.Null();
		}
	}
}