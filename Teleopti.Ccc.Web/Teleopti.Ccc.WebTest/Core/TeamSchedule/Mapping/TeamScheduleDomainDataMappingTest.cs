using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping;


namespace Teleopti.Ccc.WebTest.Core.TeamSchedule.Mapping
{
	[TestFixture]
	public class TeamScheduleDomainDataMappingTest
	{
		private IScheduleProvider scheduleProvider;
		private ITeamScheduleProjectionForMTWProvider _projectionForMtwProvider;
		private ISchedulePersonProvider personProvider;
		private IUserTimeZone userTimeZone;
		private TeamScheduleDomainDataMapper target;

		[SetUp]
		public void Setup()
		{
			scheduleProvider = MockRepository.GenerateMock<IScheduleProvider>();
			personProvider = MockRepository.GenerateMock<ISchedulePersonProvider>();
			_projectionForMtwProvider = MockRepository.GenerateStub<ITeamScheduleProjectionForMTWProvider>();
			userTimeZone = new FakeUserTimeZone(TimeZoneInfo.Utc);
			
			target = new TeamScheduleDomainDataMapper(personProvider,scheduleProvider,_projectionForMtwProvider, userTimeZone);
		}
		
		[Test]
		public void ShouldMapDate()
		{
			var result = target.Map(DateOnly.Today, Guid.Empty);

			result.Date.Should().Be.EqualTo(DateOnly.Today);
		}

		[Test]
		public void ShouldMapTeamId()
		{
			var id = Guid.NewGuid();

			var result = target.Map(DateOnly.Today, id);

			result.TeamOrGroupId.Should().Be(id);
		}

		[Test]
		public void ShouldGetPermittedPersonsForTeam()
		{
			var persons = new IPerson[] { };
			var id = Guid.NewGuid();

			personProvider.Stub(x => x.GetPermittedPersonsForGroup(DateOnly.Today, id, DefinedRaptorApplicationFunctionPaths.ViewSchedules)).Return(persons);

			target.Map(DateOnly.Today, id);

			scheduleProvider.AssertWasCalled(x => x.GetScheduleForPersons(DateOnly.Today, persons));
		}

		[Test]
		public void ShouldMapPerson()
		{
			var person = new Person();

			personProvider.Stub(x => x.GetPermittedPersonsForGroup(DateOnly.Today, Guid.Empty, DefinedRaptorApplicationFunctionPaths.ViewSchedules)).Return(new[] { person });

			var result = target.Map(DateOnly.Today, Guid.Empty);

			result.Days.Single().Person.Should().Be(person);
		}

		[Test]
		public void ShouldMapLayers()
		{
			var persons = new[] { new Person() };
			var scheduleDay = new StubFactory().ScheduleDayStub(DateTime.Today, persons.Single());
			var teamScheduleProjection = new TeamScheduleProjection(new[] { new TeamScheduleLayer() }, DateTime.MaxValue);

			personProvider.Stub(x => x.GetPermittedPersonsForGroup(DateOnly.Today, Guid.Empty, DefinedRaptorApplicationFunctionPaths.ViewSchedules)).Return(persons);
			scheduleProvider.Stub(x => x.GetScheduleForPersons(DateOnly.Today, persons)).Return(new[] { scheduleDay });
			_projectionForMtwProvider.Stub(x => x.Projection(scheduleDay)).Return(teamScheduleProjection);

			var result = target.Map(DateOnly.Today, Guid.Empty);

			result.Days.First().Projection.Should().Be.SameInstanceAs(teamScheduleProjection);
		}

		[Test]
		public void ShouldSortDaysOnProjectionSortDate()
		{
			var stubs = new StubFactory();
			var persons = new[] { new Person(), new Person() };
			var scheduleDays = new[]
			                   	{
			                   		stubs.ScheduleDayStub(DateHelper.MinSmallDateTime, persons.ElementAt(0)),
			                   		stubs.ScheduleDayStub(DateHelper.MinSmallDateTime, persons.ElementAt(1))
			                   	};
			var eveningProjection = new TeamScheduleProjection { SortDate = DateTime.Now.Date.AddHours(20) };
			var morningProjection = new TeamScheduleProjection { SortDate = DateTime.Now.Date.AddHours(7) };

			personProvider.Stub(x => x.GetPermittedPersonsForGroup(DateOnly.Today, Guid.Empty, DefinedRaptorApplicationFunctionPaths.ViewSchedules)).Return(persons);
			scheduleProvider.Stub(x => x.GetScheduleForPersons(DateOnly.Today, persons)).Return(scheduleDays);
			_projectionForMtwProvider.Stub(x => x.Projection(scheduleDays.ElementAt(0))).Return(eveningProjection);
			_projectionForMtwProvider.Stub(x => x.Projection(scheduleDays.ElementAt(1))).Return(morningProjection);

			var result = target.Map(DateOnly.Today, Guid.Empty);

			result.Days.Select(d => d.Projection)
				.Should().Have.SameSequenceAs(new[] {morningProjection, eveningProjection});
		}

		[Test]
		public void ShouldMapDisplayTimePeriodToQuartersHour()
		{
			var persons = new[] { new Person() };
			var scheduleDay = new StubFactory().ScheduleDayStub(DateTime.Today, persons.Single());
			var startTime = new DateTime(2012, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var endTime = new DateTime(2012, 1, 1, 17, 0, 0, DateTimeKind.Utc);
			var period = new DateTimePeriod(startTime, endTime);

			var layers = new TeamScheduleProjection(new[]
			             	{
			             		new TeamScheduleLayer
			             			{
			             				Period = period
			             			}
			             	}, DateTime.MaxValue);
			personProvider.Stub(x => x.GetPermittedPersonsForGroup(DateOnly.Today, Guid.Empty, DefinedRaptorApplicationFunctionPaths.ViewSchedules)).Return(persons);
			scheduleProvider.Stub(x => x.GetScheduleForPersons(DateOnly.Today, persons)).Return(new[] { scheduleDay });
			_projectionForMtwProvider.Stub(x => x.Projection(null)).IgnoreArguments().Return(layers);

			var result = target.Map(DateOnly.Today, Guid.Empty);

			result.DisplayTimePeriod.StartDateTime.Should().Be.EqualTo(new DateTime(2012, 1, 1, 7, 45, 0));
			result.DisplayTimePeriod.EndDateTime.Should().Be.EqualTo(new DateTime(2012, 1, 1, 17, 15, 0));
		}

		[Test]
		public void ShouldMapDisplayTimePeriodFromQuartersToHour()
		{
			var persons = new[] { new Person() };
			var scheduleDay = new StubFactory().ScheduleDayStub(DateTime.Today, persons.Single());
			var startTime = new DateTime(2012, 1, 1, 8, 15, 0, DateTimeKind.Utc);
			var endTime = new DateTime(2012, 1, 1, 17, 15, 0, DateTimeKind.Utc);
			var period = new DateTimePeriod(startTime, endTime);

			var layers = new TeamScheduleProjection(new[]
			             	{
			             		new TeamScheduleLayer
			             			{
			             				Period = period
			             			}
			             	}, DateTime.MaxValue);
			personProvider.Stub(x => x.GetPermittedPersonsForGroup(DateOnly.Today, Guid.Empty, DefinedRaptorApplicationFunctionPaths.ViewSchedules)).Return(persons);
			scheduleProvider.Stub(x => x.GetScheduleForPersons(DateOnly.Today, persons)).Return(new[] { scheduleDay });
			_projectionForMtwProvider.Stub(x => x.Projection(null)).IgnoreArguments().Return(layers);

			var result = target.Map(DateOnly.Today, Guid.Empty);

			result.DisplayTimePeriod.StartDateTime.Should().Be.EqualTo(new DateTime(2012, 1, 1, 8, 0, 0));
			result.DisplayTimePeriod.EndDateTime.Should().Be.EqualTo(new DateTime(2012, 1, 1, 17, 30, 0));
		}

		[Test]
		public void ShouldMapDisplayTimeRoundToWholeQuarters()
		{
			var persons = new[] { new Person() };
			var scheduleDay = new StubFactory().ScheduleDayStub(DateTime.Today, persons.Single());
			var startTime = new DateTime(2012, 1, 1, 8, 55, 0, DateTimeKind.Utc);
			var endTime = new DateTime(2012, 1, 1, 17, 5, 0, DateTimeKind.Utc);
			var period = new DateTimePeriod(startTime, endTime);

			var layers = new TeamScheduleProjection(new[]
			             	{
			             		new TeamScheduleLayer
			             			{
			             				Period = period
			             			}
			             	}, DateTime.MaxValue);
			personProvider.Stub(x => x.GetPermittedPersonsForGroup(DateOnly.Today, Guid.Empty, DefinedRaptorApplicationFunctionPaths.ViewSchedules)).Return(persons);
			scheduleProvider.Stub(x => x.GetScheduleForPersons(DateOnly.Today, persons)).Return(new[] { scheduleDay });
			_projectionForMtwProvider.Stub(x => x.Projection(null)).IgnoreArguments().Return(layers);

			var result = target.Map(DateOnly.Today, Guid.Empty);

			result.DisplayTimePeriod.StartDateTime.Should().Be.EqualTo(new DateTime(2012, 1, 1, 8, 30, 0));
			result.DisplayTimePeriod.EndDateTime.Should().Be.EqualTo(new DateTime(2012, 1, 1, 17, 30, 0));
		}

		[Test]
		public void ShouldMapDisplayTimeToDefaultIfNull()
		{
			var result = target.Map(DateOnly.Today, Guid.Empty);

			result.DisplayTimePeriod.StartDateTime.Should().Be.EqualTo(DateTime.Now.Date.Add(TeamScheduleDomainData.DefaultDisplayTime.StartTime).AddMinutes(-15));
			result.DisplayTimePeriod.EndDateTime.Should().Be.EqualTo(DateTime.Now.Date.Add(TeamScheduleDomainData.DefaultDisplayTime.EndTime).AddMinutes(15));
		}

		[Test]
		public void ShouldMapDefaultDisplayTimeInUsersTimeZone()
		{
			var timeZone = TimeZoneInfoFactory.HawaiiTimeZoneInfo();
			target = new TeamScheduleDomainDataMapper(personProvider, scheduleProvider, _projectionForMtwProvider, new FakeUserTimeZone(timeZone));

			var result = target.Map(DateOnly.Today, Guid.Empty);
			
			var startDateTimeLocal = DateTime.Now.Date.Add(TeamScheduleDomainData.DefaultDisplayTime.StartTime).AddMinutes(-15);
			var expectedStartDateTime = timeZone.SafeConvertTimeToUtc(startDateTimeLocal);
			
			var endDateTimeLocal = DateTime.Now.Date.Add(TeamScheduleDomainData.DefaultDisplayTime.EndTime).AddMinutes(15);
			var expectedEndDateTime = timeZone.SafeConvertTimeToUtc(endDateTimeLocal);

			result.DisplayTimePeriod.StartDateTime.Should().Be.EqualTo(expectedStartDateTime);
			result.DisplayTimePeriod.EndDateTime.Should().Be.EqualTo(expectedEndDateTime);
		}

		[Test]
		public void ShouldMapHasDayOffUnderAbsence()
		{
			var persons = new[] { new Person() };
			var scheduleDay = new StubFactory().ScheduleDayStub(DateTime.Today, persons.Single(),SchedulePartView.ContractDayOff, PersonAssignmentFactory.CreateAssignmentWithDayOff(), null, null);

			personProvider.Stub(x => x.GetPermittedPersonsForGroup(DateOnly.Today, Guid.Empty, DefinedRaptorApplicationFunctionPaths.ViewSchedules)).Return(persons);
			scheduleProvider.Stub(x => x.GetScheduleForPersons(DateOnly.Today, persons)).Return(new[] { scheduleDay });

			var result = target.Map(DateOnly.Today,Guid.Empty);

			result.Days.First().HasDayOffUnder.Should().Be.True();
		}

	}
}