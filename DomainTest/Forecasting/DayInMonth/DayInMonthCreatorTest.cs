using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.DayInMonth;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.DayInMonth
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
            _mocks.VerifyAll();

        }

        private IList<ITaskOwner> getWorkloadDays()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Forecasting\DayInMonth\DayInMonthSeasonalityData.txt");
            var ret = new List<ITaskOwner>();
            string line;
            var file = new StreamReader(path);
            while ((line = file.ReadLine()) != null)
            {
                var item = line.Split(';');
                var currentDate = DateTime.Parse(item[0]);
                var workloadDay = new TaskOwnerForTest(new DateOnly(currentDate)){Tasks = double.Parse(item[1])};
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

        public override double Tasks { get; set; }
        public override DateOnly CurrentDate { get { return _date; } }
    }

    public interface IDayInMonthCreator
    {
        void Create(IVolumeYear dayInMonth);
    }

    public class DayInMonthCreator : IDayInMonthCreator
    {
        public void Create(IVolumeYear dayInMonth)
        {
            //create a list to hold data temporary
            var temps = new List<TempData>();
            for (int i = 0; i < 30; i++)
            {
                temps.Add(new TempData{Day = i+1});
            }
            double totalSum = 0;
            var days = dayInMonth.TaskOwnerDays;
            foreach (var taskOwner in days)
            {
                var idx = DayInMonthHelper.DayIndex(taskOwner.CurrentDate);
                var temp = temps[idx - 1];
                temp.Tasks.Add(taskOwner.Tasks);
                totalSum += taskOwner.Tasks;
            }
            double totalAvg = 0;
            if(days.Count > 0)
                totalAvg = totalSum/days.Count;

            foreach (var tempData in temps)
            {
                double index = 1;
                if (totalAvg > 0 && tempData.AvgTasks > 0)
                    index = 1 + (tempData.AvgTasks - totalAvg)/totalAvg;
                dayInMonth.PeriodTypeCollection.Add(tempData.Day, new DayInMonthItem { TaskIndex = index, Day = tempData.Day });
            }
        }

        internal class TempData
        {
            public TempData()
            {
                Tasks = new List<double>();
            }

            public int Day { get; set; }
            public IList<double> Tasks { get; private set; }

            public double AvgTasks
            {
                get {
                    if (Tasks.Count == 0) return 0;
                    return Tasks.Sum()/Tasks.Count;
                }
            }

            public double SumTasks
            {
                get { return Tasks.Sum(); }
            }
        }
    }
        
}