using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
	[TestFixture]
	public class TeamSteadyStateCoherentCheckerTest
	{
		private TeamSteadyStateCoherentChecker _target;
		private MockRepository _mock;
		private IScheduleMatrixPro _scheduleMatrixPro;
		private IScheduleDay _scheduleDay1;
		private IScheduleDay _scheduleDay2;
		private IList<IScheduleMatrixPro> _matrixes;
		private DateOnly _dateOnly;
		private IScheduleDictionary _scheduleDictionary;
		private IPerson _person;
		private IScheduleRange _scheduleRange;
		private IPersonAssignment _personAssignment1;
		private IPersonAssignment _personAssignment2;
		private DateTimePeriod _dateTimePeriod1;
		private DateTimePeriod _dateTimePeriod2;
		private IList<IPerson> _groupMembers;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_scheduleMatrixPro = _mock.StrictMock<IScheduleMatrixPro>();
			_matrixes = new List<IScheduleMatrixPro>{_scheduleMatrixPro};
			_scheduleDay1 = _mock.StrictMock<IScheduleDay>();
			_scheduleDay2 = _mock.StrictMock<IScheduleDay>();
			_dateOnly = new DateOnly(2012, 1, 1);
			_scheduleDictionary = _mock.StrictMock<IScheduleDictionary>();
			_person = _mock.StrictMock<IPerson>();
			_scheduleRange = _mock.StrictMock<IScheduleRange>();
			_personAssignment1 = _mock.StrictMock<IPersonAssignment>();
			_personAssignment2 = _mock.StrictMock<IPersonAssignment>();
			_dateTimePeriod1 = new DateTimePeriod(2012, 1, 1, 2012, 1, 2);
			_dateTimePeriod2 = new DateTimePeriod(2012, 1, 1, 2012, 1, 3);
			_target = new TeamSteadyStateCoherentChecker();
			_groupMembers = new List<IPerson>{_person};
		}

		[Test]
		public void ShouldBeCoherent()
		{
			using(_mock.Record())
			{
				Expect.Call(_scheduleMatrixPro.Person).Return(_person).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDictionary[_person]).Return(_scheduleRange);
				Expect.Call(_scheduleRange.ScheduledDay(_dateOnly)).Return(_scheduleDay1);
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_scheduleDay2.PersonAssignment()).Return(_personAssignment2);
				Expect.Call(_scheduleDay1.PersonAssignment()).Return(_personAssignment1);
				Expect.Call(_personAssignment1.Period).Return(_dateTimePeriod1);
				Expect.Call(_personAssignment2.Period).Return(_dateTimePeriod1);
			}

			using(_mock.Playback())
			{
				var scheduleDay = _target.CheckCoherent(_matrixes, _dateOnly, _scheduleDictionary, _scheduleDay2, _groupMembers);
				Assert.IsNotNull(scheduleDay);
			}
		}

		[Test]
		public void ShouldBeCoherentWhenOnlyOneMainShift()
		{
			using (_mock.Record())
			{
				Expect.Call(_scheduleMatrixPro.Person).Return(_person).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDictionary[_person]).Return(_scheduleRange);
				Expect.Call(_scheduleRange.ScheduledDay(_dateOnly)).Return(_scheduleDay1);
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.None);
			}

			using (_mock.Playback())
			{
				var scheduleDay = _target.CheckCoherent(_matrixes, _dateOnly, _scheduleDictionary, _scheduleDay2, _groupMembers);
				Assert.IsNotNull(scheduleDay);
			}
		}

		[Test]
		public void ShouldNotBeCoherent()
		{
			using (_mock.Record())
			{
				Expect.Call(_scheduleMatrixPro.Person).Return(_person).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDictionary[_person]).Return(_scheduleRange);
				Expect.Call(_scheduleRange.ScheduledDay(_dateOnly)).Return(_scheduleDay1);
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_scheduleDay2.PersonAssignment()).Return(_personAssignment2);
				Expect.Call(_scheduleDay1.PersonAssignment()).Return(_personAssignment1);
				Expect.Call(_personAssignment1.Period).Return(_dateTimePeriod1);
				Expect.Call(_personAssignment2.Period).Return(_dateTimePeriod2);
			}

			using (_mock.Playback())
			{
				var scheduleDay = _target.CheckCoherent(_matrixes, _dateOnly, _scheduleDictionary, _scheduleDay2, _groupMembers);
				Assert.IsNull(scheduleDay);
			}
		}
	}
}
