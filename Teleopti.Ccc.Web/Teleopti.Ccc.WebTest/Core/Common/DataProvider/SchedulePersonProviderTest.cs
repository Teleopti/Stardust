using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal;
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

			teamRepository.Stub(x => x.Load(team.Id.Value)).Return(team);

			var target = new SchedulePersonProvider(personRepository, new FakePermissionProvider(), null, teamRepository);

			target.GetPermittedPersonsForTeam(DateOnly.Today, team.Id.Value);

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

			teamRepository.Stub(x => x.Load(team.Id.Value)).Return(team);
			personRepository.Stub(x => x.FindPeopleBelongTeam(team, new DateOnlyPeriod())).IgnoreArguments().Return(persons);
			permissionProvider.Stub(x => x.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.TeamSchedule, DateOnly.Today, persons.ElementAt(0))).Return(false);
			permissionProvider.Stub(x => x.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.TeamSchedule, DateOnly.Today, persons.ElementAt(1))).Return(true);

			var target = new SchedulePersonProvider(personRepository, permissionProvider, null, teamRepository);

			var result = target.GetPermittedPersonsForTeam(DateOnly.Today, team.Id.Value);

			result.Single().Should().Be(persons.ElementAt(1));
		}
	}
}