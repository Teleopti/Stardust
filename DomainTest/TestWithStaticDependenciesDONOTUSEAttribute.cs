using System;
using System.Threading;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest
{
	public class TestWithStaticDependenciesDONOTUSEAttribute : Attribute, ITestAction {

		public ActionTargets Targets => ActionTargets.Test;

		public void BeforeTest(ITest test)
		{
			BeforeTest();
		}

		public void AfterTest(ITest test)
		{
			AfterTest();
		}

		public static void BeforeTest()
		{
			var dataSource = new DataSource(UnitOfWorkFactoryFactory.CreateUnitOfWorkFactory("for test"), null, null);
			var loggedOnPerson = StateHolderProxyHelper.CreateLoggedOnPerson();
			loggedOnPerson.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			var principal = new TeleoptiPrincipalFactory().MakePrincipal(loggedOnPerson, dataSource,
				BusinessUnitFactory.BusinessUnitUsedInTest, null);
			Thread.CurrentPrincipal = principal;

			StateHolderProxyHelper.ClearStateHolder();
			StateHolder.Initialize(new FakeState(), new MessageBrokerCompositeDummy());
		}

		public static void AfterTest()
		{
			StateHolderProxyHelper.ClearStateHolder();
			Thread.CurrentPrincipal = null;
		}
	}
}