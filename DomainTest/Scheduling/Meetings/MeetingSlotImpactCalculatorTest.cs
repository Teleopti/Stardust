using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.Scheduling.Meetings
{
	[DomainTest]
	public class MeetingSlotImpactCalculatorTest
	{
		private MeetingSlotImpactCalculator _target;
		private MockRepository _mocks;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private IScheduleDictionary _dictionary;
		private ISkillStaffPeriodHolder _skillStaffPeriodHolder;
	    private IAllLayersAreInWorkTimeSpecification _allLayersInWorkTimeSpec;
		private IPerson _person;
		private IList<IPerson> _persons;
		private IScenario _scenario;
		private IActivity _activity;
		private IAbsence _absence;
		private IShiftCategory _shiftCategory;
		private DateTimePeriod _dateTimePeriod;
		private IPersonAssignment _personAssignment;
		private ScheduleDictionaryForTest _scheduleDictionaryWithPersonAssignment;
		private IAllLayersAreInWorkTimeSpecification _allLayersAreInWorkTimeSpecification;
		private ISchedulingResultStateHolder _schedulingResultState;
		private MeetingSlotImpactCalculator _calculator;
			
		private void setup()
		{
			_mocks = new MockRepository();
			_schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
			_dictionary = _mocks.StrictMock<IScheduleDictionary>();
			_skillStaffPeriodHolder = _mocks.StrictMock<ISkillStaffPeriodHolder>();
		    _allLayersInWorkTimeSpec = _mocks.StrictMock<IAllLayersAreInWorkTimeSpecification>();
            _target = new MeetingSlotImpactCalculator(_schedulingResultStateHolder, _allLayersInWorkTimeSpec);
			_person = PersonFactory.CreatePerson();
			_persons = new List<IPerson>{_person};
			_scenario = new Scenario("scenario");
			_activity = new Activity("activty") { InContractTime = true, InWorkTime = true, AllowOverwrite = true };
			_absence = new Absence { InWorkTime = true, InContractTime = true };
			_shiftCategory = new ShiftCategory("shiftCategory");
			_dateTimePeriod = new DateTimePeriod(new DateTime(2016, 1, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2016, 1, 1, 17, 0, 0, DateTimeKind.Utc));
			_personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(_person, _scenario, _activity, _dateTimePeriod, _shiftCategory);
			_scheduleDictionaryWithPersonAssignment = new ScheduleDictionaryForTest(_scenario, new DateTimePeriod(2016, 1, 1, 2016, 1, 10));
			_scheduleDictionaryWithPersonAssignment.AddPersonAssignment(_personAssignment);
			_allLayersAreInWorkTimeSpecification = new AllLayersAreInWorkTimeSpecification();
			_schedulingResultState = new SchedulingResultStateHolder();
			_schedulingResultState.Schedules = _scheduleDictionaryWithPersonAssignment;
			_calculator = new MeetingSlotImpactCalculator(_schedulingResultState, _allLayersAreInWorkTimeSpecification);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldReturnSomething()
		{
			setup();
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
                                activity);
            var layerLunch =
                new VisualLayer(lunchActivity, dateTimePeriod,
                                lunchActivity);
            var layerAfter = new VisualLayer(activity, dateTimePeriod,
                                activity);
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

		[Test]
		public void ShouldNotCalulateOnIntradayAbsences()
		{
			setup();
			var dateTimePeriodAbsence = new DateTimePeriod(_dateTimePeriod.StartDateTime, _dateTimePeriod.StartDateTime.AddHours(1)); 
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(_person, _scenario, dateTimePeriodAbsence, _absence);
			_scheduleDictionaryWithPersonAssignment.AddPersonAbsence(personAbsence);

			_calculator.GetImpact(_persons, _dateTimePeriod).HasValue.Should().Be.False();
		}

		[Test]
		public void ShouldNotCalculateOnDayOff()
		{
			setup();
			_scheduleDictionaryWithPersonAssignment.Remove(_person);
			var personAssignmentWithDayOff = PersonAssignmentFactory.CreateAssignmentWithDayOff(_person, _scenario, new DateOnly(_dateTimePeriod.StartDateTime), TimeSpan.FromHours(24), TimeSpan.FromHours(0), TimeSpan.FromHours(12));
			_scheduleDictionaryWithPersonAssignment.AddPersonAssignment(personAssignmentWithDayOff);

			_calculator.GetImpact(_persons, _dateTimePeriod).HasValue.Should().Be.False();
		}

		[Test]
		public void SholdNotCalculateWhenNoAssignment()
		{
			setup();
			_scheduleDictionaryWithPersonAssignment.Remove(_person);
			_calculator.GetImpact(_persons, _dateTimePeriod).HasValue.Should().Be.False();	
		}

		[Test]
		public void ShouldNotCalculateWhenNoContractTime()
		{
			setup();
			_activity.InContractTime = false;
			_calculator.GetImpact(_persons, _dateTimePeriod).HasValue.Should().Be.False();
		}

		[Test]
		public void ShouldNotCalculateWhenNoWorkTime()
		{
			setup();
			_activity.InWorkTime = false;
			_calculator.GetImpact(_persons, _dateTimePeriod).HasValue.Should().Be.False();
		}
	}	
}