using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Common.DataProvider
{
	[TestFixture]
	public class SchedulePersonProviderTest
	{

		[Test]
		public void ShouldQueryAllPersonInGivenTeam()
		{
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			var teamRepository = MockRepository.GenerateMock<ITeamRepository>();
			var team = new Team();
			team.SetId(Guid.NewGuid());
			var period = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today);

			teamRepository.Stub(x => x.Load(team.Id.GetValueOrDefault())).Return(team);

			var target = new SchedulePersonProvider(personRepository, new FakePermissionProvider(), teamRepository, null);

			target.GetPermittedPersonsForTeam(DateOnly.Today, team.Id.GetValueOrDefault(), DefinedRaptorApplicationFunctionPaths.TeamSchedule);

			personRepository.AssertWasCalled(x => x.FindPeopleBelongTeam(team, period));
		}

		[Test]
		public void ShouldFilterPermittedPersonWhenQueryingGivenTeam()
		{
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			var teamRepository = MockRepository.GenerateMock<ITeamRepository>();
			var team = new Team();
			team.SetId(Guid.NewGuid());
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			var persons = new[] { new Person(), new Person() };

			teamRepository.Stub(x => x.Load(team.Id.GetValueOrDefault())).Return(team);
			personRepository.Stub(x => x.FindPeopleBelongTeam(team, new DateOnlyPeriod())).IgnoreArguments().Return(persons);
			permissionProvider.Stub(x => x.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.TeamSchedule, DateOnly.Today, persons.ElementAt(0))).Return(false);
			permissionProvider.Stub(x => x.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.TeamSchedule, DateOnly.Today, persons.ElementAt(1))).Return(true);

			var target = new SchedulePersonProvider(personRepository, permissionProvider, teamRepository, null);

			var result = target.GetPermittedPersonsForTeam(DateOnly.Today, team.Id.GetValueOrDefault(), DefinedRaptorApplicationFunctionPaths.TeamSchedule);

			result.Single().Should().Be(persons.ElementAt(1));
		}

		[Test]
		public void ShouldGetPersonsInGivenGroup()
		{
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			var groupingReadOnlyRepository = MockRepository.GenerateMock<IGroupingReadOnlyRepository>();
			var groupId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			var readOnlyGroupDetail = new ReadOnlyGroupDetail
				{
					PersonId = personId
				};

			groupingReadOnlyRepository.Stub(x => x.DetailsForGroup(groupId, DateOnly.Today)).Return(new[] {readOnlyGroupDetail});

			var target = new SchedulePersonProvider(personRepository, new FakePermissionProvider(), null, groupingReadOnlyRepository);

			target.GetPermittedPersonsForGroup(DateOnly.Today, groupId, DefinedRaptorApplicationFunctionPaths.TeamSchedule);

			personRepository.AssertWasCalled(x => x.FindPeople(Arg<IEnumerable<Guid>>.List.ContainsAll(new[]{personId})));
		}


		[Test]
		public void ShouldFilterPermittedPersonWhenQueryingGivenPeopleList()
		{
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			var persons = new[] { new Person(), new Person() };

			personRepository.Stub(x => x.FindPeople(persons.Select(p=>p.Id.GetValueOrDefault()))).IgnoreArguments().Return(persons);
			permissionProvider.Stub(x => x.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.TeamSchedule, DateOnly.Today, persons.ElementAt(0))).Return(false);
			permissionProvider.Stub(x => x.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.TeamSchedule, DateOnly.Today, persons.ElementAt(1))).Return(true);

			var target = new SchedulePersonProvider(personRepository, permissionProvider, null, null);

			var result = target.GetPermittedPeople(new GroupScheduleInput { PersonIds = persons.Select(p => p.Id.GetValueOrDefault()), ScheduleDate = DateOnly.Today.Date}, DefinedRaptorApplicationFunctionPaths.TeamSchedule);

			result.Single().Should().Be(persons.ElementAt(1));
		}
	}
}