﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.Forecasting
{
    /// <summary>
    /// Tests for the StatisticHelper class
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-04-02
    /// </remarks>
    [TestFixture]
    public class StatisticHelperTest
    {
        private StatisticHelper target;
        private MockRepository mocks;
        private ISkillDayRepository _skillDayRep;
        private IStatisticRepository _statisticTaskRep;
        private IValidatedVolumeDayRepository _validatedVolumeDayRep;
        private ISkill _skill;
        private IWorkload _workload;
        private IScenario _scenario;
        private DateOnlyPeriod _period;
        private IRepositoryFactory _factory;
        private IUnitOfWork _unitOfWork;

        /// <summary>
        /// Setups this instance.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            
            _skillDayRep = mocks.StrictMock<ISkillDayRepository>();
            _statisticTaskRep = mocks.StrictMock<IStatisticRepository>();
            _validatedVolumeDayRep = mocks.StrictMock<IValidatedVolumeDayRepository>();
            _factory = mocks.StrictMock<IRepositoryFactory>();
            _unitOfWork = mocks.StrictMock<IUnitOfWork>();

            Expect.Call(_factory.CreateSkillDayRepository(_unitOfWork)).Return(_skillDayRep);
            Expect.Call(_factory.CreateStatisticRepository()).Return(_statisticTaskRep);
            Expect.Call(_factory.CreateValidatedVolumeDayRepository(_unitOfWork)).Return(_validatedVolumeDayRep);

            _skill = SkillFactory.CreateSkill("TestSkill");
            _workload = WorkloadFactory.CreateWorkload(_skill);
            _scenario = mocks.StrictMock<IScenario>();
            _period = new DateOnlyPeriod(2008, 7, 16, 2008, 7, 19);
        }

        /// <summary>
        /// Verifies the skill day rep cannot be null.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VerifyRepositoryFactoryCannotBeNull()
        {
            target = new StatisticHelper(null,_unitOfWork);
        }

        /// <summary>
        /// Verifies the statistic task rep cannot be null.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VerifyUnitOfWorkCannotBeNull()
        {
            mocks.ReplayAll();
            target = new StatisticHelper(_factory,null);
        }

        /// <summary>
        /// Verifies the workload cannot be null.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VerifyWorkloadCannotBeNull()
        {
            mocks.ReplayAll();
            target = new StatisticHelper(_factory, _unitOfWork);
            target.GetWorkloadDaysWithValidatedStatistics(_period, null, _scenario, new List<IValidatedVolumeDay>());
        }

        /// <summary>
        /// Verifies the scenario cannot be null.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VerifyScenarioCannotBeNull()
        {
            mocks.ReplayAll();
            target = new StatisticHelper(_factory, _unitOfWork);
            target.GetWorkloadDaysWithValidatedStatistics(_period, _workload, null, new List<IValidatedVolumeDay>());
        }

        /// <summary>
        /// Verifies the existing validated volume days cannot be null.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-16
        /// </remarks>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VerifyExistingValidatedVolumeDaysCannotBeNull()
        {
            mocks.ReplayAll();
            target = new StatisticHelper(_factory, _unitOfWork);
            target.GetWorkloadDaysWithValidatedStatistics(_period, _workload, _scenario, null);
        }
        /// <summary>
        /// Verifies the get workload days with statistics.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        [Test]
        public void VerifyGetWorkloadDaysWithStatistics()
        {
            var period = _period.ToDateTimePeriod(_skill.TimeZone);
            IValidatedVolumeDay validatedVolumeDay1 = new ValidatedVolumeDay(_workload, _period.StartDate);
            IValidatedVolumeDay validatedVolumeDay2 = new ValidatedVolumeDay(_workload, _period.StartDate.AddDays(1));
            IValidatedVolumeDay validatedVolumeDay3 = new ValidatedVolumeDay(_workload, _period.StartDate.AddDays(2));
            IList<ITaskOwner> validatedVolumeDayList = new List<ITaskOwner>
                                                                   {
                validatedVolumeDay1,
                validatedVolumeDay2,
                validatedVolumeDay3};

            IList<IStatisticTask> emptyStatisticTaskList = new List<IStatisticTask>();
            IList<IValidatedVolumeDay> emptyValidatedVolumeDayList = new List<IValidatedVolumeDay>();

            Expect.Call(_statisticTaskRep.LoadSpecificDates(_workload.QueueSourceCollection, period.ChangeEndTime(TimeSpan.FromHours(25)))).
                Return(emptyStatisticTaskList).Repeat.Once();
            Expect.Call(_validatedVolumeDayRep.FindRange(_period, _workload)).
                Return(emptyValidatedVolumeDayList).Repeat.Once();
            Expect.Call(_validatedVolumeDayRep.MatchDays(_workload, null, emptyValidatedVolumeDayList, true)).IgnoreArguments().
                Return(validatedVolumeDayList).Repeat.Once();

            mocks.ReplayAll();

            target = new StatisticHelper(_factory, _unitOfWork);

            int statusChangedCount = 0;
            target.StatusChanged += (x, y) =>
            {
                statusChangedCount++;
            };

            IList<ITaskOwner> result = target.GetWorkloadDaysWithValidatedStatistics(_period, _workload, _scenario, new List<IValidatedVolumeDay>());
            Assert.AreEqual(3, result.Count);
            Assert.AreSame(validatedVolumeDay1, result[0]);
            Assert.AreSame(validatedVolumeDay2, result[1]);
            Assert.AreSame(validatedVolumeDay3, result[2]);
            Assert.AreEqual(0, statusChangedCount);
            mocks.VerifyAll();
        }

        /// <summary>
        /// Verifies the get workload days with statistics using existing validated volume days.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-21
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyGetWorkloadDaysWithStatisticsUsingExistingValidatedVolumeDays()
        {
            var period = _period.ToDateTimePeriod(_skill.TimeZone);
            IValidatedVolumeDay validatedVolumeDay1 = new ValidatedVolumeDay(_workload, new DateOnly(2008, 7, 16));
            IList<IValidatedVolumeDay> validatedVolumeDayList = new List<IValidatedVolumeDay>
                                                                   {
                validatedVolumeDay1};

            IValidatedVolumeDay existingValidatedVolumeDay = new ValidatedVolumeDay(_workload, new DateOnly(2008, 7, 16));
            IList<IStatisticTask> emptyStatisticTaskList = new List<IStatisticTask>();

            Expect.Call(_statisticTaskRep.LoadSpecificDates(_workload.QueueSourceCollection, period.ChangeEndTime(TimeSpan.FromHours(25)))).
                Return(emptyStatisticTaskList).Repeat.Once();
            Expect.Call(_validatedVolumeDayRep.FindRange(_period, _workload)).
                Return(validatedVolumeDayList).Repeat.Once();
            Expect.Call(_validatedVolumeDayRep.MatchDays(_workload, null, null, false)).IgnoreArguments().
                Return(new List<ITaskOwner> { existingValidatedVolumeDay }).Repeat.Once();

            mocks.ReplayAll();

            target = new StatisticHelper(_factory, _unitOfWork);

            int statusChangedCount = 0;
            target.StatusChanged += (x, y) =>
            {
                statusChangedCount++;
            };

            IList<ITaskOwner> result = target.GetWorkloadDaysWithValidatedStatistics(_period, _workload, _scenario, new List<IValidatedVolumeDay> { existingValidatedVolumeDay });
            Assert.AreEqual(1, result.Count);
            Assert.AreSame(existingValidatedVolumeDay, result[0]);
            Assert.AreEqual(0, statusChangedCount);
            mocks.VerifyAll();
        }

        /// <summary>
        /// Verifies the load statistics with skill works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-08
        /// </remarks>
        [Test]
        public void VerifyLoadStatisticsWithSkillWorks()
        {
            var period = _period.ToDateTimePeriod(_skill.TimeZone);
            ISkillDay skillDay1 = SkillDayFactory.CreateSkillDay(_skill, period.StartDateTime, _workload, _workload);
            IList<ISkillDay> skillDayList = new List<ISkillDay>
                                                {
                skillDay1};

            IList<IStatisticTask> emptyStatisticTaskList = new List<IStatisticTask>();

            Expect.Call(_skillDayRep.FindRange(_period, _skill, _scenario)).
                Return(skillDayList).Repeat.Once();
			Expect.Call(_skillDayRep.GetAllSkillDays(_period, skillDayList, _skill, _scenario, _skillDayRep.AddRange)).
                Return(skillDayList).Repeat.Once().IgnoreArguments();
            Expect.Call(_statisticTaskRep.LoadSpecificDates(_workload.QueueSourceCollection, period)).
                Return(emptyStatisticTaskList).Repeat.Once();

            mocks.ReplayAll();

            target = new StatisticHelper(_factory, _unitOfWork);

            int statusChangedCount = 0;
            target.StatusChanged += (x, y) =>
            {
                statusChangedCount++;
            };

            IList<ISkillDay> result = target.LoadStatisticData(_period, _skill, _scenario, true);
            mocks.VerifyAll();
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(5, statusChangedCount);
        }

        /// <summary>
        /// Verifies the load statistics with workload works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-08
        /// </remarks>
        [Test]
        public void VerifyLoadStatisticsWithWorkloadWorks()
        {
            var period = _period.ToDateTimePeriod(_skill.TimeZone);
            IList<IStatisticTask> emptyStatisticTaskList = new List<IStatisticTask>();
            Expect.Call(_statisticTaskRep.LoadSpecificDates(_workload.QueueSourceCollection, period.ChangeEndTime(TimeSpan.FromHours(25)))).
                Return(emptyStatisticTaskList).Repeat.Once();

            mocks.ReplayAll();

            target = new StatisticHelper(_factory, _unitOfWork);

            int statusChangedCount = 0;
            target.StatusChanged += (x, y) =>
            {
                statusChangedCount++;
            };

            IList<IWorkloadDayBase> result = target.LoadStatisticData(_period, _workload);
            mocks.VerifyAll();
            Assert.AreEqual(4, result.Count);
            Assert.AreEqual(0, statusChangedCount);
        }

        /// <summary>
        /// Verifies the can exclude outliers from workload days.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-16
        /// </remarks>
        [Test]
        public void VerifyCanExcludeOutliersFromWorkloadDays()
        {
            _skill.TimeZone = (TimeZoneInfo.Utc);
            IList<IOutlier> outliers = new List<IOutlier>();
            IList<ITaskOwner> workloadDays = new List<ITaskOwner>();

            IOutlier outlier1 = new Outlier(new Description("Outlier1"));
            outlier1.AddDate(_period.StartDate);
            outlier1.AddDate(_period.StartDate.AddDays(4));

            IOutlier outlier2 = new Outlier(new Description("Outlier2"));
            outlier2.AddDate(_period.StartDate.AddDays(3));


            outliers.Add(outlier1);
            outliers.Add(outlier2);

            for (int i = 0; i < 6; i++)
            {
                WorkloadDay workloadDay = new WorkloadDay();
                workloadDay.Create(_period.StartDate.AddDays(i), _workload, new List<TimePeriod>());
                workloadDays.Add(workloadDay);
            }

            Assert.AreEqual(6, workloadDays.Count);
            Assert.AreEqual(2, outliers.Count);
            workloadDays = StatisticHelper.ExcludeOutliersFromStatistics(new DateOnlyPeriod(_period.StartDate, _period.EndDate.AddDays(2)),
                outliers, workloadDays);
            Assert.AreEqual(3, workloadDays.Count);
            Assert.AreEqual(_period.StartDate.AddDays(1), workloadDays[0].CurrentDate);
            Assert.AreEqual(_period.StartDate.AddDays(2), workloadDays[1].CurrentDate);
            Assert.AreEqual(_period.StartDate.AddDays(5), workloadDays[2].CurrentDate);
        }
    }
}