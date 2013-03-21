using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftFilters
{
	[TestFixture]
	public class NotOverWritableActivitiesShiftFilterTest
	{
		private MockRepository _mocks;
		private DateOnly _dateOnly;
		private ISchedulingResultStateHolder _resultStateHolder;
		private INotOverWritableActivitiesShiftFilter _target;
		private IPersonAssignment _personAssignment;
		private List<IPersonAssignment> _personAssignments;
		private IScheduleRange _scheduleRange;
		private IScheduleDictionary _scheduleDictionary;
		private IScheduleDay _part;
		private WorkShiftFinderResult _finderResult;
		private IPerson _person;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_dateOnly = new DateOnly(2013, 3, 1);
			_resultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
			_personAssignment = _mocks.StrictMock<IPersonAssignment>();
			_personAssignments = new List<IPersonAssignment>();
			_scheduleRange = _mocks.StrictMock<IScheduleRange>();
			_scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			_part = _mocks.StrictMock<IScheduleDay>();
			_person = PersonFactory.CreatePerson("Bill");
			_finderResult = new WorkShiftFinderResult(_person, new DateOnly(2009, 2, 3));
			_target = new NotOverWritableActivitiesShiftFilter(_resultStateHolder);
		}

		[Test]
		public void CanFilterOutShiftsWhichCannotBeOverwritten()
		{
			_personAssignment = _mocks.StrictMock<IPersonAssignment>();
			_personAssignments = new List<IPersonAssignment> { _personAssignment };
			var personalShift = _mocks.StrictMock<IPersonalShift>();
			var currentDate = new DateTime(2013, 3, 1, 0, 0, 0, DateTimeKind.Utc);
			var lunch = ActivityFactory.CreateActivity("lunch");
			lunch.AllowOverwrite = false;
			IList<IShiftProjectionCache> shifts = new List<IShiftProjectionCache>();
			var readOnlymeetings = new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting>());
			var c1 = _mocks.StrictMock<IShiftProjectionCache>();
			shifts.Add(c1);
			Expect.Call(c1.MainShiftProjection).Return(new VisualLayerCollection(null, getLunchLayer(currentDate, lunch), new ProjectionPayloadMerger())).Repeat.AtLeastOnce();
			Expect.Call(_part.PersonAssignmentCollection()).Return(new ReadOnlyCollection<IPersonAssignment>(_personAssignments)).Repeat.AtLeastOnce();
			Expect.Call(_part.PersonMeetingCollection()).Return(readOnlymeetings).Repeat.AtLeastOnce();
			Expect.Call(_personAssignment.PersonalShiftCollection).Return(new ReadOnlyCollection<IPersonalShift>(new List<IPersonalShift> { personalShift })).Repeat.AtLeastOnce();
			Expect.Call(personalShift.LayerCollection).Return(getPersonalLayers(currentDate)).Repeat.AtLeastOnce();
			Expect.Call(_resultStateHolder.Schedules).Return(_scheduleDictionary);
			Expect.Call(_scheduleDictionary[_person]).Return(_scheduleRange);
			Expect.Call(_scheduleRange.ScheduledDay(_dateOnly)).Return(_part);
			_mocks.ReplayAll();
			var retShifts = _target.Filter(_dateOnly, _person, shifts, _finderResult);
			retShifts.Count.Should().Be.EqualTo(0);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldNotFilterIfNoMeetingOrPersonAssignment()
		{
			_personAssignment = _mocks.StrictMock<IPersonAssignment>();
			_personAssignments = new List<IPersonAssignment>();

			var lunch = ActivityFactory.CreateActivity("lunch");
			lunch.AllowOverwrite = false;
			IList<IShiftProjectionCache> shifts = new List<IShiftProjectionCache>();
			var readOnlymeetings = new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting>());
			var c1 = _mocks.StrictMock<IShiftProjectionCache>();
			shifts.Add(c1);
			Expect.Call(_resultStateHolder.Schedules).Return(_scheduleDictionary);
			Expect.Call(_scheduleDictionary[_person]).Return(_scheduleRange);
			Expect.Call(_scheduleRange.ScheduledDay(_dateOnly)).Return(_part);
			Expect.Call(_part.PersonAssignmentCollection()).Return(new ReadOnlyCollection<IPersonAssignment>(_personAssignments)).Repeat.AtLeastOnce();
			Expect.Call(_part.PersonMeetingCollection()).Return(readOnlymeetings).Repeat.AtLeastOnce();

			_mocks.ReplayAll();
			var retShifts = _target.Filter(_dateOnly, _person, shifts, _finderResult);
			retShifts.Count.Should().Be.EqualTo(1);
			_mocks.VerifyAll();
		}

		[Test]
		public void VerifyIfPersonalShiftCannotOverrideActivity()
		{
			_personAssignment = _mocks.StrictMock<IPersonAssignment>();
			_personAssignments = new List<IPersonAssignment> { _personAssignment };

			var currentDate = new DateTime(2013, 3, 1, 0, 0, 0, DateTimeKind.Utc);
			var lunch = ActivityFactory.CreateActivity("lunch");
			lunch.AllowOverwrite = true;
			var lunchLayer = new List<IVisualLayer>
                                 {
                                     new VisualLayer(lunch, new DateTimePeriod(currentDate.AddHours(11), currentDate.AddHours(12)),
                                                     lunch, null)
                                 };
			var layerCollection1 = new VisualLayerCollection(null, lunchLayer, new ProjectionPayloadMerger());

			IList<IShiftProjectionCache> shifts = new List<IShiftProjectionCache>();
			var meeting = _mocks.StrictMock<IPersonMeeting>();
			var readOnlymeetings = new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting> { meeting });
			var c1 = _mocks.StrictMock<IShiftProjectionCache>();
			shifts.Add(c1);
			Expect.Call(c1.MainShiftProjection).Return(layerCollection1).Repeat.AtLeastOnce();
			Expect.Call(_resultStateHolder.Schedules).Return(_scheduleDictionary);
			Expect.Call(_scheduleDictionary[_person]).Return(_scheduleRange);
			Expect.Call(_scheduleRange.ScheduledDay(_dateOnly)).Return(_part);
			Expect.Call(_part.PersonAssignmentCollection()).Return(new ReadOnlyCollection<IPersonAssignment>(_personAssignments)).Repeat.AtLeastOnce();
			Expect.Call(_part.PersonMeetingCollection()).Return(readOnlymeetings).Repeat.AtLeastOnce();
			_mocks.ReplayAll();
			var retShifts = _target.Filter(_dateOnly, _person, shifts, _finderResult);
			retShifts.Count.Should().Be.EqualTo(1);
			_mocks.VerifyAll();
		}

		private static List<IVisualLayer> getLunchLayer(DateTime currentDate, Activity lunch)
		{
			var lunchLayer = new List<IVisualLayer>
                                 {
                                     new VisualLayer(lunch,
                                                     new DateTimePeriod(currentDate.AddHours(11), currentDate.AddHours(12)),
                                                     lunch, null)
                                 };
			return lunchLayer;
		}

		private static LayerCollection<IActivity> getPersonalLayers(DateTime currentDate)
		{
			var personalLayers = new LayerCollection<IActivity>
                                     {
                                         new PersonalShiftActivityLayer(ActivityFactory.CreateActivity("personal"),
                                                                        new DateTimePeriod(currentDate.AddHours(10),
                                                                                           currentDate.AddHours(13)))
                                     };
			return personalLayers;
		}
	}
}
