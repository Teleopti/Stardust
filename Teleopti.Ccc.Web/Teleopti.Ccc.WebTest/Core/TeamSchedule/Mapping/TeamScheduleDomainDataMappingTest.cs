using System;
using System.Linq;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.TeamSchedule.Mapping
{
	[TestFixture]
	public class TeamScheduleDomainDataMappingTest
	{
		private IScheduleProvider scheduleProvider;
		private ITeamScheduleProjectionProvider projectionProvider;
		private ISchedulePersonProvider personProvider;
		private IUserTimeZone userTimeZone;
		private TimeZoneInfo timeZone;

		[SetUp]
		public void Setup()
		{
			scheduleProvider = MockRepository.GenerateMock<IScheduleProvider>();
			personProvider = MockRepository.GenerateMock<ISchedulePersonProvider>();
			projectionProvider = MockRepository.GenerateStub<ITeamScheduleProjectionProvider>();
			userTimeZone = MockRepository.GenerateMock<IUserTimeZone>();

			timeZone = (TimeZoneInfo.Utc);
			userTimeZone = MockRepository.GenerateMock<IUserTimeZone>();
			userTimeZone.Stub(x => x.TimeZone()).Do((Func<TimeZoneInfo>)(() => timeZone));

			Mapper.Reset();
			Mapper.Initialize(c => c.AddProfile(new TeamScheduleDomainDataMappingProfile(
			                                    	() => Mapper.Engine,
			                                    	() => personProvider,
			                                    	() => scheduleProvider,
			                                    	() => projectionProvider,
			                                    	() => userTimeZone
			                                    	)));
		}

		[Test]
		public void ShouldConfigureCorrectly() { Mapper.AssertConfigurationIsValid(); }

		[Test]
		public void ShouldMapDate()
		{
			var result = Mapper.Map<DateOnly, TeamScheduleDomainData>(DateOnly.Today);

			result.Date.Should().Be.EqualTo(DateOnly.Today);
		}

		[Test]
		public void ShouldMapTeamId()
		{
			var id = Guid.NewGuid();

			var result = Mapper.Map<Tuple<DateOnly, Guid>, TeamScheduleDomainData>(new Tuple<DateOnly, Guid>(DateOnly.Today, id));

			result.TeamOrGroupId.Should().Be(id);
		}

		[Test]
		public void ShouldGetPermittedPersonsForTeam()
		{
			var persons = new IPerson[] { };
			var id = Guid.NewGuid();

			personProvider.Stub(x => x.GetPermittedPersonsForGroup(DateOnly.Today, id, DefinedRaptorApplicationFunctionPaths.ViewSchedules)).Return(persons);

			Mapper.Map<Tuple<DateOnly, Guid>, TeamScheduleDomainData>(new Tuple<DateOnly, Guid>(DateOnly.Today, id));

			scheduleProvider.AssertWasCalled(x => x.GetScheduleForPersons(DateOnly.Today, persons));
		}

		[Test]
		public void ShouldMapPerson()
		{
			var person = new Person();

			personProvider.Stub(x => x.GetPermittedPersonsForGroup(DateOnly.Today, Guid.Empty, DefinedRaptorApplicationFunctionPaths.ViewSchedules)).Return(new[] { person });

			var result = Mapper.Map<DateOnly, TeamScheduleDomainData>(DateOnly.Today);

			result.Days.Single().Person.Should().Be(person);
		}

		[Test]
		public void ShouldMapLayers()
		{
			var persons = new[] { new Person() };
			var scheduleDay = new StubFactory().ScheduleDayStub(DateOnly.Today, persons.Single());
			var teamScheduleProjection = new TeamScheduleProjection(new[] { new TeamScheduleLayer() }, DateTime.MaxValue);

			personProvider.Stub(x => x.GetPermittedPersonsForGroup(DateOnly.Today, Guid.Empty, DefinedRaptorApplicationFunctionPaths.ViewSchedules)).Return(persons);
			scheduleProvider.Stub(x => x.GetScheduleForPersons(DateOnly.Today, persons)).Return(new[] { scheduleDay });
			projectionProvider.Stub(x => x.Projection(scheduleDay)).Return(teamScheduleProjection);

			var result = Mapper.Map<DateOnly, TeamScheduleDomainData>(DateOnly.Today);

			result.Days.First().Projection.Should().Be.SameInstanceAs(teamScheduleProjection);
		}

		[Test]
		public void ShouldSortDaysOnProjectionSortDate()
		{
			var stubs = new StubFactory();
			var persons = new[] { new Person(), new Person() };
			var scheduleDays = new[]
			                   	{
			                   		stubs.ScheduleDayStub(DateOnly.MinValue, persons.ElementAt(0)),
			                   		stubs.ScheduleDayStub(DateOnly.MinValue, persons.ElementAt(1))
			                   	};
			var eveningProjection = new TeamScheduleProjection { SortDate = DateTime.Now.Date.AddHours(20) };
			var morningProjection = new TeamScheduleProjection { SortDate = DateTime.Now.Date.AddHours(7) };

			personProvider.Stub(x => x.GetPermittedPersonsForGroup(DateOnly.Today, Guid.Empty, DefinedRaptorApplicationFunctionPaths.ViewSchedules)).Return(persons);
			scheduleProvider.Stub(x => x.GetScheduleForPersons(DateOnly.Today, persons)).Return(scheduleDays);
			projectionProvider.Stub(x => x.Projection(scheduleDays.ElementAt(0))).Return(eveningProjection);
			projectionProvider.Stub(x => x.Projection(scheduleDays.ElementAt(1))).Return(morningProjection);

			var result = Mapper.Map<DateOnly, TeamScheduleDomainData>(DateOnly.Today);

			result.Days.Select(d => d.Projection)
				.Should().Have.SameSequenceAs(new[] {morningProjection, eveningProjection});
		}

		[Test]
		public void ShouldMapDisplayTimeToDefaultIfNull()
		{
			var result = Mapper.Map<DateOnly, TeamScheduleDomainData>(DateOnly.Today);

			result.DisplayTimePeriod.StartDateTime.Should().Be.EqualTo(DateTime.Now.Date.Add(TeamScheduleDomainData.DefaultDisplayTime.StartTime));
			result.DisplayTimePeriod.EndDateTime.Should().Be.EqualTo(DateTime.Now.Date.Add(TeamScheduleDomainData.DefaultDisplayTime.EndTime));
		}

		[Test]
		public void ShouldMapDefaultDisplayTimeInUsersTimeZone()
		{
			timeZone = TimeZoneInfoFactory.HawaiiTimeZoneInfo();

			var result = Mapper.Map<DateOnly, TeamScheduleDomainData>(DateOnly.Today);

			var startDateTimeLocal = DateTime.Now.Date.Add(TeamScheduleDomainData.DefaultDisplayTime.StartTime);
			var expectedStartDateTime = timeZone.SafeConvertTimeToUtc(startDateTimeLocal);

			var endDateTimeLocal = DateTime.Now.Date.Add(TeamScheduleDomainData.DefaultDisplayTime.EndTime);
			var expectedEndDateTime = timeZone.SafeConvertTimeToUtc(endDateTimeLocal);

			result.DisplayTimePeriod.StartDateTime.Should().Be.EqualTo(expectedStartDateTime);
			result.DisplayTimePeriod.EndDateTime.Should().Be.EqualTo(expectedEndDateTime);
		}

		[Test]
		public void ShouldMapHasDayOffUnderAbsence()
		{
			var persons = new[] { new Person() };
			var scheduleDay = new StubFactory().ScheduleDayStub(DateOnly.Today, persons.Single(),SchedulePartView.ContractDayOff, PersonAssignmentFactory.CreateAssignmentWithDayOff(), null, null);

			personProvider.Stub(x => x.GetPermittedPersonsForGroup(DateOnly.Today, Guid.Empty, DefinedRaptorApplicationFunctionPaths.ViewSchedules)).Return(persons);
			scheduleProvider.Stub(x => x.GetScheduleForPersons(DateOnly.Today, persons)).Return(new[] { scheduleDay });

			var result = Mapper.Map<DateOnly, TeamScheduleDomainData>(DateOnly.Today);

			result.Days.First().HasDayOffUnder.Should().Be.True();
		}

	}
}