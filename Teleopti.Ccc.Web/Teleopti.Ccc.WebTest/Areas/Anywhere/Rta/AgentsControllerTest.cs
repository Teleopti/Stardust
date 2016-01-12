using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModelBuilders;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Anywhere.Controllers;
using Teleopti.Ccc.WebTest.TestHelper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Rta
{
	[TestFixture]
	public class AgentsControllerTest
	{
		[Test]
		public void GetAgents_ShouldGetAllAgentsForOneTeam()
		{
			var teamId = Guid.NewGuid();
			var team = new Team { Description = new Description("team1") };
			team.SetId(teamId);
			var siteId = Guid.NewGuid();
			var site = new Site("site1");
			site.SetId(siteId);
			site.AddTeam(team);
			var person = new Person();
			var personId = Guid.NewGuid();
			person.SetId(personId);
			person.Name = new Name("bill", "gates");
			var teamRepository = MockRepository.GenerateMock<ITeamRepository>();
			teamRepository.Stub(x => x.Get(teamId)).Return(team);
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			var today = new Now();
			var period = new DateOnlyPeriod(today.LocalDateOnly(), today.LocalDateOnly());
			personRepository.Stub(x => x.FindPeopleBelongTeam(team, period)).Return(new List<IPerson> { person });
			personRepository.Stub(x => x.Get(personId)).Return(person);
			var commonAgentNameProvider = MockRepository.GenerateMock<ICommonAgentNameProvider>();
			var commonAgentNameSettings = new CommonNameDescriptionSetting();
			commonAgentNameProvider.Stub(x => x.CommonAgentNameSettings).Return(commonAgentNameSettings);

			var target = new AgentsController(new FakePermissionProvider(), teamRepository, personRepository, new Now(), commonAgentNameProvider, null, null, null);
			var expected = new AgentViewModel
			{
				PersonId = personId,
				Name = commonAgentNameSettings.BuildCommonNameDescription(person),
				SiteId = siteId.ToString(),
				SiteName = site.Description.Name,
				TeamId = teamId.ToString(),
				TeamName = team.Description.Name,
			};
			var result = target.ForTeam(teamId).Result<AgentViewModel[]>();

			result.Length.Should().Be(1);

			Assert.That(result.Single().PersonId, Is.EqualTo(expected.PersonId));
			Assert.That(result.Single().Name, Is.EqualTo(expected.Name));
			Assert.That(result.Single().SiteId, Is.EqualTo(expected.SiteId));
			Assert.That(result.Single().SiteName, Is.EqualTo(expected.SiteName));
			Assert.That(result.Single().TeamId, Is.EqualTo(expected.TeamId));
			Assert.That(result.Single().TeamName, Is.EqualTo(expected.TeamName));
		}

		[Test]
		public void GetTeamId_ShouldGetTeamIdForAPerson()
		{
			var date = DateTime.Today;
			var team = new Team { Description = new Description("team1") }.WithId();
			var site = new Site("site1").WithId();
			site.AddTeam(team);
			var person = new Person().WithId();
			person.Name = new Name("bill", "gates");
			person.AddPersonPeriod(new PersonPeriod(new DateOnly(date), PersonContractFactory.CreatePersonContract("c", "cs", "ptp"), team));
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			personRepository.Stub(x => x.Get(person.Id.GetValueOrDefault())).Return(person);

			var target = new AgentsController(new FakePermissionProvider(), null, personRepository, new Now(), null, null, null, null);
			var result = target.Team(person.Id.GetValueOrDefault(), date).Result<Guid>();
			result.Should().Be(team.Id.GetValueOrDefault());
		}

		[Test]
		public void GetPersonName_ShouldGetPersonNameForAPerson()
		{
			var person = new Person().WithId();
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			personRepository.Stub(x => x.Get(person.Id.GetValueOrDefault())).Return(person);
			var commonAgentNameProvider = MockRepository.GenerateMock<ICommonAgentNameProvider>();
			var commonAgentNameSettings = new CommonNameDescriptionSetting();
			commonAgentNameProvider.Stub(x => x.CommonAgentNameSettings).Return(commonAgentNameSettings);

			person.Name = new Name("bill", "gates");

			var target = new AgentsController(new FakePermissionProvider(), null, personRepository, new Now(), commonAgentNameProvider, null, null, null);
			
			var result = target.PersonDetails(person.Id.GetValueOrDefault()).Result<PersonDetailModel>();
			result.Name.Should().Be(commonAgentNameSettings.BuildCommonNameDescription(person));
		}
	}
}
