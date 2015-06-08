using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Meetings
{
	[TestFixture]
	public class MeetingSlotImpactCalculatorTest
	{
		private MeetingSlotImpactCalculator _target;
		private MockRepository _mocks;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private IScheduleDictionary _dictionary;
		private ISkillStaffPeriodHolder _skillStaffPeriodHolder;
	    private IAllLayersAreInWorkTimeSpecification _allLayersInWorkTimeSpec;

	    [SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
			_dictionary = _mocks.StrictMock<IScheduleDictionary>();
			_skillStaffPeriodHolder = _mocks.StrictMock<ISkillStaffPeriodHolder>();
		    _allLayersInWorkTimeSpec = _mocks.StrictMock<IAllLayersAreInWorkTimeSpecification>();
            _target = new MeetingSlotImpactCalculator(_schedulingResultStateHolder, _allLayersInWorkTimeSpec);
		}

		[Test]
		public void ShouldReturnNullIfPersonNotHaveMainShift()
		{
			var person = GetPerson();
			var persons = new List<IPerson> {person};
			var meetingStart = new DateTime(2010, 11, 1, 11, 0, 0, DateTimeKind.Utc);
			var meetingTime = new DateTimePeriod(meetingStart, meetingStart.AddHours(1));
			var range = _mocks.StrictMock<IScheduleRange>();
			Expect.Call(_schedulingResultStateHolder.Schedules).Return(_dictionary);
			Expect.Call(_dictionary[person]).Return(range);
			var scheduleDay = _mocks.StrictMock<IScheduleDay>();
			Expect.Call(range.ScheduledDay(new DateOnly(2010, 11, 1))).Return(scheduleDay);
			Expect.Call(scheduleDay.SignificantPart()).Return(SchedulePartView.DayOff);
			_mocks.ReplayAll();
			var result = _target.GetImpact(persons, meetingTime);
			Assert.That(result,Is.Null);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldReturnNullIfPersonNotScheduledAtAllInPeriod()
		{
			var person = GetPerson();
			var persons = new List<IPerson> { person };
			var meetingStart = new DateTime(2010, 11, 1, 11, 0, 0, DateTimeKind.Utc);
			var meetingTime = new DateTimePeriod(meetingStart, meetingStart.AddHours(1));
			var range = _mocks.StrictMock<IScheduleRange>();
			var projService = _mocks.StrictMock<IProjectionService>();
			var visualLayers = _mocks.StrictMock<IVisualLayerCollection>();
			var filteredVisualLayers = _mocks.StrictMock<IFilteredVisualLayerCollection>();
			Expect.Call(_schedulingResultStateHolder.Schedules).Return(_dictionary);
			Expect.Call(_dictionary[person]).Return(range);
			var scheduleDay = _mocks.StrictMock<IScheduleDay>();
			Expect.Call(range.ScheduledDay(new DateOnly(2010, 11, 1))).Return(scheduleDay);
			Expect.Call(scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
			Expect.Call(scheduleDay.ProjectionService()).Return(projService);
			Expect.Call(projService.CreateProjection()).Return(visualLayers);
			//empty
			Expect.Call(visualLayers.FilterLayers(meetingTime)).Return(filteredVisualLayers);
			Expect.Call(filteredVisualLayers.HasLayers).Return(false);

			_mocks.ReplayAll();
			var result = _target.GetImpact(persons, meetingTime);
			Assert.That(result, Is.Null);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldReturnNullIfNotWholeMeetingIsInContractTime()
		{
			var person = GetPerson();
			var persons = new List<IPerson> { person };
			var meetingStart = new DateTime(2010, 11, 1, 11, 0, 0, DateTimeKind.Utc);
			var meetingTime = new DateTimePeriod(meetingStart, meetingStart.AddHours(1));
			var range = _mocks.StrictMock<IScheduleRange>();
			var projService = _mocks.StrictMock<IProjectionService>();
			var visualLayers = _mocks.StrictMock<IVisualLayerCollection>();
			var filteredVisualLayers = _mocks.StrictMock<IFilteredVisualLayerCollection>();

			Expect.Call(_schedulingResultStateHolder.Schedules).Return(_dictionary);
			Expect.Call(_dictionary[person]).Return(range);
			var scheduleDay = _mocks.StrictMock<IScheduleDay>();
			Expect.Call(range.ScheduledDay(new DateOnly(2010, 11, 1))).Return(scheduleDay);
			Expect.Call(scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
			Expect.Call(scheduleDay.ProjectionService()).Return(projService);
			Expect.Call(projService.CreateProjection()).Return(visualLayers);
			Expect.Call(visualLayers.FilterLayers(meetingTime)).Return(filteredVisualLayers);
			Expect.Call(filteredVisualLayers.HasLayers).Return(true);
			Expect.Call(filteredVisualLayers.ContractTime()).Return(TimeSpan.FromMinutes(50));
			_mocks.ReplayAll();
			var result = _target.GetImpact(persons, meetingTime);
			Assert.That(result, Is.Null);
			_mocks.VerifyAll();
		}

        [Test]
        public void ShouldReturnNullIfNotWholeMeetingIsInWorkTime()
        {
            var person = GetPerson();
            var persons = new List<IPerson> { person };
            var meetingStart = new DateTime(2010, 11, 1, 11, 0, 0, DateTimeKind.Utc);
            var meetingTime = new DateTimePeriod(meetingStart, meetingStart.AddHours(1));
            var range = _mocks.StrictMock<IScheduleRange>();
            var projService = _mocks.StrictMock<IProjectionService>();
            var visualLayers = _mocks.StrictMock<IVisualLayerCollection>();
            var filteredVisualLayers = _mocks.StrictMock<IFilteredVisualLayerCollection>();

            Expect.Call(_schedulingResultStateHolder.Schedules).Return(_dictionary);
            Expect.Call(_dictionary[person]).Return(range);
            var scheduleDay = _mocks.StrictMock<IScheduleDay>();
            Expect.Call(range.ScheduledDay(new DateOnly(2010, 11, 1))).Return(scheduleDay);
            Expect.Call(scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
            Expect.Call(scheduleDay.ProjectionService()).Return(projService);
            Expect.Call(projService.CreateProjection()).Return(visualLayers);
            Expect.Call(visualLayers.FilterLayers(meetingTime)).Return(filteredVisualLayers);
            Expect.Call(filteredVisualLayers.HasLayers).Return(true);
            Expect.Call(filteredVisualLayers.ContractTime()).Return(TimeSpan.FromMinutes(60));
            Expect.Call(_allLayersInWorkTimeSpec.IsSatisfiedBy(filteredVisualLayers)).Return(false);
            _mocks.ReplayAll();
            var result = _target.GetImpact(persons, meetingTime);
            Assert.That(result, Is.Null);
            _mocks.VerifyAll();
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldReturnSomething()
		{
			var person = _mocks.StrictMock<IPerson>();
			var permissionInfo = new PermissionInformation(person);
			permissionInfo.SetDefaultTimeZone((TimeZoneInfo.FindSystemTimeZoneById("Utc")));
			var persons = new List<IPerson> { person };
			
			var dateTime = new DateTime(2010, 11, 1, 11, 0, 0, DateTimeKind.Utc);
			var dateTimePeriod = new DateTimePeriod(dateTime.AddMinutes(10), dateTime.AddMinutes(70));
			var skillPeriodStartPeriod = new DateTimePeriod(dateTime.AddMinutes(-15), dateTime);

			var range = _mocks.StrictMock<IScheduleRange>();
			var projService = _mocks.StrictMock<IProjectionService>();
			var visualLayers = _mocks.StrictMock<IVisualLayerCollection>();

             var lunchActivity = ActivityFactory.CreateActivity("lunch");
            lunchActivity.AllowOverwrite = true  ;

            var activity = ActivityFactory.CreateActivity("hej");
            activity.InWorkTime = true;
            activity.AllowOverwrite = true ;
            var layerBefore = new VisualLayer(activity, dateTimePeriod,
                                activity, null);
            var layerLunch =
                new VisualLayer(lunchActivity, dateTimePeriod,
                                lunchActivity, null);
            var layerAfter = new VisualLayer(activity, dateTimePeriod,
                                activity, null);
            var layers = new List<IVisualLayer> { layerBefore, layerLunch, layerAfter };
           
			var virtualPeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
			var personPeriod = _mocks.StrictMock<IPersonPeriod>();
			var filteredVisualLayers = _mocks.StrictMock<IFilteredVisualLayerCollection>();
			var period1100To1115 = _mocks.StrictMock<ISkillStaffPeriod>();
			var period1115To1130 = _mocks.StrictMock<ISkillStaffPeriod>();
			var period1130To1145 = _mocks.StrictMock<ISkillStaffPeriod>();
			var period1145To1200 = _mocks.StrictMock<ISkillStaffPeriod>();
			var period1200To1215 = _mocks.StrictMock<ISkillStaffPeriod>();
			var skillStaffPeriods = new List<ISkillStaffPeriod>
			                        	{
			                        		period1100To1115,
			                        		period1115To1130,
			                        		period1130To1145,
			                        		period1145To1200,
			                        		period1200To1215
			                        	};
			Expect.Call(_schedulingResultStateHolder.Schedules).Return(_dictionary).Repeat.AtLeastOnce();
            Expect.Call(_dictionary[person]).Return(range).Repeat.AtLeastOnce();
			Expect.Call(person.PermissionInformation).Return(permissionInfo).Repeat.Twice();
			var scheduleDay = _mocks.StrictMock<IScheduleDay>();
			Expect.Call(range.ScheduledDay(new DateOnly(2010, 11, 1))).Return(scheduleDay);
			Expect.Call(scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
			Expect.Call(scheduleDay.ProjectionService()).Return(projService);
			Expect.Call(projService.CreateProjection()).Return(visualLayers);
			Expect.Call(visualLayers.FilterLayers(dateTimePeriod)).Return(filteredVisualLayers);
            Expect.Call(filteredVisualLayers.HasLayers).Return(true).Repeat.AtLeastOnce();
			Expect.Call(filteredVisualLayers.ContractTime()).Return(TimeSpan.FromMinutes(60));
            Expect.Call(_allLayersInWorkTimeSpec.IsSatisfiedBy(filteredVisualLayers)).Return(true);
			Expect.Call(person.VirtualSchedulePeriod(new DateOnly(2010, 11, 1))).Return(virtualPeriod);
			Expect.Call(virtualPeriod.IsValid).Return(true);
		    Expect.Call(person.Period(new DateOnly(2010, 11, 1))).Return(personPeriod);
			Expect.Call(personPeriod.PersonSkillCollection).Return(new List<IPersonSkill>());
			Expect.Call(_schedulingResultStateHolder.SkillStaffPeriodHolder).Return(_skillStaffPeriodHolder);
			Expect.Call(_skillStaffPeriodHolder.IntersectingSkillStaffPeriodList(new List<ISkill>(), dateTimePeriod)).Return(skillStaffPeriods);
		    
			Expect.Call(period1100To1115.Period).Return(skillPeriodStartPeriod.MovePeriod(TimeSpan.FromMinutes(15))).Repeat.Any();
			Expect.Call(period1115To1130.Period).Return(skillPeriodStartPeriod.MovePeriod(TimeSpan.FromMinutes(30))).Repeat.Any();
			Expect.Call(period1130To1145.Period).Return(skillPeriodStartPeriod.MovePeriod(TimeSpan.FromMinutes(45))).Repeat.Any();
			Expect.Call(period1145To1200.Period).Return(skillPeriodStartPeriod.MovePeriod(TimeSpan.FromMinutes(60))).Repeat.Any();
			Expect.Call(period1200To1215.Period).Return(skillPeriodStartPeriod.MovePeriod(TimeSpan.FromMinutes(75))).Repeat.Any();

			Expect.Call(period1100To1115.AbsoluteDifference).Return(15); // 5 should be left to 1010 - 1115
			Expect.Call(period1115To1130.AbsoluteDifference).Return(15);
			Expect.Call(period1130To1145.AbsoluteDifference).Return(15);
			Expect.Call(period1145To1200.AbsoluteDifference).Return(15);
			Expect.Call(period1200To1215.AbsoluteDifference).Return(15); // 10 should be left to 1200 - 1210

            Expect.Call(filteredVisualLayers.GetEnumerator()).Return(layers.GetEnumerator());

			_mocks.ReplayAll();
			var result = _target.GetImpact(persons, dateTimePeriod);
			Assert.That(result.HasValue);
			Assert.That(result.Value,Is.EqualTo(60));
			_mocks.VerifyAll();
		}

		private static IPerson GetPerson()
		{
			var person = PersonFactory.CreatePerson("", "");
			person.PermissionInformation.SetDefaultTimeZone((TimeZoneInfo.FindSystemTimeZoneById("Utc")));
			return person;
		}
	}

	
}