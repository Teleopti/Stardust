using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[TestFixture]
	public class PersonalShiftMeetingTimeCheckerTest
	{
		private PersonalShiftMeetingTimeChecker _target;
		private IEditorShift _mainShift;
		private IShiftCategory _shiftCategory;
		private IPersonMeeting _personMeeting;
		private IPersonAssignment _personAssignment;
		private IPersonalShift _personalShift1;
		private IPersonalShift _personalShift2;
		private MainShiftActivityLayer _mainShiftLayer;
		private MainShiftActivityLayer _mainShiftLayerNotInWorkTime;
		private MainShiftActivityLayer _mainShiftLayerNoOverwrite;
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
		private IVisualLayerCollection _mainShiftProjection;

			
		[SetUp]
		public void Setup()
		{
			_personalShift1 = new PersonalShift();
			_personalShift2 = new PersonalShift();
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
			_mainShiftLayer = new MainShiftActivityLayer(_activity, _mainDateTimePeriod);
			_mainShiftLayerNotInWorkTime = new MainShiftActivityLayer(_activityNotInWorktime, _mainDateTimePeriodNotInWorkTime);
			_mainShiftLayerNoOverwrite = new MainShiftActivityLayer(_activityNoOverwrite, _mainDateTimePeriodNoOverwrite);
			_mainShift = new EditorShift(_shiftCategory);
			_mainShift.LayerCollection.Add(_mainShiftLayer);
			_mainShift.LayerCollection.Add(_mainShiftLayerNotInWorkTime);
			_mainShift.LayerCollection.Add(_mainShiftLayerNoOverwrite);
			_mainShiftProjection = _mainShift.ProjectionService().CreateProjection();
			_target = new PersonalShiftMeetingTimeChecker();
		}

		[Test]
		public void ShouldReturnTrueWhenWorkTimeAndContractTimeAreUnchangedWhenAddingMeeting()
		{
			_meeting = new Meeting(_person, new List<IMeetingPerson>(), "subject", "location", "description", _personalActivity,_scenario);
			_personMeeting = new PersonMeeting(_meeting, _meetingPerson, _mainDateTimePeriodNotInWorkTime);
			var meetings = new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting>{_personMeeting});
			var result = _target.CheckTimeMeeting(_mainShift, _mainShiftProjection, meetings);

			Assert.IsTrue(result);		
		}

		[Test]
		public void ShouldReturnTrueWhenWorkTimeAndContractTimeAreUnchangedWhenAddingPersonalShift()
		{
			_personAssignment = new PersonAssignment(_person, _scenario, new DateOnly(2013, 1, 1));
			var shiftLayer1 = new PersonalShiftActivityLayer(_personalActivity, _mainDateTimePeriodNotInWorkTime);
			var shiftLayer2 = new PersonalShiftActivityLayer(_activity, _mainDateTimePeriod);
			_personalShift1.LayerCollection.Add(shiftLayer1);
			_personalShift2.LayerCollection.Add(shiftLayer2);
			_personAssignment.AddPersonalShift(_personalShift1);
			_personAssignment.AddPersonalShift(_personalShift2);
			var personAssignments = new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> {_personAssignment});
			var result = _target.CheckTimePersonAssignment(_mainShift, _mainShiftProjection, personAssignments);

			Assert.IsTrue(result);			
		}

		[Test]
		public void ShouldReturnFalseWhenPeriodsDoNottIntersectWhenAddingMeeting()
		{
			var periodOutside = new DateTimePeriod(_mainDateTimePeriod.StartDateTime.AddDays(-1), _mainDateTimePeriod.EndDateTime.AddDays(-1));
			_meeting = new Meeting(_person, new List<IMeetingPerson>(), "subject", "location", "description", _personalActivity, _scenario);
			_personMeeting = new PersonMeeting(_meeting, _meetingPerson, periodOutside);
			var meetings = new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting> { _personMeeting });
			var result = _target.CheckTimeMeeting(_mainShift, _mainShiftProjection, meetings);

			Assert.IsFalse(result);	
		}

		[Test]
		public void ShouldReturnFalseWhenPeriodsDoNotIntersectWhenAddingPersonalShift()
		{
			var periodOutside = new DateTimePeriod(_mainDateTimePeriod.StartDateTime.AddDays(-1), _mainDateTimePeriod.EndDateTime.AddDays(-1));
			_personAssignment = new PersonAssignment(_person, _scenario, new DateOnly(2013, 1, 1));
			var shiftLayer = new PersonalShiftActivityLayer(_personalActivity, periodOutside);
			_personalShift1.LayerCollection.Add(shiftLayer);
			_personAssignment.AddPersonalShift(_personalShift1);
			var personAssignments = new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> { _personAssignment });
			var result = _target.CheckTimePersonAssignment(_mainShift, _mainShiftProjection, personAssignments);

			Assert.IsFalse(result);	
		}

		[Test]
		public void ShouldReturnFalseWhenWorkTimeIsChangedWhenAddingMeeting()
		{
			_personalActivity.InWorkTime = true;
			_meeting = new Meeting(_person, new List<IMeetingPerson>(), "subject", "location", "description", _personalActivity, _scenario);
			_personMeeting = new PersonMeeting(_meeting, _meetingPerson, _mainDateTimePeriodNotInWorkTime);
			var meetings = new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting> { _personMeeting });
			var result = _target.CheckTimeMeeting(_mainShift, _mainShiftProjection, meetings);

			Assert.IsFalse(result);			
		}

		[Test]
		public void ShouldReturnFalseWhenWorkTimeIsChangedWhenAddingPersonalShift()
		{
			_personalActivity.InWorkTime = true;
			_personAssignment = new PersonAssignment(_person, _scenario, new DateOnly(2013, 1, 1));
			var shiftLayer = new PersonalShiftActivityLayer(_personalActivity, _mainDateTimePeriodNotInWorkTime);
			_personalShift1.LayerCollection.Add(shiftLayer);
			_personAssignment.AddPersonalShift(_personalShift1);
			var personAssignments = new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> { _personAssignment });
			var result = _target.CheckTimePersonAssignment(_mainShift, _mainShiftProjection, personAssignments);

			Assert.IsFalse(result);	
		}


		[Test]
		public void ShouldReturnFalseWhenContractTimeIsChangedWhenAddingMeeting()
		{
			_personalActivity.InContractTime = false;
			_meeting = new Meeting(_person, new List<IMeetingPerson>(), "subject", "location", "description", _personalActivity, _scenario);
			_personMeeting = new PersonMeeting(_meeting, _meetingPerson, _mainDateTimePeriodNotInWorkTime);
			var meetings = new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting> { _personMeeting });
			var result = _target.CheckTimeMeeting(_mainShift, _mainShiftProjection, meetings);

			Assert.IsFalse(result);	
		}

		
		[Test]
		public void ShouldReturnFalseWhenContractTimeIsChangedWhenAddingPersonalShift()
		{
			_personalActivity.InContractTime = false;
			_personAssignment = new PersonAssignment(_person, _scenario, new DateOnly(2013, 1, 1));
			var shiftLayer = new PersonalShiftActivityLayer(_personalActivity, _mainDateTimePeriodNotInWorkTime);
			_personalShift1.LayerCollection.Add(shiftLayer);
			_personAssignment.AddPersonalShift(_personalShift1);
			var personAssignments = new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> { _personAssignment });
			var result = _target.CheckTimePersonAssignment(_mainShift, _mainShiftProjection, personAssignments);

			Assert.IsFalse(result);
		}

		[Test]
		public void ShouldReturnFalseWhenActivityDoNotAllowOverwriteWhenAddingMeeting()
		{
			_meeting = new Meeting(_person, new List<IMeetingPerson>(), "subject", "location", "description", _personalActivity, _scenario);
			_personMeeting = new PersonMeeting(_meeting, _meetingPerson, _mainDateTimePeriodNoOverwrite);
			var meetings = new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting> { _personMeeting });
			var result = _target.CheckTimeMeeting(_mainShift, _mainShiftProjection, meetings);

			Assert.IsFalse(result);
		}

		[Test]
		public void ShouldReturnFalseWhenActivityDoNotAllowOverwriteWhenAddingPersonalShift()
		{
			_personAssignment = new PersonAssignment(_person, _scenario, new DateOnly(2013, 1, 1));
			var shiftLayer1 = new PersonalShiftActivityLayer(_personalActivity, _mainDateTimePeriodNoOverwrite);
			var shiftLayer2 = new PersonalShiftActivityLayer(_activity, _mainDateTimePeriod);
			_personalShift1.LayerCollection.Add(shiftLayer1);
			_personalShift2.LayerCollection.Add(shiftLayer2);
			_personAssignment.AddPersonalShift(_personalShift1);
			_personAssignment.AddPersonalShift(_personalShift2);
			var personAssignments = new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> { _personAssignment });
			var result = _target.CheckTimePersonAssignment(_mainShift, _mainShiftProjection, personAssignments);

			Assert.IsFalse(result);	
		}

		[Test]
		public void ShouldAllowNoOverwriteActivityBetweenPersonalShifts()
		{
			_personAssignment = new PersonAssignment(_person, _scenario, new DateOnly(2013, 1, 1));
			_mainShift = new EditorShift(_shiftCategory);
			_mainShift.LayerCollection.Add(_mainShiftLayerNoOverwrite);
			_mainShiftProjection = _mainShift.ProjectionService().CreateProjection();
			_personalActivity.InContractTime = false;

			var periodBefore = new DateTimePeriod(_mainDateTimePeriodNoOverwrite.StartDateTime.AddHours(-2), _mainDateTimePeriodNoOverwrite.StartDateTime.AddHours(-1));
			var periodAfter = new DateTimePeriod(_mainDateTimePeriodNoOverwrite.EndDateTime.AddHours(1), _mainDateTimePeriodNoOverwrite.EndDateTime.AddHours(2));
			var shiftLayer1 = new PersonalShiftActivityLayer(_personalActivity, periodBefore);
			var shiftLayer2 = new PersonalShiftActivityLayer(_personalActivity, periodAfter);

			_personalShift1.LayerCollection.Add(shiftLayer1);
			_personalShift2.LayerCollection.Add(shiftLayer2);
			_personAssignment.AddPersonalShift(_personalShift1);
			_personAssignment.AddPersonalShift(_personalShift2);

			var personAssignments = new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> { _personAssignment });
			var result = _target.CheckTimePersonAssignment(_mainShift, _mainShiftProjection, personAssignments);

			Assert.IsTrue(result);		
		}

		[Test]
		public void ShouldReturnFalseIfMainShiftProjectionPeriodHasNoValue()
		{
			_mainShift.LayerCollection.Clear();
			var projection = _mainShift.ProjectionService().CreateProjection();
			var personAssignments = new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> { _personAssignment });
			var meetings = new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting> { _personMeeting });

			var result1 = _target.CheckTimeMeeting(_mainShift, projection, meetings);
			Assert.IsFalse(result1);

			var result2 = _target.CheckTimePersonAssignment(_mainShift, projection, personAssignments);
			Assert.IsFalse(result2);
		}
	}
}
