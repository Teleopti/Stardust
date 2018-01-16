using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.AgentInfo.Requests
{
	[DomainTest]
	public class OvertimeRequestDefaultStartTimeProviderTest : ISetup
	{
		public IOvertimeRequestDefaultStartTimeProvider OvertimeRequestDefaultStartTimeProvider;
		public ICurrentScenario CurrentScenario;
		public FakePersonAssignmentRepository FakeAssignmentRepository;
		public FakeActivityRepository FakeActivityRepository;
		public FakePersonAbsenceRepository PersonAbsenceRepository;
		public FakeLoggedOnUser FakeLoggedOnUser;
		public MutableNow Now;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			system.UseTestDouble(new FakeScenarioRepository(new Scenario("default") { DefaultScenario = true }))
				.For<IScenarioRepository>();
			system.UseTestDouble(new MutableNow(new DateTime(2018, 01, 08, 10, 00, 00, DateTimeKind.Utc))).For<INow>();
		}

		[Test]
		public void ShouldGetDefaultStartTime()
		{
			var date = new DateOnly(2018, 1, 8);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			FakeLoggedOnUser.SetFakeLoggedOnUser(agent);
			var phone = ActivityFactory.CreateActivity("phone activity");
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(agent, CurrentScenario.Current(), date);
			personAssignment.AddActivity(phone, new DateTimePeriod(2018, 1, 8, 08, 2018, 1, 8, 17));
			FakeAssignmentRepository.Has(personAssignment);

			var result = OvertimeRequestDefaultStartTimeProvider.GetDefaultStartTime(date);

			result.Date.Should().Be(date.Date);
			result.Hour.Should().Be(17);
			result.Minute.Should().Be(0);
		}

		[Test]
		public void ShouldGetRoundUpHourAsDefaultStartTimeWhenCurrentTimeIsLaterThanShiftEndTime()
		{
			Now.Is(new DateTime(2018, 1, 8, 18, 09, 00, DateTimeKind.Utc));
			var date = new DateOnly(2018, 1, 8);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			FakeLoggedOnUser.SetFakeLoggedOnUser(agent);
			var phone = ActivityFactory.CreateActivity("phone activity");
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(agent, CurrentScenario.Current(), date);
			personAssignment.AddActivity(phone, new DateTimePeriod(2018, 1, 8, 08, 2018, 1, 8, 17));
			FakeAssignmentRepository.Has(personAssignment);

			var result = OvertimeRequestDefaultStartTimeProvider.GetDefaultStartTime(date);

			result.Date.Should().Be(date.Date);
			result.Hour.Should().Be(18);
			result.Minute.Should().Be(30);
		}

		[Test]
		public void ShouldGetRoundUpHourOnTomorrowAsDefaultStartTimeWhenCurrentTimeIsLaterThanShiftEndTime()
		{
			Now.Is(new DateTime(2018, 1, 8, 23, 49, 00, DateTimeKind.Utc));
			var date = new DateOnly(2018, 1, 8);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			FakeLoggedOnUser.SetFakeLoggedOnUser(agent);
			var phone = ActivityFactory.CreateActivity("phone activity");
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(agent, CurrentScenario.Current(), date);
			personAssignment.AddActivity(phone, new DateTimePeriod(2018, 1, 8, 08, 2018, 1, 8, 17));
			FakeAssignmentRepository.Has(personAssignment);

			var result = OvertimeRequestDefaultStartTimeProvider.GetDefaultStartTime(date);

			result.Date.Should().Be(date.Date.AddDays(1));
			result.Hour.Should().Be(0);
			result.Minute.Should().Be(30);
		}

		[Test]
		public void ShouldGetShiftEndTimeAsDefaultStartTimeWhenCurrentTimeIsEarlierThanShiftStartTime()
		{
			Now.Is(new DateTime(2018, 1, 8, 6, 09, 00, DateTimeKind.Utc));
			var date = new DateOnly(2018, 1, 8);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			FakeLoggedOnUser.SetFakeLoggedOnUser(agent);
			var phone = ActivityFactory.CreateActivity("phone activity");
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(agent, CurrentScenario.Current(), date);
			personAssignment.AddActivity(phone, new DateTimePeriod(2018, 1, 8, 08, 2018, 1, 8, 17));
			FakeAssignmentRepository.Has(personAssignment);

			var result = OvertimeRequestDefaultStartTimeProvider.GetDefaultStartTime(date);

			result.Date.Should().Be(date.Date);
			result.Hour.Should().Be(17);
			result.Minute.Should().Be(0);
		}

		[Test]
		public void ShouldGetYesterdayOvernightShiftEndTimeAsDefaultStartTimeWhenCurrentTimeIsEarlierThanThatTime()
		{
			Now.Is(new DateTime(2018, 1, 8, 2, 09, 00, DateTimeKind.Utc));
			var date = new DateOnly(2018, 1, 8);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			FakeLoggedOnUser.SetFakeLoggedOnUser(agent);
			var phone = ActivityFactory.CreateActivity("phone activity");
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(agent, CurrentScenario.Current(), date.AddDays(-1));
			personAssignment.AddActivity(phone, new DateTimePeriod(2018, 1, 7, 08, 2018, 1, 8, 3));
			FakeAssignmentRepository.Has(personAssignment);

			var result = OvertimeRequestDefaultStartTimeProvider.GetDefaultStartTime(date);

			result.Date.Should().Be(date.Date);
			result.Hour.Should().Be(3);
			result.Minute.Should().Be(0);
		}

		[Test]
		public void ShouldGetTodayOvernightShiftEndTimeAsDefaultStartTime()
		{
			Now.Is(new DateTime(2018, 1, 8, 10, 00, 00, DateTimeKind.Utc));
			var date = new DateOnly(2018, 1, 8);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			FakeLoggedOnUser.SetFakeLoggedOnUser(agent);
			var phone = ActivityFactory.CreateActivity("phone activity");
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(agent, CurrentScenario.Current(), date);
			personAssignment.AddActivity(phone, new DateTimePeriod(2018, 1, 8, 17, 2018, 1, 9, 03));
			FakeAssignmentRepository.Has(personAssignment);

			var result = OvertimeRequestDefaultStartTimeProvider.GetDefaultStartTime(date);

			result.Date.Should().Be(date.AddDays(1).Date);
			result.Hour.Should().Be(3);
			result.Minute.Should().Be(0);
		}

		[Test]
		public void ShouldGetSelectedDayOvernightShiftEndTimeAsDefaultStartTime()
		{
			Now.Is(new DateTime(2018, 1, 8, 10, 00, 00, DateTimeKind.Utc));
			var date = new DateOnly(2018, 1, 9);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			FakeLoggedOnUser.SetFakeLoggedOnUser(agent);
			var phone = ActivityFactory.CreateActivity("phone activity");
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(agent, CurrentScenario.Current(), date);
			personAssignment.AddActivity(phone, new DateTimePeriod(2018, 1, 9, 17, 2018, 1, 10, 03));
			FakeAssignmentRepository.Has(personAssignment);

			var result = OvertimeRequestDefaultStartTimeProvider.GetDefaultStartTime(date);

			result.Date.Should().Be(date.AddDays(1).Date);
			result.Hour.Should().Be(3);
			result.Minute.Should().Be(0);
		}

		[Test]
		public void ShouldGetRoundUpHalfHourAsDefaultStartTimeWhenCurrentTimeIsEarlierThanShiftEndTimePlus15Min()
		{
			Now.Is(new DateTime(2018, 1, 8, 17, 49, 00, DateTimeKind.Utc));
			var date = new DateOnly(2018, 1, 8);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			FakeLoggedOnUser.SetFakeLoggedOnUser(agent);
			var phone = ActivityFactory.CreateActivity("phone activity");
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(agent, CurrentScenario.Current(), date);
			personAssignment.AddActivity(phone, new DateTimePeriod(2018, 1, 8, 08, 2018, 1, 8, 17));
			FakeAssignmentRepository.Has(personAssignment);

			var result = OvertimeRequestDefaultStartTimeProvider.GetDefaultStartTime(date);

			result.Date.Should().Be(date.Date);
			result.Hour.Should().Be(18);
			result.Minute.Should().Be(30);
		}

		[Test]
		public void ShouldGetCorrectDefaultStartTimeConsideringShiftEndTimeAndCurrentTimeAndThe15MinGap()
		{
			Now.Is(new DateTime(2018, 1, 8, 16, 49, 00, DateTimeKind.Utc));
			var date = new DateOnly(2018, 1, 8);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			FakeLoggedOnUser.SetFakeLoggedOnUser(agent);

			var phone = ActivityFactory.CreateActivity("phone activity");
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(agent, CurrentScenario.Current(), date);
			personAssignment.AddActivity(phone, new DateTimePeriod(2018, 1, 8, 08, 2018, 1, 8, 17));
			FakeAssignmentRepository.Has(personAssignment);

			var result = OvertimeRequestDefaultStartTimeProvider.GetDefaultStartTime(date);

			result.Date.Should().Be(date.Date);
			result.Hour.Should().Be(17);
			result.Minute.Should().Be(30);
		}

		[Test]
		public void ShouldGetCorrectDefaultStartTimeConsideringWorkDefaultStartTimeAndCurrentTimeAndThe15MinGap()
		{
			Now.Is(new DateTime(2018, 1, 8, 07, 49, 00, DateTimeKind.Utc));
			var date = new DateOnly(2018, 1, 8);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			FakeLoggedOnUser.SetFakeLoggedOnUser(agent);

			var result = OvertimeRequestDefaultStartTimeProvider.GetDefaultStartTime(date);

			result.Date.Should().Be(date.Date);
			result.Hour.Should().Be(8);
			result.Minute.Should().Be(30);
		}

		[Test]
		public void ShouldGetWorkDefaultStartTimeAsDefaultStartTimeWhenThereIsNoOvernightShiftOrNormalShiftOnSelectedDay()
		{
			Now.Is(new DateTime(2018, 1, 8, 10, 00, 00, DateTimeKind.Utc));
			var date = new DateOnly(2018, 1, 8);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			FakeLoggedOnUser.SetFakeLoggedOnUser(agent);

			var phone = ActivityFactory.CreateActivity("phone activity");
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(agent, CurrentScenario.Current(), date);
			personAssignment.AddActivity(phone, new DateTimePeriod(2018, 1, 8, 08, 2018, 1, 8, 17));
			FakeAssignmentRepository.Has(personAssignment);

			var result = OvertimeRequestDefaultStartTimeProvider.GetDefaultStartTime(date.AddDays(1));

			result.Date.Should().Be(date.AddDays(1).Date);
			result.Hour.Should().Be(8);
			result.Minute.Should().Be(0);
		}

		[Test]
		public void ShouldGetWorkDefaultStartTimeAsDefaultStartTimeWhenThereIsNoShiftOnSelectedDayAndTheDayBefore()
		{
			Now.Is(new DateTime(2018, 1, 8, 10, 00, 00, DateTimeKind.Utc));
			var date = new DateOnly(2018, 1, 9);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			FakeLoggedOnUser.SetFakeLoggedOnUser(agent);

			var result = OvertimeRequestDefaultStartTimeProvider.GetDefaultStartTime(date.AddDays(1));

			result.Date.Should().Be(date.AddDays(1).Date);
			result.Hour.Should().Be(8);
			result.Minute.Should().Be(0);
		}

		[Test]
		public void ShouldGetDefaultStartTimeInUserTimezone()
		{
			Now.Is(new DateTime(2018, 1, 8, 10, 00, 00, DateTimeKind.Utc));
			var date = new DateOnly(2018, 1, 10);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			agent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));

			FakeLoggedOnUser.SetFakeLoggedOnUser(agent);
			var phone = ActivityFactory.CreateActivity("phone activity");
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(agent, CurrentScenario.Current(), date);
			personAssignment.AddActivity(phone, new DateTimePeriod(2018, 1, 10, 08, 2018, 1, 10, 17));
			FakeAssignmentRepository.Has(personAssignment);

			var result = OvertimeRequestDefaultStartTimeProvider.GetDefaultStartTime(date);

			result.Date.Should().Be(date.Date);
			result.Hour.Should().Be(18);
			result.Minute.Should().Be(0);
		}
	}
}
