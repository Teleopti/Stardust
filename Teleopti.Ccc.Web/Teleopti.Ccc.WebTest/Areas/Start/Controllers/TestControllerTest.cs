using System;
using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Web.Areas.Start.Controllers;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Start.Controllers
{
	[TestFixture]
	public class TestControllerTest
	{
		[Test]
		public void CanSwitchINowImplementation()
		{
			var dateSet = new DateTime(2000, 1, 2, 3, 4, 5);

			var containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterModule(new DateAndTimeModule());
			using(var container = containerBuilder.Build())
			{
				using (var target = new TestController(container, null, null, null, null))
				{
					target.SetCurrentTime(dateSet);
				}
				container.Resolve<INow>().UtcTime
					.Should().Be.EqualTo(dateSet);
			}
		}

		[Test]
		public void PlainStupid()
		{
			var sessionSpecificDataProvider = MockRepository.GenerateMock<ISessionSpecificDataProvider>();
			using (var target = new TestController(null, sessionSpecificDataProvider, null, null, null))
			{
				target.BeforeScenario();
				target.CorruptMyCookie();
				target.ExpireMyCookie();
				target.NonExistingDatasourceCookie();
			}
		}

		[Test]
		public void EvenMoreStupider()
		{
			var authenticator = MockRepository.GenerateMock<IAuthenticator>();
			var person = new Person();
			person.SetId(Guid.NewGuid());
			authenticator.Stub(x => x.AuthenticateApplicationUser(null, null, null)).IgnoreArguments().Return(new AuthenticateResult { Person = person });
			var logon = MockRepository.GenerateMock<IWebLogOn>();
			var businessUnit = new BusinessUnit("businessUnit");
			businessUnit.SetId(Guid.NewGuid());
			var businessUnitProvider = MockRepository.GenerateMock<IBusinessUnitProvider>();
			businessUnitProvider.Stub(x => x.RetrieveBusinessUnitsForPerson(null, null)).IgnoreArguments().Return(new[] { businessUnit });
			using (var target = new TestController(null, null, authenticator, logon, businessUnitProvider))
			{
				target.Logon(null, businessUnit.Name, null, null);
			}
		}
	}
}