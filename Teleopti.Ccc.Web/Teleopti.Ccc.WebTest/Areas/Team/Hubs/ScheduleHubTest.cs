using System;
using System.Linq;
using NUnit.Framework;
using Newtonsoft.Json.Linq;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.Team.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Team.Hubs
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable"), TestFixture]
	public class ScheduleHubTest
	{
		private IPersonScheduleDayReadModelRepository personScheduleDayReadModelRepository;
		private ITeamProvider teamProvider;
		private ScheduleHub hub;

		[SetUp]
		public void Setup()
		{
			personScheduleDayReadModelRepository = MockRepository.GenerateMock<IPersonScheduleDayReadModelRepository>();
			teamProvider = MockRepository.GenerateMock<ITeamProvider>();
			hub = new ScheduleHub(personScheduleDayReadModelRepository, teamProvider);
		}

		[Test]
		public void ShouldGetTeamsForDate()
		{
			var date = new DateOnly(2012, 12, 01);
			var team = TeamFactory.CreateTeam("Team","Site");
			team.SetId(Guid.NewGuid());

			teamProvider.Stub(x => x.GetPermittedTeams(date, DefinedRaptorApplicationFunctionPaths.SchedulesAdminWeb)).Return(
				new[] {team});

			dynamic result = hub.AvailableTeams(date.Date);
			dynamic teamResult = result.Teams[0];
			((object) teamResult.Id).Should().Be.EqualTo(team.Id);
			((object)teamResult.SiteAndTeam).Should().Be.EqualTo(team.SiteAndTeam);
		}

		[Test]
		public void ShouldGetSchedulesForDateAndTeam()
		{
			var period = new DateTimePeriod(2012, 12, 01,2012,12,01);
			var teamId = Guid.NewGuid();
			var model = new PersonScheduleDayReadModel
			            	{
			            		Shift =
			            			"{\"FirstName\":\"Jon\",\"LastName\":\"Kleinsmith\",\"EmploymentNumber\":\"137577\",\"Id\":\"b46a2588-8861-42e3-ab03-9b5e015b257c\",\"Date\":\"2011-09-01T00:00:00\",\"WorkTimeMinutes\":480,\"ContractTimeMinutes\":480,\"Projection\":[{\"Color\":\"#00FF00\",\"Start\":\"2011-09-01T07:30:00Z\",\"End\":\"2011-09-01T09:45:00Z\",\"Minutes\":135,\"Title\":\"Phone\"},{\"Color\":\"#FF0000\",\"Start\":\"2011-09-01T09:45:00Z\",\"End\":\"2011-09-01T10:00:00Z\",\"Minutes\":15,\"Title\":\"Short break\"},{\"Color\":\"#00FF00\",\"Start\":\"2011-09-01T10:00:00Z\",\"End\":\"2011-09-01T12:00:00Z\",\"Minutes\":120,\"Title\":\"Phone\"},{\"Color\":\"#FFFF00\",\"Start\":\"2011-09-01T12:00:00Z\",\"End\":\"2011-09-01T13:00:00Z\",\"Minutes\":60,\"Title\":\"Lunch\"},{\"Color\":\"#00FF00\",\"Start\":\"2011-09-01T13:00:00Z\",\"End\":\"2011-09-01T14:45:00Z\",\"Minutes\":105,\"Title\":\"Phone\"},{\"Color\":\"#FF0000\",\"Start\":\"2011-09-01T14:45:00Z\",\"End\":\"2011-09-01T15:00:00Z\",\"Minutes\":15,\"Title\":\"Short break\"},{\"Color\":\"#00FF00\",\"Start\":\"2011-09-01T15:00:00Z\",\"End\":\"2011-09-01T16:30:00Z\",\"Minutes\":90,\"Title\":\"Phone\"}]}"
			            	};

			personScheduleDayReadModelRepository.Stub(x => x.ForTeam(period.ChangeEndTime(TimeSpan.FromHours(25)), teamId)).Return(new[] { model });

			var result = hub.SubscribeTeamSchedule(teamId, period.StartDateTime);
			JObject shift = (JObject) result.Single();
			shift["EmploymentNumber"].Value<string>().Should().Be.EqualTo("137577");
			((JArray)shift["Projection"])[0]["Title"].Value<string>().Should().Be.EqualTo("Phone");
		}

		[TearDown]
		public void Teardown()
		{
			hub.Dispose();
		}
	}
}