﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Anywhere.Controllers;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.Rta;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Controllers
{
	[TestFixture]
	public class AgentsControllerTest
	{
	[Test]
		public void ShouldFailIfNoPermissionForOneTeam()
		{
			var teamId = Guid.NewGuid();
			var team = new Team();
			team.SetId(teamId);
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			var teamRepository = MockRepository.GenerateMock<ITeamRepository>();
			var date = new Now();
			permissionProvider.Stub(
				x => x.HasTeamPermission(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, date.LocalDateOnly(),
						team)).Return(false);
			teamRepository.Stub(x => x.Get(teamId)).Return(team);

			using (var target = new StubbingControllerBuilder().CreateController<AgentsController>(permissionProvider, teamRepository, null, date,null,null,null))
			{
				target.GetStates(teamId);
				target.Response.StatusCode.Should().Be(403);
			}
		}

		[Test]
		public void GetStates_ShouldGetAllTheStatesForTeamFromReader()
		{
			var teamId = Guid.NewGuid();
			var team = new Team();
			team.SetId(teamId);
			var personId = Guid.NewGuid();
			var person = new Person();
			person.SetId(personId);
			person.Name = new Name("a","b");

			var userTimeZone = MockRepository.GenerateMock<IUserTimeZone>();
			var hawaiiTimeZoneInfo = TimeZoneInfoFactory.HawaiiTimeZoneInfo();
			userTimeZone.Stub(x => x.TimeZone()).Return(hawaiiTimeZoneInfo);
           
		    var commonAgentNameProvider = MockRepository.GenerateMock<ICommonAgentNameProvider>();
		    var commonAgentNameSettings = new CommonNameDescriptionSetting();
		    commonAgentNameProvider.Stub(x => x.CommonAgentNameSettings).Return(commonAgentNameSettings);
			
			var stateInfo = new AgentAdherenceStateInfo()
			{
				PersonId = personId,
				State = "out of adherence",
				StateStart = new DateTime(2001, 1, 1, 11, 0, 0, DateTimeKind.Utc),
				Activity = "Phone",
				NextActivity = "Lunch",
				NextActivityStartTime = new DateTime(2001, 1, 1, 12, 3, 0, DateTimeKind.Utc),
				Alarm = "Alarma!",
				AlarmStart = new DateTime(2001, 1, 1, 12, 0, 0, DateTimeKind.Utc),
				AlarmColor = ColorTranslator.ToHtml(Color.Red)
			};
			var expected = new AgentViewModel
			{
				PersonId = stateInfo.PersonId,
				Name = commonAgentNameSettings.BuildCommonNameDescription(person),
				State = stateInfo.State,
				StateStart = stateInfo.StateStart,
				Activity =stateInfo.Activity,
				NextActivity = stateInfo.NextActivity,
				NextActivityStartTime = stateInfo.NextActivityStartTime,
				Alarm = stateInfo.Alarm,
				AlarmStart = stateInfo.AlarmStart,
				AlarmColor = stateInfo.AlarmColor
			};

			var dataReader = new fakeStateReader(new List<AgentAdherenceStateInfo>()
			                                     {
				                                     stateInfo
			                                     }); 

			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			personRepository.Stub(x => x.Get(personId)).Return(person);
			using (var target = new StubbingControllerBuilder().CreateController<AgentsController>(new FakePermissionProvider(), MockRepository.GenerateStub<ITeamRepository>(), personRepository, new Now(), dataReader, userTimeZone, commonAgentNameProvider))
			{
			
				var result = target.GetStates(teamId).Data as IEnumerable<AgentViewModel>;

				result.Count().Should().Be(1);

				Assert.That(result.First().PersonId,Is.EqualTo(expected.PersonId));
				Assert.That(result.First().Name, Is.EqualTo(expected.Name));
				Assert.That(result.First().State, Is.EqualTo(expected.State));
				Assert.That(result.First().StateStart, Is.EqualTo(expected.StateStart));
				Assert.That(result.First().Activity, Is.EqualTo(expected.Activity));
				Assert.That(result.First().NextActivity, Is.EqualTo(expected.NextActivity));
				Assert.That(result.First().NextActivityStartTime, Is.EqualTo(expected.NextActivityStartTime));
				Assert.That(result.First().Alarm, Is.EqualTo(expected.Alarm));
				Assert.That(result.First().AlarmStart, Is.EqualTo(expected.AlarmStart));
				Assert.That(result.First().AlarmColor, Is.EqualTo(expected.AlarmColor));
			}
		}


		[Test]
		public void GetAgents_ShouldGetAllAgentsForOneTeam()
		{
			var teamId = Guid.NewGuid();
			var team = new Team {Description = new Description("team1")};
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
            var userTimeZone = MockRepository.GenerateMock<IUserTimeZone>();
			var hawaiiTimeZoneInfo = TimeZoneInfoFactory.HawaiiTimeZoneInfo();
			userTimeZone.Stub(x => x.TimeZone()).Return(hawaiiTimeZoneInfo);
            var commonAgentNameProvider = MockRepository.GenerateMock<ICommonAgentNameProvider>();
            var commonAgentNameSettings = new CommonNameDescriptionSetting();
            commonAgentNameProvider.Stub(x => x.CommonAgentNameSettings).Return(commonAgentNameSettings);
			

			using (var target = new StubbingControllerBuilder().CreateController<AgentsController>(new FakePermissionProvider(), teamRepository, personRepository, new Now(), null, userTimeZone, commonAgentNameProvider))
			{
				var expected = new AgentViewModel
				{
					PersonId = personId,
					Name = commonAgentNameSettings.BuildCommonNameDescription(person),
					SiteId = siteId.ToString(),
					SiteName = site.Description.Name,
					TeamId = teamId.ToString(),
					TeamName = team.Description.Name,
					TimeZoneOffsetMinutes = userTimeZone.TimeZone().GetUtcOffset(DateTime.Now).TotalMinutes
				};
				var result = target.ForTeam(teamId).Data as IEnumerable<AgentViewModel>;

				result.Count().Should().Be(1);

				Assert.That(result.Single().PersonId, Is.EqualTo(expected.PersonId));
				Assert.That(result.Single().Name, Is.EqualTo(expected.Name));
				Assert.That(result.Single().SiteId, Is.EqualTo(expected.SiteId));
				Assert.That(result.Single().SiteName, Is.EqualTo(expected.SiteName));
				Assert.That(result.Single().TeamId, Is.EqualTo(expected.TeamId));
				Assert.That(result.Single().TeamName, Is.EqualTo(expected.TeamName));
				Assert.That(result.Single().TimeZoneOffsetMinutes, Is.EqualTo(expected.TimeZoneOffsetMinutes));
			}

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
			person.AddPersonPeriod(new PersonPeriod(new DateOnly(date), PersonContractFactory.CreatePersonContract(" "," "," "), team));
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			personRepository.Stub(x => x.Get(person.Id.GetValueOrDefault())).Return(person);
			using (var target = new StubbingControllerBuilder().CreateController<AgentsController>(new FakePermissionProvider(), null, personRepository, new Now(), null, null, null))
			{
				var result = target.Team(person.Id.GetValueOrDefault(), date).Data;
				result.Should().Be(team.Id.GetValueOrDefault());
			}
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

			using (var target = new StubbingControllerBuilder().CreateController<AgentsController>(new FakePermissionProvider(), null, personRepository, new Now(), null, null, commonAgentNameProvider))
			{
				dynamic result = target.PersonDetails(person.Id.GetValueOrDefault()).Data;
				((object)result.Name).Should().Be(commonAgentNameSettings.BuildCommonNameDescription(person));
			}
		}

		
		private class fakeStateReader : IAgentStateReader
		{
			private readonly IEnumerable<AgentAdherenceStateInfo> _statesForAnyTeam;

			public fakeStateReader(IEnumerable<AgentAdherenceStateInfo> statesForAnyTeam)
			{
				_statesForAnyTeam = statesForAnyTeam;
			}

			public IEnumerable<AgentAdherenceStateInfo> GetLatestStatesForTeam(Guid teamId)
			{
				return _statesForAnyTeam;
			}
		}

	}
}
