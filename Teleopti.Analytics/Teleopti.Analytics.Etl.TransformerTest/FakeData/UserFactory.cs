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
			
			person.WindowsAuthenticationInfo = new WindowsAuthenticationInfo
												   {WindowsLogOnName = "winLogon", DomainName = "myDomain"};
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

			person.WindowsAuthenticationInfo = new WindowsAuthenticationInfo { WindowsLogOnName = "pepi", DomainName = "Domain1" };
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
			person.WindowsAuthenticationInfo = new WindowsAuthenticationInfo { WindowsLogOnName = "kk", DomainName = "Domain1" };
			RaptorTransformerHelper.SetUpdatedOn(person, DateTime.Now);

			return person;
		}
	}
}