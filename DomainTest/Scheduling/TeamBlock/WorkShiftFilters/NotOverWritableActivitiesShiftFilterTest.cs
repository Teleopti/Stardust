using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftFilters
{
	[TestFixture]
	public class NotOverWritableActivitiesShiftFilterTest
	{
		private MockRepository _mocks;
		private readonly DateOnly _dateOnly = new DateOnly(2013, 3, 1);
		private INotOverWritableActivitiesShiftFilter _target;
		private IScheduleDay _part;
		private WorkShiftFinderResult _finderResult;
		private IPerson _person;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_person = PersonFactory.CreatePerson("Bill");
			_part = ScheduleDayFactory.Create(_dateOnly,_person);
			_finderResult = new WorkShiftFinderResult(_person, new DateOnly(2009, 2, 3));
			_target = new NotOverWritableActivitiesShiftFilter(()=> new FakeScheduleDayForPerson(_part));
		}

		[Test]
		public void CanFilterOutShiftsWhichCannotBeOverwritten()
		{
			var currentDate = new DateTime(2013, 3, 1, 0, 0, 0, DateTimeKind.Utc);
			var lunch = ActivityFactory.CreateActivity("lunch");
			lunch.AllowOverwrite = false;
			IList<IShiftProjectionCache> shifts = new List<IShiftProjectionCache>();
			var c1 = _mocks.StrictMock<IShiftProjectionCache>();
			shifts.Add(c1);

			_part.CreateAndAddActivity(lunch, new DateTimePeriod(currentDate.AddHours(11), currentDate.AddHours(12)), ShiftCategoryFactory.CreateShiftCategory());
			_part.CreateAndAddPersonalActivity(ActivityFactory.CreateActivity("personal"),
																		new DateTimePeriod(currentDate.AddHours(10),
																						   currentDate.AddHours(13)));

			Expect.Call(c1.MainShiftProjection).Return(new VisualLayerCollection(null, getLunchLayer(currentDate, lunch), new ProjectionPayloadMerger())).Repeat.AtLeastOnce();
			
			_mocks.ReplayAll();
			var retShifts = _target.Filter(_dateOnly, _person, shifts, _finderResult);
			retShifts.Count.Should().Be.EqualTo(0);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldNotFilterIfNoMeeting()
		{
			IList<IShiftProjectionCache> shifts = new List<IShiftProjectionCache>();
			var c1 = _mocks.StrictMock<IShiftProjectionCache>();
			shifts.Add(c1);
			
			_mocks.ReplayAll();
			var retShifts = _target.Filter(_dateOnly, _person, shifts, _finderResult);
			retShifts.Count.Should().Be.EqualTo(1);
			_mocks.VerifyAll();
		}

		[Test]
		public void VerifyIfMeetingCannotOverrideActivity()
		{
			var currentDate = new DateTime(2013, 3, 1, 0, 0, 0, DateTimeKind.Utc);
			var lunch = ActivityFactory.CreateActivity("lunch");
			lunch.AllowOverwrite = false;

			var meeting = new Meeting(PersonFactory.CreatePerson(), new[] {new MeetingPerson(_person, false)}, "subject",
				"location", "", ActivityFactory.CreateActivity("Training"), _part.Scenario)
			{
				StartDate = _dateOnly,
				EndDate = _dateOnly,
				StartTime = TimeSpan.FromHours(11),
				EndTime = TimeSpan.FromHours(12)
			};

			_part.CreateAndAddActivity(lunch, new DateTimePeriod(currentDate.AddHours(11), currentDate.AddHours(12)),
				ShiftCategoryFactory.CreateShiftCategory());
			_part.Add(meeting.GetPersonMeetings(_person).First());

			var layerCollection1 = new VisualLayerCollection(null, getLunchLayer(currentDate,lunch), new ProjectionPayloadMerger());

			IList<IShiftProjectionCache> shifts = new List<IShiftProjectionCache>();
			var c1 = _mocks.StrictMock<IShiftProjectionCache>();
			shifts.Add(c1);
			Expect.Call(c1.MainShiftProjection).Return(layerCollection1).Repeat.AtLeastOnce();
			_mocks.ReplayAll();
			var retShifts = _target.Filter(_dateOnly, _person, shifts, _finderResult);
			retShifts.Count.Should().Be.EqualTo(0);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldNotFilterIfNoPersonalShift()
		{
			IList<IShiftProjectionCache> shifts = new List<IShiftProjectionCache>();
			var c1 = _mocks.StrictMock<IShiftProjectionCache>();
			shifts.Add(c1);
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
	}
}
