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
	public class TeamScheduleViewModelFactoryTest
	{
		private Guid _teamId;
		private DateTime _scheduleDate;
		private DateTimePeriod _period;
		private IPermissionProvider _permissionProvider;
		private ISchedulePersonProvider _schedulePersonProvider;
		private IPersonScheduleDayReadModelFinder _personScheduleDayReadModelRepository;
		private IPerson _personWithPublishedSchedule; 

		[SetUp]
		public void Setup()
		{
			_teamId = Guid.NewGuid();
			_scheduleDate = new DateTime(2013, 3, 4, 0, 0, 0);
			_period = new DateTimePeriod(2013, 3, 4, 2013, 3, 5).ChangeEndTime(TimeSpan.FromHours(1));

			_permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			_schedulePersonProvider = MockRepository.GenerateStub<ISchedulePersonProvider>();
			_personScheduleDayReadModelRepository = MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>();

			_personWithPublishedSchedule = 
				PersonFactory.CreatePersonWithSchedulePublishedToDate(new DateOnly(_scheduleDate.AddDays(1))); // set the published date one day after the schedule time

		}

		[Test]
		public void ShouldGetTeamSchedule()
		{
			var shifts = new[] {new PersonScheduleDayReadModel {Shift = "{FirstName: 'Pierre', Projection: [{Title:'Vacation', Color:'Red'}]}"}};

			_personScheduleDayReadModelRepository.Stub(x => x.ForTeam(_period, _teamId))
				.Return(shifts);
			_permissionProvider.Stub(x => x.HasApplicationFunctionPermission(
				DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules))
				.Return(true);
			_schedulePersonProvider.Stub(x => x.GetPermittedPersonsForTeam(new DateOnly(_scheduleDate), _teamId, 
				DefinedRaptorApplicationFunctionPaths.ViewConfidential))
				.Return(new[] { _personWithPublishedSchedule });

			var target = new TeamScheduleViewModelFactory(new TeamScheduleViewModelMapper(), _personScheduleDayReadModelRepository, _permissionProvider, _schedulePersonProvider);

			var result = target.CreateViewModel(_teamId, _scheduleDate).First();
			result.FirstName.Should().Be.EqualTo("Pierre");
		}

		[Test]
		public void ShouldFilterOutUnpublishedSchedule()
		{
			var personWithUnpublishedSchedule = PersonFactory.CreatePersonWithSchedulePublishedToDate(new DateOnly(_scheduleDate.AddDays(-1))); // set the published date one day before the schedule time

			var shifts = new[]
				{
					new PersonScheduleDayReadModel {PersonId = _personWithPublishedSchedule.Id.Value, Shift = "{FirstName: 'Published', Projection: [{Title:'Vacation', Color:'Red'}]}"},
					new PersonScheduleDayReadModel {PersonId = personWithUnpublishedSchedule.Id.Value, Shift = "{FirstName: 'Unpublished'}"}
				};

			_personScheduleDayReadModelRepository.Stub(x => x.ForTeam(_period, _teamId))
				.Return(shifts);
			_permissionProvider.Stub(x => x.HasApplicationFunctionPermission(
				DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules))
				.Return(false);
			_schedulePersonProvider.Stub(x => x.GetPermittedPersonsForTeam(new DateOnly(_scheduleDate), _teamId,
				DefinedRaptorApplicationFunctionPaths.SchedulesAnywhere))
				.Return(new[] { _personWithPublishedSchedule, personWithUnpublishedSchedule });
			_schedulePersonProvider.Stub(x => x.GetPermittedPersonsForTeam(new DateOnly(_scheduleDate), _teamId,
				DefinedRaptorApplicationFunctionPaths.ViewConfidential))
				.Return(new[] { _personWithPublishedSchedule, personWithUnpublishedSchedule });

			var target = new TeamScheduleViewModelFactory(new TeamScheduleViewModelMapper(), _personScheduleDayReadModelRepository, _permissionProvider, _schedulePersonProvider);

			var result = target.CreateViewModel(_teamId, _scheduleDate);
			result.Count().Should().Be.EqualTo(1);
			result.First().FirstName.Should().Be.EqualTo("Published");
		}

		[Test]
		public void ShouldGreyConfidentialAbsenceIfNoPermission()
		{
			var shifts = new[]
				{
					new PersonScheduleDayReadModel
						{
							PersonId = _personWithPublishedSchedule.Id.Value,
							Shift =
								"{FirstName: 'Pierre', Projection:[{Start:'2013-07-08T06:30:00Z',End:'2013-07-08T08:30:00Z',Title:'Vacation', Color:'Red', IsAbsenceConfidential:'true'}]}"
						}
				};
			_personScheduleDayReadModelRepository.Stub(x => x.ForTeam(_period, _teamId)).Return(shifts);
			_schedulePersonProvider.Stub(x => x.GetPermittedPersonsForTeam(new DateOnly(_scheduleDate), _teamId,
				DefinedRaptorApplicationFunctionPaths.SchedulesAnywhere))
				.Return(new[] { _personWithPublishedSchedule });
			_schedulePersonProvider.Stub(x => x.GetPermittedPersonsForTeam(new DateOnly(_scheduleDate), _teamId,
			     DefinedRaptorApplicationFunctionPaths.ViewConfidential))
			     .Return(new List<IPerson>()); // no person with confidental

			var target = new TeamScheduleViewModelFactory(new TeamScheduleViewModelMapper(), _personScheduleDayReadModelRepository, _permissionProvider, _schedulePersonProvider);

			var result = target.CreateViewModel(_teamId, _scheduleDate).First();
			result.FirstName.Should().Be.EqualTo("Pierre");
			result.Projection[0].Title.Should().Be.EqualTo(ConfidentialPayloadValues.Description.Name);
			result.Projection[0].Color.Should().Be.EqualTo(ColorTranslator.ToHtml(ConfidentialPayloadValues.DisplayColor));
		}

		[Test]
		public void ShouldNotGreyNormalAbsenceIfNoPermission()
		{
			var shifts = new[]
				{
					new PersonScheduleDayReadModel
						{
							PersonId = _personWithPublishedSchedule.Id.Value,
							Shift =
								"{FirstName: 'Pierre', Projection:[{Start:'2013-07-08T06:30:00Z',End:'2013-07-08T08:30:00Z',Title:'Vacation', Color:'Red', IsAbsenceConfidential:'false'}]}"
						}
				};
			_personScheduleDayReadModelRepository.Stub(x => x.ForTeam(_period, _teamId)).Return(shifts);
			_schedulePersonProvider.Stub(x => x.GetPermittedPersonsForTeam(new DateOnly(_scheduleDate), _teamId,
				DefinedRaptorApplicationFunctionPaths.SchedulesAnywhere))
				.Return(new[] { _personWithPublishedSchedule });
			_schedulePersonProvider.Stub(x => x.GetPermittedPersonsForTeam(new DateOnly(_scheduleDate), _teamId,
				 DefinedRaptorApplicationFunctionPaths.ViewConfidential))
				 .Return(new List<IPerson>()); // no person with confidental

			var target = new TeamScheduleViewModelFactory(new TeamScheduleViewModelMapper(), _personScheduleDayReadModelRepository, _permissionProvider, _schedulePersonProvider);

			var result = target.CreateViewModel(_teamId, _scheduleDate).First();
			result.FirstName.Should().Be.EqualTo("Pierre");
			result.Projection[0].Title.Should().Be.EqualTo("Vacation");
			result.Projection[0].Color.Should().Be.EqualTo("Red");
		}


		[Test]
		public void ShouldNotGreyConfidentialAbsenceIfHasPermission()
		{
			var shifts = new[]
				{
					new PersonScheduleDayReadModel
						{
							PersonId = _personWithPublishedSchedule.Id.Value,
							Shift =
								"{FirstName: 'Pierre', Projection:[{Start:'2013-07-08T06:30:00Z',End:'2013-07-08T08:30:00Z',Title:'Vacation', Color:'Red', IsAbsenceConfidential:'true'}]}"
						}
				};
			_personScheduleDayReadModelRepository.Stub(x => x.ForTeam(_period, _teamId)).Return(shifts);
			_schedulePersonProvider.Stub(x => x.GetPermittedPersonsForTeam(new DateOnly(_scheduleDate), _teamId,
				DefinedRaptorApplicationFunctionPaths.SchedulesAnywhere))
				.Return(new[] { _personWithPublishedSchedule });
			_schedulePersonProvider.Stub(x => x.GetPermittedPersonsForTeam(new DateOnly(_scheduleDate), _teamId,
				 DefinedRaptorApplicationFunctionPaths.ViewConfidential))
				 .Return(new[] { _personWithPublishedSchedule });

			var target = new TeamScheduleViewModelFactory(new TeamScheduleViewModelMapper(), _personScheduleDayReadModelRepository, _permissionProvider, _schedulePersonProvider);

			var result = target.CreateViewModel(_teamId, _scheduleDate).First();
			result.FirstName.Should().Be.EqualTo("Pierre");
			result.Projection[0].Title.Should().Be.EqualTo("Vacation");
			result.Projection[0].Color.Should().Be.EqualTo(ColorTranslator.ToHtml(Color.Red));
		}
	}
}