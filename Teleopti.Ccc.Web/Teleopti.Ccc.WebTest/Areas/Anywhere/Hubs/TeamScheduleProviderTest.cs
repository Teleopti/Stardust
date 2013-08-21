using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Scheduling;
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
		private ILoggedOnUser _loggedOnUser;
		private IPerson _user;

		[SetUp]
		public void SetUp()
		{
			_loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			_user = PersonFactory.CreatePersonWithId();
			_loggedOnUser.Stub(x => x.CurrentUser()).Return(_user);
		}

		[Test]
		public void ShouldGetTeamSchedule()
		{
			var teamId = Guid.NewGuid();
			var scheduleDate = new DateTime(2013, 3, 4, 0, 0, 0);
			var period = new DateTimePeriod(2013, 3, 4, 2013, 3, 5).ChangeEndTime(TimeSpan.FromHours(1));
			var personScheduleDayReadModelRepository = MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>();
			var person = PersonFactory.CreatePersonWithSchedulePublishedToDate(new DateOnly(scheduleDate.AddDays(1))); // set the published date one day after the schedule time
			var shifts = new[] {new PersonScheduleDayReadModel {Shift = "{FirstName: 'Pierre', Projection: [{Title:'Vacation', Color:'Red'}]}"}};
			personScheduleDayReadModelRepository.Stub(x => x.ForTeam(period, teamId)).Return(shifts);
			
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(
				x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules))
							  .Return(true);
			var schedulePersonProvider = MockRepository.GenerateStub<ISchedulePersonProvider>();
			schedulePersonProvider.Stub(x => x.GetPermittedPersonsForTeam(new DateOnly(scheduleDate), teamId,
				DefinedRaptorApplicationFunctionPaths.ViewConfidential))
				.Return(new[] { person });
			var target = new TeamScheduleProvider(personScheduleDayReadModelRepository, permissionProvider, schedulePersonProvider);

			var result = target.TeamSchedule(teamId, scheduleDate).First();
			result.FirstName.Should().Be.EqualTo("Pierre");
		}

		[Test]
		public void ShouldFilterOutUnpublishedSchedule()
		{
			var teamId = Guid.NewGuid();
			var scheduleDate = new DateTime(2013, 3, 4, 0, 0, 0);
			var period = new DateTimePeriod(2013, 3, 4, 2013, 3, 5).ChangeEndTime(TimeSpan.FromHours(1));
			var personScheduleDayReadModelRepository = MockRepository.GenerateStub<IPersonScheduleDayReadModelFinder>();
			var personWithPublishedSchedule = PersonFactory.CreatePersonWithSchedulePublishedToDate(new DateOnly(scheduleDate.AddDays(1))); // set the published date one day after the schedule time
			var personWithUnpublishedSchedule = PersonFactory.CreatePersonWithSchedulePublishedToDate(new DateOnly(scheduleDate.AddDays(-1))); // set the published date one day before the schedule time
			var schedules = new[]
				{
					new PersonScheduleDayReadModel {PersonId = personWithPublishedSchedule.Id.Value, Shift = "{FirstName: 'Published', Projection: [{Title:'Vacation', Color:'Red'}]}"},
					new PersonScheduleDayReadModel {PersonId = personWithUnpublishedSchedule.Id.Value, Shift = "{FirstName: 'Unpublished'}"}
				};
			personScheduleDayReadModelRepository.Stub(x => x.ForTeam(period, teamId))
				.Return(schedules);

			// has not permission for ViewUnpublishedSchedules
			var permissionProvider = MockRepository.GenerateStub<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules))
				.Return(false);

			var schedulePersonProvider = MockRepository.GenerateStub<ISchedulePersonProvider>();
			schedulePersonProvider.Stub(x => x.GetPermittedPersonsForTeam(new DateOnly(scheduleDate), teamId,
				DefinedRaptorApplicationFunctionPaths.SchedulesAnywhere))
				.Return(new[] { personWithPublishedSchedule, personWithUnpublishedSchedule });
			schedulePersonProvider.Stub(x => x.GetPermittedPersonsForTeam(new DateOnly(scheduleDate), teamId,
				DefinedRaptorApplicationFunctionPaths.ViewConfidential))
				.Return(new[] { personWithPublishedSchedule, personWithUnpublishedSchedule });

			var target = new TeamScheduleProvider(personScheduleDayReadModelRepository, permissionProvider, schedulePersonProvider);

			var result = target.TeamSchedule(teamId, scheduleDate);
			result.Count().Should().Be.EqualTo(1);
			result.First().FirstName.Should().Be.EqualTo("Published");
		}

		[Test]
		public void ShouldGreyConfidentialAbsenceIfNoPermission()
		{
			var teamId = Guid.NewGuid();
			var scheduleDate = new DateTime(2013, 3, 4, 0, 0, 0);
			var period = new DateTimePeriod(2013, 3, 4, 2013, 3, 5).ChangeEndTime(TimeSpan.FromHours(1));
			var person = PersonFactory.CreatePersonWithSchedulePublishedToDate(new DateOnly(scheduleDate.AddDays(1)));
			var personScheduleDayReadModelRepository = MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>();
			var schedules = new[]
				{
					new PersonScheduleDayReadModel
						{
							PersonId = person.Id.Value,
							Shift =
								"{FirstName: 'Pierre', Projection:[{Start:'2013-07-08T06:30:00Z',End:'2013-07-08T08:30:00Z',Title:'Vacation', Color:'Red', IsAbsenceConfidential:'true'}]}"
						}
				};
			personScheduleDayReadModelRepository.Stub(x => x.ForTeam(period, teamId)).Return(schedules);
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			var schedulePersonProvider = MockRepository.GenerateMock<ISchedulePersonProvider>();
			schedulePersonProvider.Stub(x => x.GetPermittedPersonsForTeam(new DateOnly(scheduleDate), teamId,
				DefinedRaptorApplicationFunctionPaths.SchedulesAnywhere))
				.Return(new[] { person });
			schedulePersonProvider.Stub(x => x.GetPermittedPersonsForTeam(new DateOnly(scheduleDate), teamId,
			     DefinedRaptorApplicationFunctionPaths.ViewConfidential))
			     .Return(new List<IPerson>()); // no person with confidental

			var target = new TeamScheduleProvider(personScheduleDayReadModelRepository, permissionProvider, schedulePersonProvider);

			var result = target.TeamSchedule(teamId, scheduleDate).First();
			result.FirstName.Should().Be.EqualTo("Pierre");
			result.Projection[0].Title.Should().Be.EqualTo(ConfidentialPayloadValues.Description.Name);
			result.Projection[0].Color.Should().Be.EqualTo(ColorTranslator.ToHtml(ConfidentialPayloadValues.DisplayColor));
		}

		[Test]
		public void ShouldNotGreyNormalAbsenceIfNoPermission()
		{
			var teamId = Guid.NewGuid();
			var scheduleDate = new DateTime(2013, 3, 4, 0, 0, 0);
			var period = new DateTimePeriod(2013, 3, 4, 2013, 3, 5).ChangeEndTime(TimeSpan.FromHours(1));
			var person = PersonFactory.CreatePersonWithSchedulePublishedToDate(new DateOnly(scheduleDate.AddDays(1)));
			var personScheduleDayReadModelRepository = MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>();
			var schedules = new[]
				{
					new PersonScheduleDayReadModel
						{
							PersonId = person.Id.Value,
							Shift =
								"{FirstName: 'Pierre', Projection:[{Start:'2013-07-08T06:30:00Z',End:'2013-07-08T08:30:00Z',Title:'Vacation', Color:'Red', IsAbsenceConfidential:'false'}]}"
						}
				};
			personScheduleDayReadModelRepository.Stub(x => x.ForTeam(period, teamId)).Return(schedules);
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			var schedulePersonProvider = MockRepository.GenerateMock<ISchedulePersonProvider>();
			schedulePersonProvider.Stub(x => x.GetPermittedPersonsForTeam(new DateOnly(scheduleDate), teamId,
				DefinedRaptorApplicationFunctionPaths.SchedulesAnywhere))
				.Return(new[] { person });
			schedulePersonProvider.Stub(x => x.GetPermittedPersonsForTeam(new DateOnly(scheduleDate), teamId,
				 DefinedRaptorApplicationFunctionPaths.ViewConfidential))
				 .Return(new List<IPerson>()); // no person with confidental

			var target = new TeamScheduleProvider(personScheduleDayReadModelRepository, permissionProvider, schedulePersonProvider);

			var result = target.TeamSchedule(teamId, scheduleDate).First();
			result.FirstName.Should().Be.EqualTo("Pierre");
			result.Projection[0].Title.Should().Be.EqualTo("Vacation");
			result.Projection[0].Color.Should().Be.EqualTo("Red");
		}


		[Test]
		public void ShouldNotGreyConfidentialAbsenceIfHasPermission()
		{
			var teamId = Guid.NewGuid();
			var scheduleDate = new DateTime(2013, 3, 4, 0, 0, 0);
			var period = new DateTimePeriod(2013, 3, 4, 2013, 3, 5).ChangeEndTime(TimeSpan.FromHours(1));
			var person = PersonFactory.CreatePersonWithSchedulePublishedToDate(new DateOnly(scheduleDate.AddDays(1)));
			var personScheduleDayReadModelRepository = MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>();
			var schedules = new[]
				{
					new PersonScheduleDayReadModel
						{
							PersonId = person.Id.Value,
							Shift =
								"{FirstName: 'Pierre', Projection:[{Start:'2013-07-08T06:30:00Z',End:'2013-07-08T08:30:00Z',Title:'Vacation', Color:'Red', IsAbsenceConfidential:'true'}]}"
						}
				};
			personScheduleDayReadModelRepository.Stub(x => x.ForTeam(period, teamId)).Return(schedules);
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			var schedulePersonProvider = MockRepository.GenerateMock<ISchedulePersonProvider>();
			schedulePersonProvider.Stub(x => x.GetPermittedPersonsForTeam(new DateOnly(scheduleDate), teamId,
				DefinedRaptorApplicationFunctionPaths.SchedulesAnywhere))
				.Return(new[] { person });
			schedulePersonProvider.Stub(x => x.GetPermittedPersonsForTeam(new DateOnly(scheduleDate), teamId,
				 DefinedRaptorApplicationFunctionPaths.ViewConfidential))
				 .Return(new[] { person }); // no person with confidental

			var target = new TeamScheduleProvider(personScheduleDayReadModelRepository, permissionProvider, schedulePersonProvider);

			var result = target.TeamSchedule(teamId, scheduleDate).First();
			result.FirstName.Should().Be.EqualTo("Pierre");
			result.Projection[0].Title.Should().Be.EqualTo("Vacation");
			result.Projection[0].Color.Should().Be.EqualTo(ColorTranslator.ToHtml(Color.Red));
		}
	}
}