using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
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
            var skill = _mock.StrictMock<ISkill>();
            var skillDay1 = _mock.StrictMock<ISkillDay>();
            var skillDay2 = _mock.StrictMock<ISkillDay>();
            var skillDay3 = _mock.StrictMock<ISkillDay>();
            var skillDay4 = _mock.StrictMock<ISkillDay>();
            var skillDay5 = _mock.StrictMock<ISkillDay>();
            var skillDictionary = new Dictionary<ISkill, IList<ISkillDay>>();
            var skillDayList = new List<ISkillDay> {skillDay1, skillDay2, skillDay3,skillDay4,skillDay5 };
            skillDictionary.Add(skill,skillDayList  );

            var scheduleDictionary = _mock.StrictMock<IScheduleDictionary>();
            var scheduleDateTimePeriod = _mock.StrictMock<IScheduleDateTimePeriod>();

            var dateTimePeriod = new DateTimePeriod(date, date.AddDays(2));

            using (_mock.Record())
            {
                Expect.Call(scheduleDateTimePeriod.LoadedPeriod()).Return(dateTimePeriod);
                Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary);
                Expect.Call(scheduleDictionary.Period).Return(scheduleDateTimePeriod);
                Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly>())).Return(new List<ISkillDay> { skillDay4, skillDay5 }).IgnoreArguments();

            }
            
            using (_mock.Playback())
            {
                var result = _target.ExtractSkillDays(date);
                Assert.AreEqual(result[0], skillDay4);
                Assert.AreEqual(result[1], skillDay5);
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
            var dateOnlyTempList = new List<DateOnly>();
            for (var i = 0; dateTime.AddDays(i) <= selectedPeriod.EndDateTime;i++ )
            {
                dateOnlyTempList.Add(new DateOnly(dateTime.AddDays(i)));
            }
            var retList = SchedulingResultStateHolder.SkillDaysOnDateOnly(dateOnlyTempList);
           
            return retList.ToList() ;
        }
    }
}
