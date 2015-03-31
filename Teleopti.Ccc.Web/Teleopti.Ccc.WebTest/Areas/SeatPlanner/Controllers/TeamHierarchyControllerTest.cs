using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Controllers;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.SeatPlanner.Controllers
{
	internal class TeamHierarchyControllerTest
	{

		private TeamHierarchyController target;
		private ISiteRepository siteRepository;
		private IBusinessUnitRepository businessUnitRepository;
		private INumberOfAgentsInTeamReader numberOfAgentsQuery;
		private ICurrentBusinessUnit currentBusinessUnit;

		[SetUp]
		public void Setup()
		{
			siteRepository = MockRepository.GenerateMock<ISiteRepository>();
			numberOfAgentsQuery = MockRepository.GenerateMock<INumberOfAgentsInTeamReader>();

			var bu1 = BusinessUnitFactory.BusinessUnitUsedInTest;
			businessUnitRepository = MockRepository.GenerateMock<IBusinessUnitRepository>();
			currentBusinessUnit = MockRepository.GenerateMock<ICurrentBusinessUnit>();
			currentBusinessUnit.Stub(x => x.Current()).Return(bu1);
			businessUnitRepository.Stub(x => x.Get(bu1.Id.GetValueOrDefault())).Return(bu1);
		}

		[Test]
		public void ShouldGetTeamHierarchy()
		{
			var site = new Site("Test Site");
			site.SetId(Guid.NewGuid());
			var team = new Team { Description = new Description("Test Team") };
			team.SetId(Guid.NewGuid());
			site.AddTeam(team);

			siteRepository.Stub(x => x.LoadAll()).Return(new List<ISite>(){site});
			numberOfAgentsQuery.Stub(x => x.FetchNumberOfAgents(new[] { team })).Return(new Dictionary<Guid, int>() { { team.Id.Value, 2 } });

			ITeamsProvider teamsProvider = new TeamsProvider(siteRepository, numberOfAgentsQuery, businessUnitRepository, currentBusinessUnit);
			target = new TeamHierarchyController(teamsProvider);
			
			var result = target.Get() as dynamic;
			var businessUnitWithSitesViewModel = result as BusinessUnitWithSitesViewModel;
			businessUnitWithSitesViewModel.Should().Not.Be.Null();
			var siteViewModel = businessUnitWithSitesViewModel.Children.SingleOrDefault(x => x.Id == site.Id.ToString());
			siteViewModel.Should().Not.Be.Null();
			var teamViewModel = siteViewModel.Children.SingleOrDefault (x => x.Id == team.Id.ToString());
			teamViewModel.Should().Not.Be.Null();
		}

		[TearDown]
		public void Teardown()
		{
			target.Dispose();
		}
	}
}
