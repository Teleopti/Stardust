using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.WorkShiftCalculation
{
    [TestFixture]
    public class SkillDayBlockFinderTest
    {
        private ISkillDayBlockFinder _target;
        private SchedulingOptions  _schedulingOptions;
        private MockRepository _mock;
        private ISchedulingResultStateHolder _schedulingResultStateHolder;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _schedulingResultStateHolder = _mock.StrictMock<ISchedulingResultStateHolder>();
            _target = new SkillDayBlockFinder(_schedulingOptions, _schedulingResultStateHolder);
            _schedulingOptions = new SchedulingOptions();
        }

        [Test]
        public void FindSkillDayFromBlock()
        {
            var date = DateTime.UtcNow ;
            var skillDay1 = _mock.StrictMock<ISkillDay>();
            var skillDay2 = _mock.StrictMock<ISkillDay>();

            var scheduleDictionary = _mock.StrictMock<IScheduleDictionary>();
            var scheduleDateTimePeriod = _mock.StrictMock<IScheduleDateTimePeriod>();
            var dateTimePeriod = new DateTimePeriod(date, date.AddDays(2));

            using (_mock.Record())
            {
                Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary);
                Expect.Call(scheduleDictionary.Period).Return(scheduleDateTimePeriod);
                Expect.Call(scheduleDateTimePeriod.LoadedPeriod()).Return(dateTimePeriod);
                Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly>())).Return(new List<ISkillDay> {skillDay1, skillDay2 }).IgnoreArguments();
            }
            using (_mock.Playback())
            {
                Assert.AreEqual(_target.ExtractSkillDays(date), new List<ISkillDay>{skillDay1, skillDay2});    
            }
        }
    }

    public  interface ISkillDayBlockFinder
    {
        List<ISkillDay> ExtractSkillDays(DateTime dateTime);
    }

    public class SkillDayBlockFinder : ISkillDayBlockFinder
    {
        public ISchedulingResultStateHolder SchedulingResultStateHolder { get; set; }
        private readonly SchedulingOptions _schedulingOptions;

        public SkillDayBlockFinder(SchedulingOptions schedulingOptions, ISchedulingResultStateHolder schedulingResultStateHolder)
        {
            SchedulingResultStateHolder = schedulingResultStateHolder;
            _schedulingOptions = schedulingOptions;
        }

        public List<ISkillDay> ExtractSkillDays(DateTime dateTime)
        {
            var selectedPeriod = SchedulingResultStateHolder.Schedules.Period.LoadedPeriod();
            var timeZone = TeleoptiPrincipal.Current.Regional.TimeZone;
            var retList = SchedulingResultStateHolder.SkillDaysOnDateOnly(selectedPeriod.ToDateOnlyPeriod(timeZone).DayCollection()).ToList();
           
            return retList;
        }
    }
}
