using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.ResourceCalculation;
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
		private DateOnly _dateOnly = new DateOnly(2013, 3, 1);
		private INotOverWritableActivitiesShiftFilter _target;
		private IPersonAssignment _personAssignment;
		private IScheduleDay _part;
		private WorkShiftFinderResult _finderResult;
		private IPerson _person;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_personAssignment = _mocks.StrictMock<IPersonAssignment>();
			_part = _mocks.StrictMock<IScheduleDay>();
			_person = PersonFactory.CreatePerson("Bill");
			_finderResult = new WorkShiftFinderResult(_person, new DateOnly(2009, 2, 3));
			_target = new NotOverWritableActivitiesShiftFilter(()=> new FakeScheduleDayForPerson(_part));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void CanFilterOutShiftsWhichCannotBeOverwritten()
		{
			_personAssignment = _mocks.StrictMock<IPersonAssignment>();
			var currentDate = new DateTime(2013, 3, 1, 0, 0, 0, DateTimeKind.Utc);
			var lunch = ActivityFactory.CreateActivity("lunch");
			lunch.AllowOverwrite = false;
			IList<IShiftProjectionCache> shifts = new List<IShiftProjectionCache>();
			var readOnlymeetings = new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting>());
			var c1 = _mocks.StrictMock<IShiftProjectionCache>();
			shifts.Add(c1);
			Expect.Call(c1.MainShiftProjection).Return(new VisualLayerCollection(null, getLunchLayer(currentDate, lunch), new ProjectionPayloadMerger())).Repeat.AtLeastOnce();
			Expect.Call(_part.PersonAssignment(true)).Return(_personAssignment).Repeat.AtLeastOnce();
			Expect.Call(_part.PersonMeetingCollection()).Return(readOnlymeetings).Repeat.AtLeastOnce();
			Expect.Call(_personAssignment.PersonalActivities()).Return(getPersonalLayers(currentDate)).Repeat.AtLeastOnce();
			_mocks.ReplayAll();
			var retShifts = _target.Filter(_dateOnly, _person, shifts, _finderResult);
			retShifts.Count.Should().Be.EqualTo(0);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldNotFilterIfNoMeeting()
		{

			var lunch = ActivityFactory.CreateActivity("lunch");
			lunch.AllowOverwrite = false;
			IList<IShiftProjectionCache> shifts = new List<IShiftProjectionCache>();
			var readOnlymeetings = new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting>());
			var c1 = _mocks.StrictMock<IShiftProjectionCache>();
			shifts.Add(c1);
			Expect.Call(_part.PersonAssignment(true)).Return(_personAssignment).Repeat.AtLeastOnce();
			Expect.Call(_part.PersonMeetingCollection()).Return(readOnlymeetings).Repeat.AtLeastOnce();
			Expect.Call(_personAssignment.PersonalActivities())
				  .Return(Enumerable.Empty<IPersonalShiftLayer>());
			_mocks.ReplayAll();
			var retShifts = _target.Filter(_dateOnly, _person, shifts, _finderResult);
			retShifts.Count.Should().Be.EqualTo(1);
			_mocks.VerifyAll();
		}

		[Test]
		public void VerifyIfMeetingCannotOverrideActivity()
		{
			_personAssignment = _mocks.StrictMock<IPersonAssignment>();

			var currentDate = new DateTime(2013, 3, 1, 0, 0, 0, DateTimeKind.Utc);
			var lunch = ActivityFactory.CreateActivity("lunch");
			lunch.AllowOverwrite = false;
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
			Expect.Call(meeting.Period).Return(new DateTimePeriod(currentDate.AddHours(11), currentDate.AddHours(12)));
			Expect.Call(_part.PersonAssignment(true)).Return(_personAssignment).Repeat.AtLeastOnce();
			Expect.Call(_part.PersonMeetingCollection()).Return(readOnlymeetings).Repeat.AtLeastOnce();
			_mocks.ReplayAll();
			var retShifts = _target.Filter(_dateOnly, _person, shifts, _finderResult);
			retShifts.Count.Should().Be.EqualTo(0);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldNotFilterIfNoPersonalShift()
		{
			_personAssignment = _mocks.StrictMock<IPersonAssignment>();

			IList<IShiftProjectionCache> shifts = new List<IShiftProjectionCache>();
			var c1 = _mocks.StrictMock<IShiftProjectionCache>();
			shifts.Add(c1);
			Expect.Call(_part.PersonAssignment(true)).Return(_personAssignment).Repeat.AtLeastOnce();
			Expect.Call(_part.PersonMeetingCollection()).Return(new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting>())).Repeat.AtLeastOnce();
			Expect.Call(_personAssignment.PersonalActivities())
			      .Return(Enumerable.Empty<IPersonalShiftLayer>());
			_mocks.ReplayAll();
			var retShifts = _target.Filter(_dateOnly, _person, shifts, _finderResult);
			retShifts.Count.Should().Be.EqualTo(1);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldCheckParameters()
		{
			var result = _target.Filter(_dateOnly, _person, null, _finderResult);
			Assert.IsNull(result);

			result = _target.Filter(_dateOnly, null, new List<IShiftProjectionCache>(), _finderResult);
			Assert.IsNull(result);

			result = _target.Filter(_dateOnly, _person, new List<IShiftProjectionCache>(), null);
			Assert.IsNull(result);

			result = _target.Filter(_dateOnly, _person, new List<IShiftProjectionCache>(), _finderResult);
			Assert.That(result.Count, Is.EqualTo(0));
		}

		private static List<IVisualLayer> getLunchLayer(DateTime currentDate, IActivity lunch)
		{
			var lunchLayer = new List<IVisualLayer>
                                 {
                                     new VisualLayer(lunch,
                                                     new DateTimePeriod(currentDate.AddHours(11), currentDate.AddHours(12)),
                                                     lunch, null)
                                 };
			return lunchLayer;
		}

		private static IEnumerable<IPersonalShiftLayer> getPersonalLayers(DateTime currentDate)
		{
			var personalLayers = new []
                                     {
                                         new PersonalShiftLayer(ActivityFactory.CreateActivity("personal"),
                                                                        new DateTimePeriod(currentDate.AddHours(10),
                                                                                           currentDate.AddHours(13)))
                                     };
			return personalLayers;
		}
	}

	public class FakeScheduleDayForPerson : IScheduleDayForPerson
	{
		private readonly IScheduleDay _scheduleDay;

		public FakeScheduleDayForPerson(IScheduleDay scheduleDay)
		{
			_scheduleDay = scheduleDay;
		}

		public IScheduleDay ForPerson(IPerson person, DateOnly date)
		{
			return _scheduleDay;
		}
	}
}
