using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
{
    [TestFixture]
    public class NewOverlappingAssignmentRuleTest
    {
        private NewOverlappingAssignmentRule _target;
        private MockRepository _mocks;
        private IPerson _person;
	    private IScheduleRange _range;
	    private IDictionary<IPerson, IScheduleRange> _dic;
	    private IDateOnlyAsDateTimePeriod _dateOnlyAsDateTimePeriod;
	    private IScheduleDay _scheduleDayToCheck;
	    private IPersonAssignment _personAssignmentToCheck;
	    private DateOnly _dateToCheck;
	    private DateTimePeriod _periodToCheck;
	    private IList<IPersonAssignment> _personAssignmentConflictCollection;
	    private IList<IScheduleDay> _days;

		[SetUp]
	    public void Setup()
	    {
		    _mocks = new MockRepository();
		    _target = new NewOverlappingAssignmentRule();
		    _person = PersonFactory.CreatePerson();
		    _range = _mocks.StrictMock<IScheduleRange>();
		    _dic = new Dictionary<IPerson, IScheduleRange> {{_person, _range}};
		    _dateOnlyAsDateTimePeriod = _mocks.StrictMock<IDateOnlyAsDateTimePeriod>();
		    _scheduleDayToCheck = _mocks.StrictMock<IScheduleDay>();
		    _personAssignmentToCheck = _mocks.StrictMock<IPersonAssignment>();
		    _dateToCheck = new DateOnly(2013, 9, 27);
		    _periodToCheck = new DateTimePeriod(2013, 9, 27, 2013, 9, 28);
		    _personAssignmentConflictCollection = new List<IPersonAssignment>();
			_days = new List<IScheduleDay> { _scheduleDayToCheck };
	    }

	    [Test]
        public void CanAccessSimpleProperties()
        {
            Assert.IsNotNull(_target);
            Assert.IsTrue(_target.IsMandatory);
            Assert.IsTrue(_target.HaltModify);
            // ska man inte kunna ändra
            _target.HaltModify = false;
            Assert.IsTrue(_target.HaltModify);
            Assert.AreEqual("", _target.ErrorMessage);
        }

		[Test]
		public void ShouldCheckForConflictsInTheDateOfTheChangedScheduleDay()
		{
			_personAssignmentConflictCollection.Add(_personAssignmentToCheck);
			using (_mocks.Record())
			{
				commonMocks();
				Expect.Call(_scheduleDayToCheck.PersonAssignmentConflictCollection).Return(_personAssignmentConflictCollection);
				Expect.Call(_personAssignmentToCheck.Period).Return(_periodToCheck);
				Expect.Call(_range.ScheduledDay(_dateToCheck.AddDays(-1))).Return(_scheduleDayToCheck);
				Expect.Call(_scheduleDayToCheck.PersonAssignmentCollection())
				      .Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment>()));
				Expect.Call(_range.ScheduledDay(_dateToCheck.AddDays(1))).Return(_scheduleDayToCheck);
				Expect.Call(_scheduleDayToCheck.PersonAssignmentCollection())
					  .Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment>()));
			}

			using (_mocks.Playback())
			{
				var ret = _target.Validate(_dic, _days);
				Assert.AreEqual(1, ret.Count());
			}

		}

		[Test]
		public void ShouldCheckForConflictsInTheDateBeforeTheChangedScheduleDay()
		{
			_personAssignmentConflictCollection.Add(_personAssignmentToCheck);
			using (_mocks.Record())
			{
				commonMocks();
				Expect.Call(_scheduleDayToCheck.PersonAssignmentConflictCollection).Return(_personAssignmentConflictCollection);
				Expect.Call(_personAssignmentToCheck.Period).Return(_periodToCheck);
				Expect.Call(_range.ScheduledDay(_dateToCheck.AddDays(-1))).Return(_scheduleDayToCheck);
				Expect.Call(_scheduleDayToCheck.PersonAssignmentCollection())
				      .Return(
					      new ReadOnlyCollection<IPersonAssignment>(
						      new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> {_personAssignmentToCheck})));
				Expect.Call(_personAssignmentToCheck.Period).Return(_periodToCheck);
				Expect.Call(_range.ScheduledDay(_dateToCheck.AddDays(1))).Return(_scheduleDayToCheck);
				Expect.Call(_scheduleDayToCheck.PersonAssignmentCollection())
					  .Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment>()));
			}

			using (_mocks.Playback())
			{
				var ret = _target.Validate(_dic, _days);
				Assert.AreEqual(2, ret.Count());
			}

		}

		[Test]
		public void ShouldCheckForConflictsInTheDateAfterTheChangedScheduleDay()
		{
			_personAssignmentConflictCollection.Add(_personAssignmentToCheck);
			using (_mocks.Record())
			{
				commonMocks();
				Expect.Call(_scheduleDayToCheck.PersonAssignmentConflictCollection).Return(_personAssignmentConflictCollection);
				Expect.Call(_personAssignmentToCheck.Period).Return(_periodToCheck);
				Expect.Call(_range.ScheduledDay(_dateToCheck.AddDays(-1))).Return(_scheduleDayToCheck);
				Expect.Call(_scheduleDayToCheck.PersonAssignmentCollection())
					  .Return(
						  new ReadOnlyCollection<IPersonAssignment>(
							  new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> { _personAssignmentToCheck })));
				Expect.Call(_personAssignmentToCheck.Period).Return(_periodToCheck);
				Expect.Call(_range.ScheduledDay(_dateToCheck.AddDays(1))).Return(_scheduleDayToCheck);
				Expect.Call(_scheduleDayToCheck.PersonAssignmentCollection())
					  .Return(
						  new ReadOnlyCollection<IPersonAssignment>(
							  new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> { _personAssignmentToCheck })));
				Expect.Call(_personAssignmentToCheck.Period).Return(_periodToCheck);
			}

			using (_mocks.Playback())
			{
				var ret = _target.Validate(_dic, _days);
				Assert.AreEqual(3, ret.Count());
			}

		}

		private void commonMocks()
		{
			Expect.Call(_scheduleDayToCheck.Person).Return(_person);
			Expect.Call(_scheduleDayToCheck.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
			Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(_dateToCheck);
			Expect.Call(_scheduleDayToCheck.PersonAssignmentCollection())
			      .Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> {_personAssignmentToCheck}));
			Expect.Call(_personAssignmentToCheck.Period).Return(_periodToCheck);
		}
    }
}
