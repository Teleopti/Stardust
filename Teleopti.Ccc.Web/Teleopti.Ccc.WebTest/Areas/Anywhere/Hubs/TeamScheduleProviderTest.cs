using System;
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
			var dateTime = new DateTime(2013, 3, 4, 0, 0, 0);
			var period = new DateTimePeriod(2013, 3, 4, 2013, 3, 5).ChangeEndTime(TimeSpan.FromHours(1));
			var personScheduleDayReadModelRepository = MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>();
			var data = new[] { new PersonScheduleDayReadModel { Shift = "{FirstName: 'Pierre'}" } };
			personScheduleDayReadModelRepository.Stub(x => x.ForTeam(period, teamId)).Return(data);
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(
				x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules))
			                  .Return(true);
			permissionProvider.Stub(
				x => x.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.ViewConfidential, DateOnly.Today, _user))
			                  .Return(true);
			var target = new TeamScheduleProvider(personScheduleDayReadModelRepository, permissionProvider, null, _loggedOnUser);

			var result = target.TeamSchedule(teamId, dateTime).First();
			result.FirstName.Should().Be.EqualTo("Pierre");
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
			permissionProvider.Stub(
				x => x.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.ViewConfidential, DateOnly.Today, _user))
							  .Return(true);
			var schedulePersonProvider = MockRepository.GenerateMock<ISchedulePersonProvider>();
			schedulePersonProvider.Stub(x => x.GetPermittedPersonsForTeam(new DateOnly(dateTime), teamId,
			                                                              DefinedRaptorApplicationFunctionPaths
				                                                              .SchedulesAnywhere)).Return(new[] {person1, person2});
			var target = new TeamScheduleProvider(personScheduleDayReadModelRepository, permissionProvider, schedulePersonProvider, _loggedOnUser);

			var result = target.TeamSchedule(teamId, dateTime);
			result.Count().Should().Be.EqualTo(1);
			result.First().FirstName.Should().Be.EqualTo("Pierre");
		}

		[Test]
		public void ShouldGreyConfidentialAbsenceIfNoPermission()
		{
			var teamId = Guid.NewGuid();
			var dateTime = new DateTime(2013, 3, 4, 0, 0, 0);
			var period = new DateTimePeriod(2013, 3, 4, 2013, 3, 5).ChangeEndTime(TimeSpan.FromHours(1));
			var personScheduleDayReadModelRepository = MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>();
			var data = new[]
				{
					new PersonScheduleDayReadModel
						{
							Shift =
								"{FirstName: 'Pierre', Projection:[{Start:'2013-07-08T06:30:00Z',End:'2013-07-08T08:30:00Z',Title:'Vocation', Color:'Red', IsAbsenceConfidential:'true'}]}"
						}
				};
			personScheduleDayReadModelRepository.Stub(x => x.ForTeam(period, teamId)).Return(data);
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(
				x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules))
							  .Return(true);
			permissionProvider.Stub(
				x => x.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.ViewConfidential, DateOnly.Today, _user))
							  .Return(false);
			var target = new TeamScheduleProvider(personScheduleDayReadModelRepository, permissionProvider, null, _loggedOnUser);

			var result = target.TeamSchedule(teamId, dateTime).First();
			result.FirstName.Should().Be.EqualTo("Pierre");
			result.Projection[0].Title.Should().Be.EqualTo(ConfidentialPayloadValues.Description.Name);
			result.Projection[0].Color.Should().Be.EqualTo(ColorTranslator.ToHtml(ConfidentialPayloadValues.DisplayColor));
		}

		[Test]
		public void ShouldNotGreyNormalAbsenceIfNoPermission()
		{
			var teamId = Guid.NewGuid();
			var dateTime = new DateTime(2013, 3, 4, 0, 0, 0);
			var period = new DateTimePeriod(2013, 3, 4, 2013, 3, 5).ChangeEndTime(TimeSpan.FromHours(1));
			var personScheduleDayReadModelRepository = MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>();
			var data = new[]
				{
					new PersonScheduleDayReadModel
						{
							Shift =
								"{FirstName: 'Pierre', Projection:[{Start:'2013-07-08T06:30:00Z',End:'2013-07-08T08:30:00Z',Title:'Vocation', Color:'Red'}]}"
						}
				};
			personScheduleDayReadModelRepository.Stub(x => x.ForTeam(period, teamId)).Return(data);
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(
				x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules))
							  .Return(true);
			permissionProvider.Stub(
				x => x.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.ViewConfidential, DateOnly.Today, _user))
							  .Return(false);
			var target = new TeamScheduleProvider(personScheduleDayReadModelRepository, permissionProvider, null, _loggedOnUser);

			var result = target.TeamSchedule(teamId, dateTime).First();
			result.FirstName.Should().Be.EqualTo("Pierre");
			result.Projection[0].Title.Should().Be.EqualTo("Vocation");
			result.Projection[0].Color.Should().Be.EqualTo("Red");
		}
	}
}