using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Hubs
{
	[TestFixture]
	public class TeamScheduleProviderTest
	{
		[Test]
		public void ShouldGetTeamSchedule()
		{
			var teamId = Guid.NewGuid();
			var dateTime = new DateTime(2013, 3, 4, 0, 0, 0);
			var period = new DateTimePeriod(2013, 3, 4, 2013, 3, 5).ChangeEndTime(TimeSpan.FromHours(1));
			var personScheduleDayReadModelRepository = MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>();
			var data = new[] { new PersonScheduleDayReadModel { Shift = "{FirstName: 'Pierre'}" } };
			personScheduleDayReadModelRepository.Stub(x => x.ForTeam(period, teamId)).Return(data);
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(
				x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules))
			                  .Return(true);
			var target = new TeamScheduleProvider(personScheduleDayReadModelRepository, permissionProvider, MockRepository.GenerateMock<ISchedulePersonProvider>());

			var result = target.TeamSchedule(teamId, dateTime).First();
			result.Shift.Should().Be.EqualTo(data[0].Shift);
		}

		[Test]
		public void ShouldFilterOutUnpublishedSchedule()
		{
			var teamId = Guid.NewGuid();
			var dateTime = new DateTime(2013, 3, 4, 0, 0, 0);
			var period = new DateTimePeriod(2013, 3, 4, 2013, 3, 5).ChangeEndTime(TimeSpan.FromHours(1));
			var personScheduleDayReadModelRepository = MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>();
			var person1 = PersonFactory.CreatePersonWithSchedulePublishedToDate(new DateOnly(2013, 3, 10));
			var person2 = PersonFactory.CreatePersonWithSchedulePublishedToDate(new DateOnly(2013, 2, 10));
			var data = new[]
				{
					new PersonScheduleDayReadModel {PersonId = person1.Id.Value, Shift = "{FirstName: 'Pierre'}"},
					new PersonScheduleDayReadModel {PersonId = person2.Id.Value, Shift = "{FirstName: 'Ashley'}"}
				};
			personScheduleDayReadModelRepository.Stub(x => x.ForTeam(period, teamId)).Return(data);
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(
				x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules))
			                  .Return(false);
			var schedulePersonProvider = MockRepository.GenerateMock<ISchedulePersonProvider>();
			schedulePersonProvider.Stub(x => x.GetPermittedPersonsForTeam(new DateOnly(dateTime), teamId,
			                                                              DefinedRaptorApplicationFunctionPaths
				                                                              .SchedulesAnywhere)).Return(new[] {person1, person2});
			var target = new TeamScheduleProvider(personScheduleDayReadModelRepository, permissionProvider, schedulePersonProvider);

			var result = target.TeamSchedule(teamId, dateTime);
			result.Count().Should().Be.EqualTo(1);
			result.First().PersonId.Should().Be.EqualTo(person1.Id.Value);
		}
	}
}