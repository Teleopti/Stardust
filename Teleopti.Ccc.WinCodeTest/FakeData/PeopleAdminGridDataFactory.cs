using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.FakeData
{
	public static class PeopleAdminGridDataFactory
	{
		public static PersonGeneralModel GetPeopleAdminGridData(string contractName, string scheduleName,
			string partTimePercentage)
		{
			// Instantiates the person and teh team
			IPerson person = PersonFactory.CreatePerson();
			ITeam team1 = TeamFactory.CreateSimpleTeam();
			// Creates the person period
			IPersonPeriod personPeriod1 = PersonPeriodFactory.CreatePersonPeriod
				(DateOnly.Today,
					PersonContractFactory.CreatePersonContract(contractName, scheduleName, partTimePercentage),
					team1);
			person.AddPersonPeriod(personPeriod1);
			var principalAuthorization = new PrincipalAuthorization(new CurrentTeleoptiPrincipal(new ThreadPrincipalContext()));
			return new PersonGeneralModel(person, principalAuthorization, new PersonAccountUpdaterDummy(),
				new LogonInfoModel(), new PasswordPolicyFake());
		}
	}
}
