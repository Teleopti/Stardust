using System;
using System.ComponentModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
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
		/*
		 * just dummy tests here - impl used only from behaviour tests  
		 * (and they are not part of ncover)
		 */

		[Test]
		public void PlainStupid()
		{
			var sessionSpecificDataProvider = MockRepository.GenerateMock<ISessionSpecificDataProvider>();
			using (var target = new TestController(sessionSpecificDataProvider, null, null, null))
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
			using (var target = new TestController(null, authenticator, logon, businessUnitProvider))
			{
				target.Logon(null, businessUnit.Name, null, null);
			}
		}
	}
}