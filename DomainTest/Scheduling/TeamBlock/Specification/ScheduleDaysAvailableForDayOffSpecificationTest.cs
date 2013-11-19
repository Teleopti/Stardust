using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.Specification
{
	[TestFixture]
	public class ScheduleDaysAvailableForDayOffSpecificationTest
	{
		private MockRepository _mocks;
		private IScheduleDaysAvailableForDayOffSpecification _target;
		private IScheduleDay _part1;
		private IScheduleDay _part2;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_part1 = _mocks.StrictMock<IScheduleDay>();
			_part2 = _mocks.StrictMock<IScheduleDay>();
			_target = new ScheduleDaysAvailableForDayOffSpecification();
		}

		[Test]
		public void ShouldReturnFalseIfScheduleDayIsNull()
		{
			Assert.That(_target.IsSatisfiedBy(null), Is.False);
		}

		[Test]
		public void ShouldReturnTrueIfNoDayOffOrAbsenceOrAssignmentOrMeeting()
		{
			var absenceCollection = new ReadOnlyCollection<IPersonAbsence>(new List<IPersonAbsence>());
			var meetingCollection = new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting>());

			Expect.Call(_part1.PersonAbsenceCollection()).Return(absenceCollection).Repeat.AtLeastOnce();
			Expect.Call(_part1.IsScheduled()).Return(false).Repeat.AtLeastOnce();
			Expect.Call(_part1.PersonMeetingCollection()).Return(meetingCollection).Repeat.AtLeastOnce();

			Expect.Call(_part2.PersonAbsenceCollection()).Return(absenceCollection).Repeat.AtLeastOnce();
			Expect.Call(_part2.IsScheduled()).Return(false).Repeat.AtLeastOnce();
			Expect.Call(_part2.PersonMeetingCollection()).Return(meetingCollection).Repeat.AtLeastOnce();

			_mocks.ReplayAll();
			Assert.That(_target.IsSatisfiedBy(new List<IScheduleDay> { _part1, _part2 }), Is.True);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldReturnFalseIfAnyAbsenceOrAssignmentOrMetting()
		{
			var absenceCollection = new ReadOnlyCollection<IPersonAbsence>(new List<IPersonAbsence>());

			Expect.Call(_part1.PersonAbsenceCollection()).Return(absenceCollection).Repeat.AtLeastOnce();
			Expect.Call(_part1.IsScheduled()).Return(true).Repeat.AtLeastOnce();

			_mocks.ReplayAll();
			Assert.That(_target.IsSatisfiedBy(new List<IScheduleDay> { _part1, _part2 }), Is.False);
			_mocks.VerifyAll();
		}
	}
}
