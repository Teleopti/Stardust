using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[TestFixture]
	public class ScheduleDayAvailableForDayOffSpecificationTest
	{
		private MockRepository _mocks;
		private ScheduleDayAvailableForDayOffSpecification _target;
		private IScheduleDay _part;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_part = _mocks.StrictMock<IScheduleDay>();
			_target = new ScheduleDayAvailableForDayOffSpecification();
		}

		[Test]
		public void ShouldReturnFalseIfScheduleDayIsNull()
		{
			Assert.That(_target.IsSatisfiedBy(null), Is.False);
		}

		[Test]
		public void ShouldReturnTrueIfNoDayOffOrAbsenceOrMainShiftOrMeeting()
		{
			var meetingCollection = new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting>());

			Expect.Call(_part.IsScheduled()).Return(false).Repeat.AtLeastOnce();
			Expect.Call(_part.PersonMeetingCollection()).Return(meetingCollection).Repeat.AtLeastOnce();

			_mocks.ReplayAll();
			Assert.That(_target.IsSatisfiedBy(_part), Is.True);
			_mocks.VerifyAll();
		}
	}

}