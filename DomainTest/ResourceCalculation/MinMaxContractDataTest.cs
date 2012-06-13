using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Rhino.Mocks;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    public class MinMaxContractDataTest
    {
        DateOnly _dateToCheck;
        IPerson _person;
        MinMaxContractData _target;
        MockRepository _mocks;
        IPersonPeriod _personPeriod;
        ISchedulingResultStateHolder _schedulingResultStateHolder;
    	//private IScheduleDictionary _scheduleDictionary;
        private SchedulingOptions _schedulingOptions;
		  private IWorkShiftWorkTime _workShiftWorkTime;
        
        

        [SetUp]
        public void Setup()
        {
            _dateToCheck = new DateOnly(2008, 05, 19);
            _mocks = new MockRepository();
            _workShiftWorkTime = _mocks.StrictMock<IWorkShiftWorkTime>();
            _person = new Person();
            _schedulingOptions = new SchedulingOptions();
            _target = new MinMaxContractData(_workShiftWorkTime, _person);
            _personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(_dateToCheck.AddDays(-2)), TeamFactory.CreateSimpleTeam("test"));
            _schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
        	//_scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
        }

      
       

        [Test]
        public void VerifyGetMinMaxWorkShiftsLengthReturnsNull()
        {
            Assert.IsNull(_target.GetMinMaxContractTime(_dateToCheck, _schedulingResultStateHolder, _schedulingOptions), "No personperiod");
            _person.AddPersonPeriod(_personPeriod);
            Assert.IsNull(_target.GetMinMaxContractTime(_dateToCheck, _schedulingResultStateHolder, _schedulingOptions), "RuleSetBag not set");
            Assert.IsNull(_target.GetMinMaxContractTime(_dateToCheck.AddDays(-10), _schedulingResultStateHolder, _schedulingOptions), "Outside personPeriod");
        }


        [Test]
        public void VerifyScheduledValuesCanBeInitialized()
        {
           
            var scenario = new Scenario("poo");
            var baseShiftDateTime = new DateTime(2000,06,06,0,00,00,DateTimeKind.Utc);
            var baseShiftDate = new DateOnly(2000, 06, 06);

            var dic = new ScheduleDictionary(scenario,
                                             new ScheduleDateTimePeriod(new DateTimePeriod(2000, 1, 1, 2100, 1, 1)));
            //IScheduleDay schedulePart = new SchedulePartUsedInTest(mocks.StrictMock<IScheduleDictionary>(), parameters, true, true);
            IScheduleDay schedulePart = ExtractedSchedule.CreateScheduleDay(dic, _person, baseShiftDate);

            //PersonDayOff pDayOff = new PersonDayOff(person, scenario, new AnchorDateTimePeriod(baseShiftDateTime, TimeSpan.Zero, new Percent(0.5d)));

            var dayOff = new DayOffTemplate(new Description("test"));
            
            dayOff.SetTargetAndFlexibility(TimeSpan.FromHours(24), TimeSpan.FromHours(6));
            dayOff.Anchor = TimeSpan.FromHours(10);
            var pDayOff = new PersonDayOff(_person, scenario, dayOff, new DateOnly(2000, 6, 6));


            
            IPersonAssignment p1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(new Activity("Test"), _person,
                                                                  new DateTimePeriod(baseShiftDateTime, baseShiftDateTime.AddHours(8)),
                                                                  new ShiftCategory("Day"),
                                                                  scenario);

            IPersonAssignment p2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(new Activity("Test"), _person,
                                                                  new DateTimePeriod(baseShiftDateTime.AddHours(10), baseShiftDateTime.AddHours(12)),
                                                                  new ShiftCategory("Day"),
                                                                  scenario);

            IPersonAssignment p3 = PersonAssignmentFactory.CreateAssignmentWithMainShift(new Activity("Test"), _person,
                                                                  new DateTimePeriod(baseShiftDateTime.AddHours(13), baseShiftDateTime.AddHours(36)),
                                                                  new ShiftCategory("Day"),
                                                                  scenario);

            IPersonAssignment p4 = PersonAssignmentFactory.CreateAssignmentWithMainShift(new Activity("Test"), _person,
                                                                  new DateTimePeriod(baseShiftDateTime, baseShiftDateTime.AddHours(5)),
                                                                  new ShiftCategory("Day"),
                                                                  scenario);

            
           
            schedulePart.Add(p1);
            _target.AddSchedulePart(schedulePart);
            MinMax<TimeSpan> minMax = _target.GetMinMaxContractTime(baseShiftDate, _schedulingResultStateHolder, _schedulingOptions).Value;
            Assert.AreEqual(TimeSpan.FromHours(8), minMax.Maximum, "Contracttime should be the same as Maximum for basedate");
            Assert.AreEqual(TimeSpan.FromHours(8), minMax.Minimum, "Contracttime should be the same as Minimum for basedate");

            _target.AddSchedulePart(schedulePart);
            Assert.AreEqual(TimeSpan.FromHours(8), _target.GetMinMaxContractTime(baseShiftDate, _schedulingResultStateHolder, _schedulingOptions).Value.Minimum, "Contracttime should be cleared when initializing");

            schedulePart.Add(p2);
            _target.AddSchedulePart(schedulePart);
            Assert.AreEqual(TimeSpan.FromHours(10), _target.GetMinMaxContractTime(baseShiftDate, _schedulingResultStateHolder, _schedulingOptions).Value.Minimum, "Contracttime should be combined on same date");

            schedulePart.Add(p3);
            _target.AddSchedulePart(schedulePart);
            Assert.AreEqual(TimeSpan.FromHours(33), _target.GetMinMaxContractTime(baseShiftDate, _schedulingResultStateHolder, _schedulingOptions).Value.Minimum, "ContractTime over midnight should count on startdate");

            schedulePart.Add(p4);
            _target.AddSchedulePart(schedulePart);
            Assert.AreEqual(TimeSpan.FromHours(33), _target.GetMinMaxContractTime(baseShiftDate, _schedulingResultStateHolder, _schedulingOptions).Value.Minimum, "Overlapping ContractTime is merged");

            schedulePart.Add(pDayOff);
            _target.AddSchedulePart(schedulePart);

            Assert.AreEqual(TimeSpan.FromHours(0), _target.GetMinMaxContractTime(baseShiftDate, _schedulingResultStateHolder, _schedulingOptions).Value.Maximum, "Day off overwrites all contracttime (TimeSpan.Zero)");
           
            
        }

    }

}
   
            