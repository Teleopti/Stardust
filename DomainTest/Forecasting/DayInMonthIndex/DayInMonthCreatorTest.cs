using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.DayInMonthIndex;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.Forecasting.DayInMonthIndex
{
    [TestFixture]
    public class DayInMonthCreatorTest
    {
        private DayInMonthCreator _target;
        private MockRepository _mocks;
        private IVolumeYear _dayInMonth;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _target = new DayInMonthCreator();
            _dayInMonth = _mocks.DynamicMock<IVolumeYear>();
        }

        [Test, SetCulture("en-GB")]
        public void ShouldCreateThirtyDayAverageMonth()
        {
            var periodTypes = new Dictionary<int, IPeriodType>();
            var days = getWorkloadDays();
            Expect.Call(_dayInMonth.TaskOwnerDays).Return(days);
            Expect.Call(_dayInMonth.PeriodTypeCollection).Return(periodTypes);
            _mocks.ReplayAll();
            _target.Create(_dayInMonth);
            Assert.That(periodTypes.Count,Is.EqualTo(30));
            Assert.That(Math.Round(periodTypes[1].TaskIndex,4), Is.EqualTo(1.3918d));
            Assert.That(Math.Round(periodTypes[15].TaskIndex,4), Is.EqualTo(.9109d));
            Assert.That(Math.Round(periodTypes[30].TaskIndex,4), Is.EqualTo(1.3280d));
            Assert.That(Math.Round(periodTypes[30].AfterTalkTimeIndex, 0), Is.EqualTo(1d));
            Assert.That(Math.Round(periodTypes[30].TalkTimeIndex, 0), Is.EqualTo(1d));
            _mocks.VerifyAll();

        }

        private IList<ITaskOwner> getWorkloadDays()
        {
            string path = Path.Combine(TestContext.CurrentContext.TestDirectory, @"Forecasting\DayInMonthIndex\DayInMonthSeasonalityData.txt");
            var ret = new List<ITaskOwner>();
            string line;
            var file = new StreamReader(path);
            while ((line = file.ReadLine()) != null)
            {
                var item = line.Split(';');
                var currentDate = DateTime.Parse(item[0]);
                var workloadDay = new TaskOwnerForTest(new DateOnly(currentDate)) { TotalStatisticCalculatedTasks = double.Parse(item[1]) };
                ret.Add(workloadDay);
            }

            file.Close();
            return ret;
        }
    }

    public class TaskOwnerForTest: WorkloadDay
    {
        private readonly DateOnly _date;
        public TaskOwnerForTest(DateOnly date)
        {
            _date = date;
        }

        public override double TotalStatisticCalculatedTasks { get; set; }
        public override DateOnly CurrentDate { get { return _date; } }
    }

    
        
}