using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation.GroupScheduling
{
	[TestFixture]
	public class GroupPersonConsistentCheckerTest
	{
		private GroupPersonConsistentChecker _target;
		private IScheduleDictionary _scheduleDictionary;
		private IScheduleRange _rangeUnscheduled;
		private IScheduleRange _rangeScheduled;
		private IScheduleRange _rangeScheduledOther;
		private IScheduleDay _unscheduledDay;

		private IScheduleDay _scheduledDay;
		private IScheduleDay _scheduledDayOtherCategory;

		private readonly IShiftCategory _category1 = new ShiftCategory("kat1");
		private readonly IShiftCategory _category2 = new ShiftCategory("kat2");

		private IPerson _person1;
		private IPerson _person2;
		private IList<IPerson> _persons;
		private MockRepository _mocks;
	    private IVirtualSchedulePeriod _virtualPeriod;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;

	    [SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
	        _person1 = _mocks.StrictMock<IPerson>();
            _person2 = _mocks.StrictMock<IPerson>();
		    _virtualPeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
			_persons = new List<IPerson> { _person1, _person2 };
			_scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
	    	_schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
			_target = new GroupPersonConsistentChecker(_schedulingResultStateHolder);
	    	//Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDictionary);
			_rangeUnscheduled = _mocks.StrictMock<IScheduleRange>();
			_rangeScheduled = _mocks.StrictMock<IScheduleRange>();
			_rangeScheduledOther = _mocks.StrictMock<IScheduleRange>();
			_unscheduledDay = _mocks.StrictMock<IScheduleDay>();
			_scheduledDay = _mocks.StrictMock<IScheduleDay>();
			_scheduledDayOtherCategory = _mocks.StrictMock<IScheduleDay>();
		}

		[Test]
		public void ShouldReturnTrueIfAllUnscheduled()
		{
			var date = new DateOnly(2010,10,4);
			var schedulingOptions = new SchedulingOptions{UseGroupSchedulingCommonCategory = true};
            Expect.Call(_person1.VirtualSchedulePeriod(date)).Return(_virtualPeriod);
            Expect.Call(_person2.VirtualSchedulePeriod(date)).Return(_virtualPeriod);
            Expect.Call(_virtualPeriod.IsValid).Return(true).Repeat.Twice();

			Expect.Call(_scheduleDictionary[_person1]).Return(_rangeUnscheduled);
			Expect.Call(_scheduleDictionary[_person2]).Return(_rangeUnscheduled);

			Expect.Call(_rangeUnscheduled.ScheduledDay(date)).Return(_unscheduledDay).Repeat.Twice();
			Expect.Call(_unscheduledDay.SignificantPart()).Return(SchedulePartView.None).Repeat.Twice();
			_mocks.ReplayAll();
			var result = _target.AllPersonsHasSameOrNoneScheduled(_scheduleDictionary,_persons,date,schedulingOptions);
			Assert.That(result,Is.True);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldReturnTrueIfAllUnscheduledForGroupPerson()
		{
			var date = new DateOnly(2010,10,4);
			var schedulingOptions = new SchedulingOptions{UseGroupSchedulingCommonCategory = true, UseGroupSchedulingCommonStart = true};
            var groupPerson = _mocks.StrictMock<IGroupPerson>();
            Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDictionary);
		    Expect.Call(groupPerson.GroupMembers).Return(
		        new ReadOnlyCollection<IPerson>(new List<IPerson> {_person1, _person2}));
            Expect.Call(_person1.VirtualSchedulePeriod(date)).Return(_virtualPeriod);
            Expect.Call(_person2.VirtualSchedulePeriod(date)).Return(_virtualPeriod);
            Expect.Call(_virtualPeriod.IsValid).Return(true).Repeat.Twice();

			Expect.Call(_scheduleDictionary[_person1]).Return(_rangeUnscheduled);
			Expect.Call(_scheduleDictionary[_person2]).Return(_rangeUnscheduled);

			Expect.Call(_rangeUnscheduled.ScheduledDay(date)).Return(_unscheduledDay).Repeat.Twice();
			Expect.Call(_unscheduledDay.SignificantPart()).Return(SchedulePartView.None).Repeat.Twice();
			_mocks.ReplayAll();
            var result = _target.AllPersonsHasSameOrNoneScheduled(groupPerson,date, schedulingOptions);
			Assert.That(result,Is.True);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldReturnTrueIfSameCategory()
		{
			var date = new DateOnly(2010, 10, 4);
			var schedulingOptions = new SchedulingOptions { UseGroupSchedulingCommonCategory = true };
			var mainShift = _mocks.StrictMock<IEditorShift>();
            Expect.Call(_person1.VirtualSchedulePeriod(date)).Return(_virtualPeriod);
            Expect.Call(_person2.VirtualSchedulePeriod(date)).Return(_virtualPeriod);
            Expect.Call(_virtualPeriod.IsValid).Return(true).Repeat.Twice();
			Expect.Call(_scheduleDictionary[_person1]).Return(_rangeScheduled);
			Expect.Call(_scheduleDictionary[_person2]).Return(_rangeScheduled);

			Expect.Call(_rangeScheduled.ScheduledDay(date)).Return(_scheduledDay).Repeat.Twice();
			Expect.Call(_scheduledDay.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Twice();
			Expect.Call(_scheduledDay.GetEditorShift()).Return(mainShift).Repeat.Twice();
			Expect.Call(mainShift.ShiftCategory).Return(_category1).Repeat.Twice();
			_mocks.ReplayAll();
			var result = _target.AllPersonsHasSameOrNoneScheduled(_scheduleDictionary, _persons, date, schedulingOptions);
			Assert.That(result, Is.True);
			Assert.That(_target.CommonPossibleStartEndCategory.ShiftCategory,Is.EqualTo(_category1));
			_mocks.VerifyAll();
		}
        
        [Test]
		public void ShouldReturnTrueIfSameActivity()
		{
			var date = new DateOnly(2010, 10, 4);
			var mainShift = _mocks.StrictMock<IEditorShift>();
            var projectionService = _mocks.StrictMock<IProjectionService>();
            var activity = ActivityFactory.CreateActivity("lunch");
            var schedulingOptions = new SchedulingOptions { UseCommonActivity = true, CommonActivity = activity};
            var lunch = new DateTimePeriod(new DateTime(2010, 10, 4, 12, 0, 0, DateTimeKind.Utc), new DateTime(2010, 10, 4, 13, 0, 0, DateTimeKind.Utc));
            var layerLunch = new VisualLayer(activity, lunch, activity, null);
            
            Expect.Call(_person1.VirtualSchedulePeriod(date)).Return(_virtualPeriod);
            Expect.Call(_person2.VirtualSchedulePeriod(date)).Return(_virtualPeriod);
            Expect.Call(_virtualPeriod.IsValid).Return(true).Repeat.Twice();
			Expect.Call(_scheduleDictionary[_person1]).Return(_rangeScheduled);
			Expect.Call(_scheduleDictionary[_person2]).Return(_rangeScheduled);

			Expect.Call(_rangeScheduled.ScheduledDay(date)).Return(_scheduledDay).Repeat.Twice();
			Expect.Call(_scheduledDay.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Twice();
			Expect.Call(_scheduledDay.GetEditorShift()).Return(mainShift).Repeat.Twice();
            Expect.Call(mainShift.ProjectionService()).Return(projectionService).Repeat.Twice();
            Expect.Call(projectionService.CreateProjection()).Return(new VisualLayerCollection(_person1,
                                                                                               new List<IVisualLayer> { layerLunch }, new ProjectionPayloadMerger())).Repeat.Twice();
			_mocks.ReplayAll();
			var result = _target.AllPersonsHasSameOrNoneScheduled(_scheduleDictionary, _persons, date, schedulingOptions);
			Assert.That(result, Is.True);
			Assert.That(_target.CommonPossibleStartEndCategory.ActivityPeriods.Count,Is.EqualTo(1));
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldReturnFalseIfDifferentCategory()
		{
			var date = new DateOnly(2010, 10, 4);
			var schedulingOptions = new SchedulingOptions { UseGroupSchedulingCommonCategory = true };
			var mainShift = _mocks.StrictMock<IEditorShift>();
			var mainShiftOther = _mocks.StrictMock<IEditorShift>();
            Expect.Call(_person1.VirtualSchedulePeriod(date)).Return(_virtualPeriod);
            Expect.Call(_person2.VirtualSchedulePeriod(date)).Return(_virtualPeriod);
            Expect.Call(_virtualPeriod.IsValid).Return(true).Repeat.Twice();
			Expect.Call(_scheduleDictionary[_person1]).Return(_rangeScheduled);
			Expect.Call(_scheduleDictionary[_person2]).Return(_rangeScheduledOther);

			Expect.Call(_rangeScheduled.ScheduledDay(date)).Return(_scheduledDay);
			Expect.Call(_rangeScheduledOther.ScheduledDay(date)).Return(_scheduledDayOtherCategory);

			Expect.Call(_scheduledDay.SignificantPart()).Return(SchedulePartView.MainShift);
			Expect.Call(_scheduledDayOtherCategory.SignificantPart()).Return(SchedulePartView.MainShift);

			Expect.Call(_scheduledDay.GetEditorShift()).Return(mainShift);
			Expect.Call(_scheduledDayOtherCategory.GetEditorShift()).Return(mainShiftOther);

			Expect.Call(mainShift.ShiftCategory).Return(_category1);
			Expect.Call(mainShiftOther.ShiftCategory).Return(_category2);
			_mocks.ReplayAll();
			var result = _target.AllPersonsHasSameOrNoneScheduled(_scheduleDictionary, _persons, date, schedulingOptions);
			Assert.That(result, Is.False);
			_mocks.VerifyAll();
		}

        [Test]
        public void ShouldNotFetchShiftCategoryWhenSchedulePeriodInvalid()
        {
            var date = new DateOnly(2010, 10, 4);
			var schedulingOptions = new SchedulingOptions { UseGroupSchedulingCommonCategory = true };
            var person = _mocks.StrictMock<IPerson>();
			_persons = new List<IPerson> { person };
            Expect.Call(person.VirtualSchedulePeriod(date)).Return(_virtualPeriod);
            Expect.Call(_virtualPeriod.IsValid).Return(false);

            _mocks.ReplayAll();
			var result = _target.AllPersonsHasSameOrNoneScheduled(_scheduleDictionary, _persons, date, schedulingOptions);
            Assert.That(result, Is.True);
            _mocks.VerifyAll();
        }

		[Test]
		public void ShouldReturnFalseIfDifferentStartTime()
		{
			var date = new DateOnly(2010, 10, 4);
			var dateTime = new DateTime(2012, 6, 18, 12, 0, 0, 0, DateTimeKind.Utc);
			var schedulingOptions = new SchedulingOptions { UseGroupSchedulingCommonStart = true };
			var mainShift = _mocks.StrictMock<IEditorShift>();
			var mainShiftOther = _mocks.StrictMock<IEditorShift>();
			var projectionService = _mocks.StrictMock<IProjectionService>();
			var visualLayerCol = _mocks.StrictMock<IVisualLayerCollection>();
			Expect.Call(_person1.VirtualSchedulePeriod(date)).Return(_virtualPeriod);
			Expect.Call(_person2.VirtualSchedulePeriod(date)).Return(_virtualPeriod);
			Expect.Call(_virtualPeriod.IsValid).Return(true).Repeat.Twice();
			Expect.Call(_scheduleDictionary[_person1]).Return(_rangeScheduled);
			Expect.Call(_scheduleDictionary[_person2]).Return(_rangeScheduledOther);

			Expect.Call(_rangeScheduled.ScheduledDay(date)).Return(_scheduledDay);
			Expect.Call(_rangeScheduledOther.ScheduledDay(date)).Return(_scheduledDayOtherCategory);

			Expect.Call(_scheduledDay.SignificantPart()).Return(SchedulePartView.MainShift);
			Expect.Call(_scheduledDayOtherCategory.SignificantPart()).Return(SchedulePartView.MainShift);

			Expect.Call(_scheduledDay.GetEditorShift()).Return(mainShift);
			Expect.Call(_scheduledDayOtherCategory.GetEditorShift()).Return(mainShiftOther);

			Expect.Call(mainShift.ProjectionService()).Return(projectionService);
			Expect.Call(mainShiftOther.ProjectionService()).Return(projectionService);
			Expect.Call(projectionService.CreateProjection()).Return(visualLayerCol).Repeat.Twice();
			Expect.Call(visualLayerCol.Period()).Return(new DateTimePeriod(dateTime, dateTime.AddHours(8)));
			Expect.Call(visualLayerCol.Period()).Return(new DateTimePeriod(dateTime.AddHours(1), dateTime.AddHours(10)));

			_mocks.ReplayAll();
			var result = _target.AllPersonsHasSameOrNoneScheduled(_scheduleDictionary, _persons, date, schedulingOptions);
			Assert.That(result, Is.False);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldReturnFalseIfDifferentEndTime()
		{
			var date = new DateOnly(2010, 10, 4);
			var dateTime = new DateTime(2012, 6, 18, 12, 0, 0, 0, DateTimeKind.Utc);
			var schedulingOptions = new SchedulingOptions { UseGroupSchedulingCommonEnd = true };
			var mainShift = _mocks.StrictMock<IEditorShift>();
			var mainShiftOther = _mocks.StrictMock<IEditorShift>();
			var projectionService = _mocks.StrictMock<IProjectionService>();
			var visualLayerCol = _mocks.StrictMock<IVisualLayerCollection>();
			Expect.Call(_person1.VirtualSchedulePeriod(date)).Return(_virtualPeriod);
			Expect.Call(_person2.VirtualSchedulePeriod(date)).Return(_virtualPeriod);
			Expect.Call(_virtualPeriod.IsValid).Return(true).Repeat.Twice();
			Expect.Call(_scheduleDictionary[_person1]).Return(_rangeScheduled);
			Expect.Call(_scheduleDictionary[_person2]).Return(_rangeScheduledOther);

			Expect.Call(_rangeScheduled.ScheduledDay(date)).Return(_scheduledDay);
			Expect.Call(_rangeScheduledOther.ScheduledDay(date)).Return(_scheduledDayOtherCategory);

			Expect.Call(_scheduledDay.SignificantPart()).Return(SchedulePartView.MainShift);
			Expect.Call(_scheduledDayOtherCategory.SignificantPart()).Return(SchedulePartView.MainShift);

			Expect.Call(_scheduledDay.GetEditorShift()).Return(mainShift);
			Expect.Call(_scheduledDayOtherCategory.GetEditorShift()).Return(mainShiftOther);

			Expect.Call(mainShift.ProjectionService()).Return(projectionService);
			Expect.Call(mainShiftOther.ProjectionService()).Return(projectionService);
			Expect.Call(projectionService.CreateProjection()).Return(visualLayerCol).Repeat.Twice();
			Expect.Call(visualLayerCol.Period()).Return(new DateTimePeriod(dateTime, dateTime.AddHours(8)));
			Expect.Call(visualLayerCol.Period()).Return(new DateTimePeriod(dateTime, dateTime.AddHours(10)));

			_mocks.ReplayAll();
			var result = _target.AllPersonsHasSameOrNoneScheduled(_scheduleDictionary, _persons, date, schedulingOptions);
			Assert.That(result, Is.False);
			Assert.That(_target.CommonPossibleStartEndCategory,Is.Null);
			_mocks.VerifyAll();
		}

        [Test]
        public void ShouldHandleEndTimeAcrossMidnight()
        {
            var date = new DateOnly(2010, 10, 4);
            var period = new DateTimePeriod(new DateTime(2010, 10, 4, 17, 30, 0, DateTimeKind.Utc),
                                            new DateTime(2010, 10, 5, 1, 30, 0, DateTimeKind.Utc));
            var schedulingOptions = new SchedulingOptions
                                        {UseGroupSchedulingCommonStart = true, UseGroupSchedulingCommonEnd = true};
            var mainShift = _mocks.StrictMock<IEditorShift>();
            var groupPerson = _mocks.StrictMock<IGroupPerson>();
            var projectionService = _mocks.StrictMock<IProjectionService>();
            var visualLayerCollection = _mocks.StrictMock<IVisualLayerCollection>();
            Expect.Call(_person1.VirtualSchedulePeriod(date)).Return(_virtualPeriod);
            Expect.Call(_virtualPeriod.IsValid).Return(true);
            Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDictionary);
            Expect.Call(_scheduleDictionary[_person1]).Return(_rangeScheduled);
            Expect.Call(groupPerson.GroupMembers).Return(new ReadOnlyCollection<IPerson>(new List<IPerson> {_person1}));
            Expect.Call(_rangeScheduled.ScheduledDay(date)).Return(_scheduledDay);
            Expect.Call(_scheduledDay.SignificantPart()).Return(SchedulePartView.MainShift);
			Expect.Call(_scheduledDay.GetEditorShift()).Return(mainShift);
            Expect.Call(mainShift.ProjectionService()).Return(projectionService);
            Expect.Call(projectionService.CreateProjection()).Return(visualLayerCollection);
            Expect.Call(visualLayerCollection.Period()).Return(period);
            Expect.Call(groupPerson.CommonPossibleStartEndCategory).SetPropertyAndIgnoreArgument();
            _mocks.ReplayAll();
            var result = _target.AllPersonsHasSameOrNoneScheduled(groupPerson, date, schedulingOptions);
            Assert.That(result, Is.True);
            Assert.That(_target.CommonPossibleStartEndCategory.StartTime, Is.EqualTo(period.LocalStartDateTime.TimeOfDay));
            Assert.That(_target.CommonPossibleStartEndCategory.EndTime, Is.EqualTo(period.LocalEndDateTime.TimeOfDay.Add(TimeSpan.FromDays(1))));
            _mocks.VerifyAll();
        }

		[Test]
		public void PropertyCommonPossibleStartEndCategoryBackingFieldMayNotBeNull()
		{
			Assert.IsNull(_target.CommonPossibleStartEndCategory);
		}
	}

	
}