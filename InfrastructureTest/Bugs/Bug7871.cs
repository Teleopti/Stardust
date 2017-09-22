using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.Bugs
{
	[DatabaseTest]
	public class Bug7871
	{
		public ISiteRepository SiteRepository;
		public ITeamRepository TeamRepository;
		public IApplicationRoleRepository ApplicationRoleRepository;
		public IAvailableDataRepository AvailableDataRepository;

		[Test]
		public void CanDeleteAndAddTheSameTeamToAvailableData()
		{
			var team1 = TeamFactory.CreateSimpleTeam("Team1");
			var applicationRole = ApplicationRoleFactory.CreateRole("Role", "Role");
			var availableData = new AvailableData
			{
				ApplicationRole = applicationRole,
				AvailableDataRange = AvailableDataRangeOption.MySite
			};
			using (var setupUow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var site = new Site("Site");
				SiteRepository.Add(site);
				site.AddTeam(team1);
				TeamRepository.Add(team1);
				ApplicationRoleRepository.Add(applicationRole);
				AvailableDataRepository.Add(availableData);
				setupUow.PersistAll();
			}

			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var a = AvailableDataRepository.Get(availableData.Id.Value);
				a.AddAvailableTeam(team1);
				a.DeleteAvailableTeam(team1);
				a.AddAvailableTeam(team1);
				uow.PersistAll();
			}
		}
	}
}
