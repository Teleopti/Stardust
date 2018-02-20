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
			var dataSource = new DataSource(UnitOfWorkFactoryFactoryForTest.CreateUnitOfWorkFactory("for test"), null, null);
			var loggedOnPerson = StateHolderProxyHelper.CreateLoggedOnPerson();
			loggedOnPerson.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			var principal = new TeleoptiPrincipalFactory().MakePrincipal(loggedOnPerson, dataSource,
				BusinessUnitFactory.BusinessUnitUsedInTest, null);
			Thread.CurrentPrincipal = principal;

			StateHolderProxyHelper.ClearStateHolder();
			StateHolder.Initialize(new FakeState(), new MessageBrokerCompositeDummy());
		}

		public void AfterTest(ITest test)
		{
			StateHolderProxyHelper.ClearStateHolder();
			Thread.CurrentPrincipal = null;
		}
	}
}