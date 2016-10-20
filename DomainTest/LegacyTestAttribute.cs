//using log4net.Config;
//using NUnit.Framework;
//using Teleopti.Ccc.DomainTest.Common;
//using Teleopti.Ccc.Infrastructure.UnitOfWork;
//using Teleopti.Ccc.TestCommon;
//using Teleopti.Ccc.TestCommon.FakeData;

//namespace Teleopti.Ccc.DomainTest
//{
//	[SetUpFixture]
//	[Parallelizable]
//	public class SetupFixtureForAssembly
//	{
//		[OneTimeSetUp]
//		public void RunBeforeAnyTest()
//		{
//			var stateMock = new FakeState();

//			var dataSource = new DataSource(UnitOfWorkFactoryFactory.CreateUnitOfWorkFactory("for test"), null, null);
//			var loggedOnPerson = StateHolderProxyHelper.CreateLoggedOnPerson();
//			StateHolderProxyHelper.CreateSessionData(loggedOnPerson, dataSource, BusinessUnitFactory.BusinessUnitUsedInTest);

//			StateHolderProxyHelper.ClearAndSetStateHolder(stateMock);

//			BasicConfigurator.Configure(new DoNothingAppender());
//		}
//	}
//}


using System;
using System.Threading;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest
{
	public class LegacyTestAttribute : Attribute, ITestAction {

		public ActionTargets Targets => ActionTargets.Test;

		public void BeforeTest(ITest test)
		{
			var dataSource = new DataSource(UnitOfWorkFactoryFactory.CreateUnitOfWorkFactory("for test"), null, null);
			var loggedOnPerson = StateHolderProxyHelper.CreateLoggedOnPerson();
			loggedOnPerson.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			var principal = new TeleoptiPrincipalFactory().MakePrincipal(loggedOnPerson, dataSource, BusinessUnitFactory.BusinessUnitUsedInTest, null);
			Thread.CurrentPrincipal = principal;

			CurrentAuthorization.GloballyUse(new FullPermission());

			StateHolderProxyHelper.ClearStateHolder();
			StateHolder.Initialize(new FakeState(), new MessageBrokerCompositeDummy());
		}

		public void AfterTest(ITest test)
		{
			StateHolderProxyHelper.ClearStateHolder();

			CurrentAuthorization.GloballyUse(null);

			Thread.CurrentPrincipal = null;
		}
	}
}