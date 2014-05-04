using System;
using System.Globalization;
using Teleopti.Analytics.Etl.Transformer;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.TransformerTest.FakeData
{
	public static class UserFactory
	{
		public static IPerson CreatePersonUser()
		{
			IPerson person = Ccc.TestCommon.FakeData.PersonFactory.CreatePerson("kalle", "kula");
			person.SetId(Guid.NewGuid());
			person.ApplicationAuthenticationInfo = new ApplicationAuthenticationInfo
													   {ApplicationLogOnName = "karl", Password = "secret"};
			
			person.AuthenticationInfo = new AuthenticationInfo { Identity = @"myDomain\winLogon" };
			person.Email = "kalle.kula@myDomain.com";
			person.PermissionInformation.SetCulture(CultureInfo.GetCultureInfo(1033));
			person.PermissionInformation.SetUICulture(CultureInfo.GetCultureInfo(1053));
			RaptorTransformerHelper.SetUpdatedOn(person, DateTime.Now);

			return person;
		}

		public static IPerson CreatePersonUserWithNoCultureSet()
		{
			IPerson person = Ccc.TestCommon.FakeData.PersonFactory.CreatePerson("pelle", "pilla");
			person.SetId(Guid.NewGuid());
			person.ApplicationAuthenticationInfo = new ApplicationAuthenticationInfo { ApplicationLogOnName = "perra", Password = "ts" };

			person.AuthenticationInfo = new AuthenticationInfo { Identity = @"Domain1\pepi" };
			person.Email = "pella.pilla@Domain1.com";
			RaptorTransformerHelper.SetUpdatedOn(person, DateTime.Now);

			return person;
		}

		public static IPerson CreatePersonUserWithNoWindowsAuthentication()
		{
			IPerson person = Ccc.TestCommon.FakeData.PersonFactory.CreatePerson("kalle", "kula");
			person.SetId(Guid.NewGuid());
			person.ApplicationAuthenticationInfo = new ApplicationAuthenticationInfo { ApplicationLogOnName = "karl", Password = "secret" };
			RaptorTransformerHelper.SetUpdatedOn(person, DateTime.Now);

			return person;
		}

		public static IPerson CreatePersonUserWithNoApplicationAuthentication()
		{
			IPerson person = Ccc.TestCommon.FakeData.PersonFactory.CreatePerson("kalle", "kula");
			person.SetId(Guid.NewGuid());
			person.AuthenticationInfo = new AuthenticationInfo { Identity = @"Domain1\kk" };
			RaptorTransformerHelper.SetUpdatedOn(person, DateTime.Now);

			return person;
		}
	}
}