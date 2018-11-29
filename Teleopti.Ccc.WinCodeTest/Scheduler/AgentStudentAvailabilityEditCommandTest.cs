using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture]
	public class AgentStudentAvailabilityEditCommandTest
	{
		private AgentStudentAvailabilityEditCommand _target;

		private IScheduleDay _scheduleDay;
		private TimeSpan _startTime;
		private TimeSpan _endTime;
		private IStudentAvailabilityDay _studentAvailabilityDay;
		private IAgentStudentAvailabilityDayCreator _studentAvailabilityDayCreator;
	    private IScheduleDictionary _scheduleDictionary;

	    [SetUp]
		public void Setup()
		{
			_studentAvailabilityDayCreator = MockRepository.GenerateMock<IAgentStudentAvailabilityDayCreator>();
			_scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
			_scheduleDictionary = MockRepository.GenerateMock<IScheduleDictionary>();
			_startTime = TimeSpan.FromHours(8);
			_endTime = TimeSpan.FromHours(10);
			_target = new AgentStudentAvailabilityEditCommand(_scheduleDay, _startTime, _endTime, _studentAvailabilityDayCreator,_scheduleDictionary, new DoNothingScheduleDayChangeCallBack());
			_studentAvailabilityDay = new StudentAvailabilityDay(PersonFactory.CreatePerson(), new DateOnly(2001,1,1), new List<IStudentAvailabilityRestriction>());
		}

		[Test]
		public void ShouldEdit()
		{
			bool startTimeError;
			bool endTimeError;
			_scheduleDay.Stub(x => x.PersistableScheduleDataCollection())
				.Return(
					new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData> {_studentAvailabilityDay}));
			_studentAvailabilityDayCreator.Stub(x => x.CanCreate(_startTime, _endTime, out startTimeError, out endTimeError)).Return(true);
			_studentAvailabilityDayCreator.Stub(x => x.Create(_scheduleDay, _startTime, _endTime)).Return(_studentAvailabilityDay);

			_target.Execute();

			_studentAvailabilityDay.RestrictionCollection[0].StartTimeLimitation.StartTime.Should()
				.Be.EqualTo(TimeSpan.FromHours(8));
			_studentAvailabilityDay.RestrictionCollection[0].EndTimeLimitation.EndTime.Should()
				.Be.EqualTo(TimeSpan.FromHours(10));
		}

		[Test]
		public void ShouldNotEditWhenCannotCreateDay()
		{
			bool startTimeError;
			bool endTimeError;
			_scheduleDay.Stub(x => x.PersistableScheduleDataCollection())
				.Return(
					new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData> {_studentAvailabilityDay}));
			_studentAvailabilityDayCreator.Stub(x => x.CanCreate(_startTime, _endTime, out startTimeError, out endTimeError))
				.Return(false);

			_target.Execute();

			_studentAvailabilityDay.RestrictionCollection.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotEditWhenNoDay()
		{
			_scheduleDay.Stub(x => x.PersistableScheduleDataCollection())
				.Return(new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData>()));

			_target.Execute();

			_studentAvailabilityDay.RestrictionCollection.Should().Be.Empty();
		}
	}
}
