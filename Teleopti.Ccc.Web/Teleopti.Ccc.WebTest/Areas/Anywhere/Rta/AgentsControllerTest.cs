using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModelBuilders;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Anywhere.Controllers;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.WebTest.TestHelper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Rta
{
	[TestFixture]
	public class AgentsControllerTest
	{
		[Test]
		public void GetStates_ShouldGetAllTheStatesForTeamFromReader()
		{
			var teamId = Guid.NewGuid();
			var personId = Guid.NewGuid();

			var data = new AgentStateReadModel
			{
				PersonId = personId,
				TeamId = teamId,
				State = "out of adherence",
				StateStartTime = "2001-01-01 11:00".Utc(),
				Scheduled = "Phone",
				ScheduledNext = "Lunch",
				NextStart = "2001-01-01 12:30".Utc(),
				AlarmName = "Alarma!",
				RuleStartTime = "2001-01-01 12:00".Utc(),
				Color = Color.Red.ToArgb()
			};
			var expected = new AgentStateViewModel
			{
				PersonId = personId,
				State = "out of adherence",
				StateStartTime = "2001-01-01 11:00".Utc(),
				Activity = "Phone",
				NextActivity = "Lunch",
				NextActivityStartTime = "2001-01-01 12:30".Utc(),
				Alarm = "Alarma!",
				AlarmStart = "2001-01-01 12:00".Utc(),
				AlarmColor = "#FF0000"
			};

			var target = new AgentsController(null, null, null, new Now(), null, new FakeAgentStateReadModelReader(new[] { data }), null,null);
			var result = target.GetStates(teamId).Result<AgentStateViewModel[]>();

			result.Length.Should().Be(1);
			Assert.That(result.First().PersonId, Is.EqualTo(expected.PersonId));
			Assert.That(result.First().State, Is.EqualTo(expected.State));
			Assert.That(result.First().StateStartTime, Is.EqualTo(expected.StateStartTime));
			Assert.That(result.First().Activity, Is.EqualTo(expected.Activity));
			Assert.That(result.First().NextActivity, Is.EqualTo(expected.NextActivity));
			Assert.That(result.First().NextActivityStartTime, Is.EqualTo(expected.NextActivityStartTime));
			Assert.That(result.First().Alarm, Is.EqualTo(expected.Alarm));
			Assert.That(result.First().AlarmStart, Is.EqualTo(expected.AlarmStart));
			Assert.That(result.First().AlarmColor, Is.EqualTo(expected.AlarmColor));
		}

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

		[Test]
		public void ShouldReturnTimeInStateWhenNoStateStartTime()
		{
			var teamId = Guid.NewGuid();
			var data = new AgentStateReadModel
			{
				TeamId = teamId
			};
			var target = new AgentsController(null, null, null, null, null, new FakeAgentStateReadModelReader(new[] { data }), null, null);
			
			var result = target.GetStates(teamId).Result<AgentStateViewModel[]>();
			result.Single().TimeInState.Should().Be(0);
		}

		[Test]
		public void ShouldCalculateTimeInState()
		{
			var teamId = Guid.NewGuid();
			var data = new AgentStateReadModel
			{
				TeamId = teamId,
				StateStartTime = "2015-10-02 09:00".Utc(),
			};
			var now = new MutableNow("2015-10-02 09:05");

			var target = new AgentsController(null, null, null, now, null, new FakeAgentStateReadModelReader(new[] { data }), null, null);
			
			var result = target.GetStates(teamId).Result<AgentStateViewModel[]>();
			result.Single().TimeInState.Should().Be((int)"5".Minutes().TotalSeconds);
		}

		[Test]
		public void ShouldReturnTimeInAlarmWhenNoAlarmStartTime()
		{
			var teamId = Guid.NewGuid();
			var data = new AgentStateReadModel
			{
				TeamId = teamId
			};
			var target = new AgentsController(null, null, null, null, null, new FakeAgentStateReadModelReader(new[] { data }), null, null);
			
			var result = target.GetStates(teamId).Result<AgentStateViewModel[]>();
			result.Single().TimeInAlarm.Should().Be(null);
		}

		[Test]
		public void ShouldCalculateTimeInAlarm()
		{
			var teamId = Guid.NewGuid();
			var data = new AgentStateReadModel
			{
				TeamId = teamId,
				StateStartTime = "2015-10-02 09:00".Utc(),
			};
			var now = new MutableNow("2015-10-02 09:05");

			var target = new AgentsController(null, null, null, now, null, new FakeAgentStateReadModelReader(new[] { data }), null, null);
			
			var result = target.GetStates(teamId).Result<AgentStateViewModel[]>();
			result.Single().TimeInAlarm.Should().Be((int?)"5".Minutes().TotalSeconds);
		}

		[Test]
		public void ShouldBeNullWhenAlarmHasNotStartedYet()
		{
			var teamId = Guid.NewGuid();
			var data = new AgentStateReadModel
			{
				TeamId = teamId,
				StateStartTime = "2015-10-02 09:05".Utc(),
			};
			var now = new MutableNow("2015-10-02 09:00");

			var target = new AgentsController(null, null, null, now, null, new FakeAgentStateReadModelReader(new[] { data }), null, null);
			
			var result = target.GetStates(teamId).Result<AgentStateViewModel[]>();
			result.Single().TimeInAlarm.Should().Be(null);
		}
	}
}
