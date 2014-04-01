using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Web.Areas.Start.Controllers;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;

namespace Teleopti.Ccc.WebTest.Areas.Start.Controllers
{
	[TestFixture]
	public class TestControllerTest
	{
		[Test]
		public void CanSwitchINowImplementation()
		{
			var dateSet = new DateTime(2000, 1, 2, 3, 4, 5);

			var formsAuthentication = MockRepository.GenerateMock<IFormsAuthentication>();
			var modifyNow = MockRepository.GenerateStrictMock<IMutateNow>();
			modifyNow.Expect(mock => mock.Mutate(dateSet));

			using (var target = new TestController(modifyNow, null, null, null, null, formsAuthentication))
			{
				target.SetCurrentTime(dateSet.Ticks);
			}
		}

		[Test]
		public void PlainStupid()
		{
			var sessionSpecificDataProvider = MockRepository.GenerateMock<ISessionSpecificDataProvider>();
			var formsAuthentication = MockRepository.GenerateMock<IFormsAuthentication>();
			using (var target = new TestController(new MutableNow(), sessionSpecificDataProvider, null, null, null, formsAuthentication))
			{
				target.BeforeScenario(true);
				target.CorruptMyCookie();
				target.ExpireMyCookie();
				target.NonExistingDatasourceCookie();
			}
			formsAuthentication.AssertWasCalled(x => x.SignOut());
		}

		[Test]
		public void EvenMoreStupider()
		{
			var formsAuthentication = MockRepository.GenerateMock<IFormsAuthentication>();
			var authenticator = MockRepository.GenerateMock<IAuthenticator>();
			var person = new Person();
			person.SetId(Guid.NewGuid());
			authenticator.Stub(x => x.AuthenticateApplicationUser(null, null, null)).IgnoreArguments().Return(new AuthenticateResult { Person = person });
			var logon = MockRepository.GenerateMock<IWebLogOn>();
			var businessUnit = new BusinessUnit("businessUnit");
			businessUnit.SetId(Guid.NewGuid());
			var businessUnitProvider = MockRepository.GenerateMock<IBusinessUnitProvider>();
			businessUnitProvider.Stub(x => x.RetrieveBusinessUnitsForPerson(null, null)).IgnoreArguments().Return(new[] { businessUnit });
			using (var target = new TestController(null, null, authenticator, logon, businessUnitProvider, formsAuthentication))
			{
				target.Logon(null, businessUnit.Name, null, null);
			}
		}
	}
}