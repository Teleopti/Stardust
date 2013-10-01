using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.PeopleAdmin;
using Teleopti.Ccc.WinCode.PeopleAdmin.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.FakeData
{
    /// <summary>
    /// Test class for the PeopleAdminApplicationRoleComparer class of the wincode.
    /// </summary>
    /// <remarks>
    /// Created By: Savani Nirasha
    /// Created Date: 22-09-2008
    /// </remarks>
    public static class PeopleAdminGridDataFactory
    {
        public static PersonGeneralModel GetPeopleAdminGridData(string contractName, string scheduleName, string partTimePercentage)
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
            var principalAuthorization = new PrincipalAuthorization(new CurrentTeleoptiPrincipal());
            return new PersonGeneralModel(person, new UserDetail(person), principalAuthorization, new PersonAccountUpdaterDummy());
        }
    }
}
