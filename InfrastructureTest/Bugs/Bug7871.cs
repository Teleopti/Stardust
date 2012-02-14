using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Bugs
{
    [TestFixture]
    public class Bug7871 : DatabaseTest
    {
        [Test]
        public void CanDeleteAndAddTheSameTeamToAvailableData()
        {
            //setup
            ISite site = new Site("Site");
            PersistAndRemoveFromUnitOfWork(site);
            ITeam team1 = TeamFactory.CreateSimpleTeam("Team1");
            site.AddTeam(team1);
            PersistAndRemoveFromUnitOfWork(team1);
            IApplicationRole applicationRole = ApplicationRoleFactory.CreateRole("Role", "Role");
            PersistAndRemoveFromUnitOfWork(applicationRole);
            IAvailableData availableData = new AvailableData();
            availableData.ApplicationRole = applicationRole;
            availableData.AvailableDataRange = AvailableDataRangeOption.MySite;
            PersistAndRemoveFromUnitOfWork(availableData);

            //do it
            AvailableDataRepository rep = new AvailableDataRepository(UnitOfWork);
            IAvailableData a = rep.Get(availableData.Id.Value);
            a.AddAvailableTeam(team1);
            a.DeleteAvailableTeam(team1);
            a.AddAvailableTeam(team1);
            Session.Flush();
        }

    }
}
