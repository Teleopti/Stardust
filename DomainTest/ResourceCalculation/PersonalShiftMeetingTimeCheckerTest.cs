using System;
using System.Collections.Generic;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Meetings;


namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[TestFixture]
	public class PersonalShiftMeetingTimeCheckerTest
	{
		private PersonalShiftMeetingTimeChecker _target;
		private IEditableShift _mainShift;
		private IShiftCategory _shiftCategory;
		private IPersonMeeting _personMeeting;
		private IPersonAssignment _personAssignment;
		private EditableShiftLayer _mainShiftLayer;
		private EditableShiftLayer _mainShiftLayerNotInWorkTime;
		private EditableShiftLayer _mainShiftLayerNoOverwrite;
		private IActivity _activity;
		private IActivity _activityNotInWorktime;
		private IActivity _activityNoOverwrite;
		private IActivity _personalActivity;
		private DateTimePeriod _mainDateTimePeriod;
		private DateTimePeriod _mainDateTimePeriodNotInWorkTime;
		private DateTimePeriod _mainDateTimePeriodNoOverwrite;
		private IMeeting _meeting;
		private IPerson _person;
		private IScenario _scenario;
		private IMeetingPerson _meetingPerson;
			
		[SetUp]
		public void Setup()
		{
			_scenario = new Scenario("scenario");
			_person = new Person();
			_meetingPerson = new MeetingPerson(_person, false);
			_shiftCategory = new ShiftCategory("shiftCategory");
			var start = new DateTime(2013, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2013, 1, 1, 12, 0, 0, DateTimeKind.Utc);
			_mainDateTimePeriod = new DateTimePeriod(start, end);
			_mainDateTimePeriodNotInWorkTime = new DateTimePeriod(end, end.AddHours(2));
			_mainDateTimePeriodNoOverwrite = new DateTimePeriod(end.AddHours(2), end.AddHours(4));
			_activity = new Activity("activity") {InWorkTime = true, AllowOverwrite = true};
			_activityNotInWorktime = new Activity("activityNotInWorktime") {AllowOverwrite = true};
			_activityNoOverwrite = new Activity("activityNoOverwrite") { InWorkTime = true, AllowOverwrite = false };
			_personalActivity = new Activity("personalActivity");
			_mainShiftLayer = new EditableShiftLayer(_activity, _mainDateTimePeriod);
			_mainShiftLayerNotInWorkTime = new EditableShiftLayer(_activityNotInWorktime, _mainDateTimePeriodNotInWorkTime);
			_mainShiftLayerNoOverwrite = new EditableShiftLayer(_activityNoOverwrite, _mainDateTimePeriodNoOverwrite);
			_mainShift = new EditableShift(_shiftCategory);
			_mainShift.LayerCollection.Add(_mainShiftLayer);
			_mainShift.LayerCollection.Add(_mainShiftLayerNotInWorkTime);
			_mainShift.LayerCollection.Add(_mainShiftLayerNoOverwrite);
			_target = new PersonalShiftMeetingTimeChecker();
		}

		[Test]
		public void ShouldReturnTrueWhenWorkTimeAndContractTimeAreUnchangedWhenAddingMeeting()
		{
			_meeting = new Meeting(_person, new List<IMeetingPerson>(), "subject", "location", "description", _personalActivity,_scenario);
			_personMeeting = new PersonMeeting(_meeting, _meetingPerson, _mainDateTimePeriodNotInWorkTime);
			var meetings = new [] {_personMeeting};
			var result = _target.CheckTimeMeeting(_mainShift, meetings);

			Assert.IsTrue(result);		
		}

		[Test]
		public void ShouldReturnTrueWhenWorkTimeAndContractTimeAreUnchangedWhenAddingPersonalShift()
		{
			_personAssignment = new PersonAssignment(_person, _scenario, new DateOnly(2013, 1, 1));
			_personAssignment.AddPersonalActivity(_personalActivity, _mainDateTimePeriodNotInWorkTime);
			_personAssignment.AddPersonalActivity(_activity, _mainDateTimePeriod);
			var result = _target.CheckTimePersonAssignment(_mainShift, _personAssignment);

			Assert.IsTrue(result);			
		}

		[Test]
		public void ShouldReturnFalseWhenPeriodsDoNottIntersectWhenAddingMeeting()
		{
			var periodOutside = new DateTimePeriod(_mainDateTimePeriod.StartDateTime.AddDays(-1), _mainDateTimePeriod.EndDateTime.AddDays(-1));
			_meeting = new Meeting(_person, new List<IMeetingPerson>(), "subject", "location", "description", _personalActivity, _scenario);
			_personMeeting = new PersonMeeting(_meeting, _meetingPerson, periodOutside);
			var meetings = new [] { _personMeeting };
			var result = _target.CheckTimeMeeting(_mainShift, meetings);

			Assert.IsFalse(result);	
		}

		[Test]
		public void ShouldReturnFalseWhenPeriodsDoNotIntersectWhenAddingPersonalShift()
		{
			var periodOutside = new DateTimePeriod(_mainDateTimePeriod.StartDateTime.AddDays(-1), _mainDateTimePeriod.EndDateTime.AddDays(-1));
			_personAssignment = new PersonAssignment(_person, _scenario, new DateOnly(2013, 1, 1));
			_personAssignment.AddPersonalActivity(_personalActivity, periodOutside);
			var result = _target.CheckTimePersonAssignment(_mainShift, _personAssignment);

			Assert.IsFalse(result);	
		}

		[Test]
		public void ShouldReturnFalseWhenWorkTimeIsChangedWhenAddingMeeting()
		{
			_personalActivity.InWorkTime = true;
			_meeting = new Meeting(_person, new List<IMeetingPerson>(), "subject", "location", "description", _personalActivity, _scenario);
			_personMeeting = new PersonMeeting(_meeting, _meetingPerson, _mainDateTimePeriodNotInWorkTime);
			var meetings = new [] { _personMeeting };
			var result = _target.CheckTimeMeeting(_mainShift, meetings);

			Assert.IsFalse(result);			
		}

		[Test]
		public void ShouldReturnFalseWhenWorkTimeIsChangedWhenAddingPersonalShift()
		{
			_personalActivity.InWorkTime = true;
			_personAssignment = new PersonAssignment(_person, _scenario, new DateOnly(2013, 1, 1));
			_personAssignment.AddPersonalActivity(_personalActivity, _mainDateTimePeriodNotInWorkTime);
			var result = _target.CheckTimePersonAssignment(_mainShift, _personAssignment);

			Assert.IsFalse(result);	
		}


		[Test]
		public void ShouldReturnFalseWhenContractTimeIsChangedWhenAddingMeeting()
		{
			_personalActivity.InContractTime = false;
			_meeting = new Meeting(_person, new List<IMeetingPerson>(), "subject", "location", "description", _personalActivity, _scenario);
			_personMeeting = new PersonMeeting(_meeting, _meetingPerson, _mainDateTimePeriodNotInWorkTime);
			var meetings = new [] { _personMeeting };
			var result = _target.CheckTimeMeeting(_mainShift, meetings);

			Assert.IsFalse(result);	
		}

		
		[Test]
		public void ShouldReturnFalseWhenContractTimeIsChangedWhenAddingPersonalShift()
		{
			_personalActivity.InContractTime = false;
			_personAssignment = new PersonAssignment(_person, _scenario, new DateOnly(2013, 1, 1));
			_personAssignment.AddPersonalActivity(_personalActivity, _mainDateTimePeriodNotInWorkTime);
			var result = _target.CheckTimePersonAssignment(_mainShift, _personAssignment);

			Assert.IsFalse(result);
		}

		[Test]
		public void ShouldReturnFalseWhenActivityDoNotAllowOverwriteWhenAddingMeeting()
		{
			_meeting = new Meeting(_person, new List<IMeetingPerson>(), "subject", "location", "description", _personalActivity, _scenario);
			_personMeeting = new PersonMeeting(_meeting, _meetingPerson, _mainDateTimePeriodNoOverwrite);
			var meetings = new [] { _personMeeting };
			var result = _target.CheckTimeMeeting(_mainShift, meetings);

			Assert.IsFalse(result);
		}

		[Test]
		public void ShouldReturnFalseWhenActivityDoNotAllowOverwriteWhenAddingPersonalShift()
		{
			_personAssignment = new PersonAssignment(_person, _scenario, new DateOnly(2013, 1, 1));
			_personAssignment.AddPersonalActivity(_personalActivity, _mainDateTimePeriodNoOverwrite);
			_personAssignment.AddPersonalActivity(_activity, _mainDateTimePeriod);

			var result = _target.CheckTimePersonAssignment(_mainShift, _personAssignment);

			Assert.IsFalse(result);	
		}

		[Test]
		public void ShouldAllowNoOverwriteActivityBetweenPersonalShifts()
		{
			_personAssignment = new PersonAssignment(_person, _scenario, new DateOnly(2013, 1, 1));
			_mainShift = new EditableShift(_shiftCategory);
			_mainShift.LayerCollection.Add(_mainShiftLayerNoOverwrite);
			_personalActivity.InContractTime = false;

			var periodBefore = new DateTimePeriod(_mainDateTimePeriodNoOverwrite.StartDateTime.AddHours(-2), _mainDateTimePeriodNoOverwrite.StartDateTime.AddHours(-1));
			var periodAfter = new DateTimePeriod(_mainDateTimePeriodNoOverwrite.EndDateTime.AddHours(1), _mainDateTimePeriodNoOverwrite.EndDateTime.AddHours(2));

			_personAssignment.AddPersonalActivity(_personalActivity, periodBefore);
			_personAssignment.AddPersonalActivity(_personalActivity, periodAfter);

			var result = _target.CheckTimePersonAssignment(_mainShift, _personAssignment);

			Assert.IsTrue(result);		
		}

		[Test]
		public void ShouldReturnFalseIfMainShiftProjectionPeriodHasNoValue()
		{
			_mainShift.LayerCollection.Clear();

			var meetings = new [] { _personMeeting };

			var result1 = _target.CheckTimeMeeting(_mainShift, meetings);
			Assert.IsFalse(result1);

			var result2 = _target.CheckTimePersonAssignment(_mainShift, _personAssignment);
			Assert.IsFalse(result2);
		}

		[Test]
		public void ShouldCheckMeetingsOnVisualLayerProjection()
		{
			var mock = new MockRepository();
			var mainShift = mock.StrictMock<IEditableShift>();
			var meeting = mock.StrictMock<IPersonMeeting>();
			var meetings = new [] { meeting };
			var projectionService = mock.StrictMock<IProjectionService>();
			var period = new DateTimePeriod(2014, 1, 1, 2014, 1, 2);
			var meetingPeriod = new DateTimePeriod(2014, 1, 1, 10, 2014, 1, 1, 11);
			var activity = new Activity("activity"){AllowOverwrite = false};
			var visualLayer = new VisualLayer(activity, period, activity);
			var visualLayers = new List<IVisualLayer> {visualLayer};
			var visualLayerCollection = mock.StrictMock<IVisualLayerCollection>();
	
			using (mock.Record())
			{
				Expect.Call(mainShift.ProjectionService()).Return(projectionService);
				Expect.Call(projectionService.CreateProjection()).Return(visualLayerCollection);
				Expect.Call(visualLayerCollection.WorkTime()).Return(TimeSpan.FromHours(8));
				Expect.Call(visualLayerCollection.ContractTime()).Return(TimeSpan.FromHours(8));
				Expect.Call(visualLayerCollection.Period()).Return(period).Repeat.AtLeastOnce();
				Expect.Call(mainShift.MakeCopy()).Return(mainShift);
				Expect.Call(meeting.Period).Return(meetingPeriod).Repeat.AtLeastOnce();
				Expect.Call(visualLayerCollection.GetEnumerator()).Return(visualLayers.GetEnumerator());
			}

			using (mock.Playback())
			{
				_target.CheckTimeMeeting(mainShift, meetings);
			}
		}
	}
}
