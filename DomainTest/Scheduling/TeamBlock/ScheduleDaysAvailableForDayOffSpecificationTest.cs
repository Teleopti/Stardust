using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
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
			var dayOffCollection = new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff>());
			var absenceCollection = new ReadOnlyCollection<IPersonAbsence>(new List<IPersonAbsence>());
			var assCollection = new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment>());
			var meetingCollection = new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting>());

			Expect.Call(_part1.PersonDayOffCollection()).Return(dayOffCollection).Repeat.AtLeastOnce();
			Expect.Call(_part1.PersonAbsenceCollection()).Return(absenceCollection).Repeat.AtLeastOnce();
			Expect.Call(_part1.PersonAssignmentCollection()).Return(assCollection).Repeat.AtLeastOnce();
			Expect.Call(_part1.PersonMeetingCollection()).Return(meetingCollection).Repeat.AtLeastOnce();

			Expect.Call(_part2.PersonDayOffCollection()).Return(dayOffCollection).Repeat.AtLeastOnce();
			Expect.Call(_part2.PersonAbsenceCollection()).Return(absenceCollection).Repeat.AtLeastOnce();
			Expect.Call(_part2.PersonAssignmentCollection()).Return(assCollection).Repeat.AtLeastOnce();
			Expect.Call(_part2.PersonMeetingCollection()).Return(meetingCollection).Repeat.AtLeastOnce();

			_mocks.ReplayAll();
			Assert.That(_target.IsSatisfiedBy(new List<IScheduleDay> { _part1, _part2 }), Is.True);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldReturnFalseIfAnyAbsenceOrAssignmentOrMetting()
		{
			var absenceCollection = new ReadOnlyCollection<IPersonAbsence>(new List<IPersonAbsence>());
			var assCollection =
				new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment>
					{
						new PersonAssignment(PersonFactory.CreatePerson("bill"),ScenarioFactory.CreateScenarioAggregate(), new DateOnly())
					});

			Expect.Call(_part1.PersonAbsenceCollection()).Return(absenceCollection).Repeat.AtLeastOnce();
			Expect.Call(_part1.PersonAssignmentCollection()).Return(assCollection).Repeat.AtLeastOnce();

			_mocks.ReplayAll();
			Assert.That(_target.IsSatisfiedBy(new List<IScheduleDay> { _part1, _part2 }), Is.False);
			_mocks.VerifyAll();
		}
	}
}
