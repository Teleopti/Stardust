using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;


namespace Teleopti.Ccc.DomainTest.Forecasting.Template
{
     /// <summary>
    /// Tests for ClassName
    /// </summary>
    [TestFixture]
    public class TemplateTaskPeriodTest
    {
        private ITemplateTaskPeriod _target;
        private Task _task;
        private Campaign _campaign;
        private DateTimePeriod _timePeriod;
        private DateTime _date = new DateTime(2008, 2, 1, 0, 0, 0, DateTimeKind.Utc);

        [SetUp]
        public void Setup()
        {
            _task = new Task();
            _campaign = new Campaign(
                new Percent(0.10d),
                new Percent(0.15d),
                new Percent(0.2d));
            DateTime tsStart = _date.Add(TimeSpan.FromHours(12));
            DateTime tsEnd = _date.Add(TimeSpan.FromDays(1));
            _timePeriod = new DateTimePeriod(tsStart, tsEnd);
            _target = new TemplateTaskPeriod(_task, _timePeriod);
        }

        /// <summary>
        /// Verifies the constructor with campaign works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-04
        /// </remarks>
        [Test]
        public void VerifyConstructorWithCampaignWorks()
        {
			  _target = new TemplateTaskPeriod(_task, _campaign, _timePeriod);
            Assert.IsNotNull(_target);
            Assert.AreEqual(_task, _target.Task);
            Assert.AreEqual(_campaign, _target.Campaign);
            Assert.AreEqual(_timePeriod, _target.Period);
        }

        [Test]
        public void CanGetProperties()
        {
            Assert.AreEqual(_target.Task, _task);
            Assert.AreEqual(_target.Period, _timePeriod);
            Assert.AreEqual(new Campaign(), _target.Campaign);
        }

        [Test]
        public void VerifyStatisticTaskIsNotNull()
        {
            Assert.IsNotNull(_target.StatisticTask);
        }

        /// <summary>
        /// Verifies the statistic information works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-03
        /// </remarks>
        [Test]
        public void VerifyStatisticInformationWorks()
        {
            Assert.AreEqual(0d, _target.TotalStatisticCalculatedTasks);
            Assert.AreEqual(0d, _target.TotalStatisticAbandonedTasks);
            Assert.AreEqual(0d, _target.TotalStatisticAnsweredTasks);
            Assert.AreEqual(TimeSpan.Zero, _target.TotalStatisticAverageTaskTime);
            Assert.AreEqual(TimeSpan.Zero, _target.TotalStatisticAverageAfterTaskTime);
        }

        [Test]
        public void VerifyNoPublicEmptyConstructor()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(_target.GetType()));
        }

        [Test]
        public void CanChangeTasksInTaskPeriod()
        {
            _task = new Task();
            _target = new TemplateTaskPeriod(_task, _timePeriod);
            _target.SetTasks(321);

            Assert.AreEqual(321, _target.Task.Tasks);
        }

        [Test]
        public void VerifyAverageTaskTimeCanBeZero()
        {
            _task = new Task();
            _target = new TemplateTaskPeriod(_task, _timePeriod);
            _target.AverageTaskTime = TimeSpan.Zero;

            Assert.AreEqual(TimeSpan.Zero, _target.AverageTaskTime);
        }

        [Test]
        public void ShouldAllowOneHundredHoursAsMaximumForTaskTime()
        {
            _task = new Task();
            _target = new TemplateTaskPeriod(_task, _timePeriod);
            _target.AverageTaskTime = TimeSpan.FromHours(101);

            Assert.AreEqual(TimeSpan.FromHours(100), _target.AverageTaskTime);
        }

        [Test]
        public void ShouldAllowOneHundredHoursAsMaximumForAfterTaskTime()
        {
            _task = new Task();
            _target = new TemplateTaskPeriod(_task, _timePeriod);
            _target.AverageAfterTaskTime = TimeSpan.FromHours(101);

            Assert.AreEqual(TimeSpan.FromHours(100), _target.AverageAfterTaskTime);
        }

        [Test]
        public void VerifyAverageAfterTaskTimeCanBeZero()
        {
            _task = new Task();
            _target = new TemplateTaskPeriod(_task, _timePeriod);
            _target.AverageAfterTaskTime = TimeSpan.Zero;

            Assert.AreEqual(TimeSpan.Zero, _target.AverageAfterTaskTime);
        }

        [Test]
        public void VerifyTasksCanBeZero()
        {
            _task = new Task();
            _target = new TemplateTaskPeriod(_task, _timePeriod);
            _target.Tasks = 0d;

            Assert.AreEqual(0d, _target.Tasks);
        }

        /// <summary>
        /// Verifies the recalculate daily average times not implemented.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-22
        /// </remarks>
        [Test]
        public void VerifyRecalculateDailyAverageTimesNotImplemented()
        {
			Assert.Throws<NotImplementedException>(() => _target.RecalculateDailyAverageTimes());
        }

        /// <summary>
        /// Verifies the recalculate daily tasks not implemented.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-22
        /// </remarks>
        [Test]
       
        public void VerifyRecalculateDailyTasksNotImplemented()
        {
			Assert.Throws<NotImplementedException>(() => _target.RecalculateDailyTasks());
        }

        /// <summary>
        /// Verifies the recalculate daily statistic tasks not implemented.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-03
        /// </remarks>
        [Test]
        public void VerifyRecalculateDailyStatisticTasksNotImplemented()
        {
			Assert.Throws<NotImplementedException>(() => _target.RecalculateDailyStatisticTasks());
        }

        /// <summary>
        /// Verifies the recalculate daily average statistic times not implemented.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-03
        /// </remarks>
        [Test]
        public void VerifyRecalculateDailyAverageStatisticTimesNotImplemented()
        {
			Assert.Throws<NotImplementedException>(() => _target.RecalculateDailyAverageStatisticTimes());
        }

        /// <summary>
        /// Verifies the recalculate daily average campaign times not implemented.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-04
        /// </remarks>
        [Test]
        public void VerifyRecalculateDailyAverageCampaignTimesNotImplemented()
        {
			Assert.Throws<NotImplementedException>(() => _target.RecalculateDailyAverageCampaignTimes());
        }

        /// <summary>
        /// Verifies the recalculate daily campaign tasks not implemented.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-04
        /// </remarks>
        [Test]
        public void VerifyRecalculateDailyCampaignTasksNotImplemented()
        {
			Assert.Throws<NotImplementedException>(() => _target.RecalculateDailyCampaignTasks());
        }

        /// <summary>
        /// Verifies the lock and release.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-22
        /// </remarks>
        [Test]
        public void VerifyLockAndRelease()
        {
            _target.Lock();
            _target.Release();
        }

        /// <summary>
        /// Verifies the set dirty.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-23
        /// </remarks>
        [Test]
        public void VerifySetDirty()
        {
            _target.SetDirty();
        }

        /// <summary>
        /// Verifies the current date.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-28
        /// </remarks>
        [Test]
        public void VerifyCurrentDate()
        {
            //CurrentDate is irrelevant for TemplateTaskPeriod, for now anyway
            Assert.AreEqual(DateOnly.MinValue, _target.CurrentDate);
        }

        /// <summary>
        /// Verifies the is closed.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-28
        /// </remarks>
        [Test]
        public void VerifyIsClosed()
        {
            //Assert.IsFalse(_target.IsClosed);
            Assert.IsTrue(_target.OpenForWork.IsOpen);
        }

        /// <summary>
        /// Verifies the is locked.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-28
        /// </remarks>
        [Test]
        public void VerifyIsLocked()
        {
            Assert.IsFalse(_target.IsLocked);
        }

        /// <summary>
        /// Verifies the add parent gives exception.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-28
        /// </remarks>
        [Test]
        public void VerifyAddParentGivesException()
        {
            _target.AddParent(null);
            Assert.IsNotNull(_target);
        }

        /// <summary>
        /// Verifies the remove parent gives exception.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-28
        /// </remarks>
        [Test]
        public void VerifyRemoveParentGivesException()
        {
			Assert.Throws<NotImplementedException>(() => _target.RemoveParent(null));
        }

        /// <summary>
        /// Verifies the update template name gives exception.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-20
        /// </remarks>
        [Test]
        public void VerifyUpdateTemplateNameGivesException()
        {
			Assert.Throws<NotImplementedException>(() => _target.ClearTemplateName());
        }

        /// <summary>
        /// Verifies the campaign tasks works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-04
        /// </remarks>
        [Test]
        public void VerifyCampaignTasksWorks()
        {
            Percent percent = new Percent(0.2d);

            _target.SetTasks(100d);

            Assert.AreEqual(100d, _target.Tasks);
            Assert.AreEqual(100d, _target.TotalTasks);

            _target.CampaignTasks = percent;

            Assert.AreEqual(percent, _target.CampaignTasks);
            Assert.AreEqual(100d, _target.Tasks);
            Assert.AreEqual(120d, _target.TotalTasks);
        }

        /// <summary>
        /// Verifies the campaign task times works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-04
        /// </remarks>
        [Test]
        public void VerifyCampaignTaskTimesWorks()
        {
            Percent percent = new Percent(0.2d);
            TimeSpan time = TimeSpan.FromSeconds(10d);

            _target.AverageTaskTime = time;

            Assert.AreEqual(time, _target.AverageTaskTime);
            Assert.AreEqual(time, _target.TotalAverageTaskTime);

            _target.CampaignTaskTime = percent;

            Assert.AreEqual(percent, _target.CampaignTaskTime);
            Assert.AreEqual(time, _target.AverageTaskTime);
            Assert.AreEqual(time.Add(TimeSpan.FromSeconds(2)), _target.TotalAverageTaskTime);
        }

        /// <summary>
        /// Verifies the campaign after task times works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-04
        /// </remarks>
        [Test]
        public void VerifyCampaignAfterTaskTimesWorks()
        {
            Percent percent = new Percent(0.2d);
            TimeSpan time = TimeSpan.FromSeconds(10d);

            _target.AverageAfterTaskTime = time;

            Assert.AreEqual(time, _target.AverageAfterTaskTime);
            Assert.AreEqual(time, _target.TotalAverageAfterTaskTime);

            _target.CampaignAfterTaskTime = percent;

            Assert.AreEqual(percent, _target.CampaignAfterTaskTime);
            Assert.AreEqual(time, _target.AverageAfterTaskTime);
            Assert.AreEqual(time.Add(TimeSpan.FromSeconds(2)), _target.TotalAverageAfterTaskTime);
        }

        /// <summary>
        /// Verifies the can reset data.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-17
        /// </remarks>
        [Test]
        public void VerifyCanResetData()
        {
            _target.Tasks = 2d;
            _target.AverageTaskTime = TimeSpan.FromSeconds(10d);
            _target.AverageAfterTaskTime = TimeSpan.FromSeconds(20d);

            _target.ResetTaskOwner();

            Assert.AreEqual(0d, _target.Tasks);
            Assert.AreEqual(TimeSpan.Zero,_target.AverageTaskTime);
            Assert.AreEqual(TimeSpan.Zero, _target.AverageAfterTaskTime);
        }

        [Test]
        public void VerifyCanAddToTotalTasks()
        {
            _target.Tasks = 8d;
            _target.CampaignTasks = new Percent(0.10d);
            _target.AggregatedTasks = 2d;

            Assert.AreEqual(8.8d,_target.TotalTasks);
            Assert.AreEqual(2d, _target.AggregatedTasks);
        }

        [Test]
        public void CanClone()
        {
            _target.SetId(Guid.NewGuid());
            ITemplateTaskPeriod taskPeriodClone = (ITemplateTaskPeriod)_target.Clone();
            Assert.IsFalse(taskPeriodClone.Id.HasValue);
            Assert.AreEqual(_target.AggregatedTasks, taskPeriodClone.AggregatedTasks);
            Assert.AreEqual(_target.Task, taskPeriodClone.Task);
            Assert.AreEqual(_target.Campaign, taskPeriodClone.Campaign);
            Assert.AreEqual(_target.Period, taskPeriodClone.Period);
            
            taskPeriodClone = _target.NoneEntityClone();
            Assert.IsFalse(taskPeriodClone.Id.HasValue);
            Assert.AreEqual(_target.AggregatedTasks, taskPeriodClone.AggregatedTasks);
            Assert.AreEqual(_target.Task, taskPeriodClone.Task);
            Assert.AreEqual(_target.Campaign, taskPeriodClone.Campaign);
            Assert.AreEqual(_target.Period, taskPeriodClone.Period);
            
            taskPeriodClone = _target.EntityClone();
            Assert.AreEqual(_target.Id.Value, taskPeriodClone.Id.Value);
            Assert.AreEqual(_target.AggregatedTasks, taskPeriodClone.AggregatedTasks);
            Assert.AreEqual(_target.Task, taskPeriodClone.Task);
            Assert.AreEqual(_target.Campaign, taskPeriodClone.Campaign);
            Assert.AreEqual(_target.Period, taskPeriodClone.Period);
        }

        [Test]
        public void VerifySplitTemplateTaskPeriodWithLowerPeriodLength()
        {
            // try to split 30 minutes in 60, can't be done
            DateTime start = new DateTime(2009, 02, 02, 09, 0, 0, DateTimeKind.Utc);
            DateTime end = new DateTime(2009, 02, 02, 09, 30, 0, DateTimeKind.Utc);


            _timePeriod = new DateTimePeriod(start, end);
            _target = new TemplateTaskPeriod(_task, _timePeriod);

			Assert.Throws<ArgumentOutOfRangeException>(() => _target.Split(new TimeSpan(1, 0, 0)));
        }

        [Test]
        public void VerifySplitTemplateTaskPeriodWithUnevenPeriodLengths()
        {
            // try to split 35 minutes in 15 can't be done
            DateTime start = new DateTime(2009, 02, 02, 09, 0, 0, DateTimeKind.Utc);
            DateTime end = new DateTime(2009, 02, 02, 09, 35, 0, DateTimeKind.Utc);

           
            _timePeriod = new DateTimePeriod(start, end);
            _target = new TemplateTaskPeriod(_task, _timePeriod);

			Assert.Throws<ArgumentOutOfRangeException>(() => _target.Split(new TimeSpan(0, 15, 0)));
        }

        [Test]
        public void VerifySplitTemplateTaskPeriod()
        {
            TimeSpan averageTaskTime = new TimeSpan(0, 3, 20);
            TimeSpan averageAfterTaskTime = new TimeSpan(0, 1, 30);
            _task = new Task(80, averageTaskTime, averageAfterTaskTime);
            
            DateTime tsStart = _date;
            DateTime tsEnd = _date.Add(TimeSpan.FromMinutes(30));
            _timePeriod = new DateTimePeriod(tsStart, tsEnd);
            _target = new TemplateTaskPeriod(_task, _timePeriod);

            Percent campaign = new Percent(0.10d);
            _target.CampaignTasks = campaign;
            Percent campaignAfter = new Percent(0.20d);
            _target.CampaignAfterTaskTime = campaignAfter;
            _target.CampaignTaskTime = campaign;

            IList<ITemplateTaskPeriodView> views = _target.Split(new TimeSpan(0, 15, 0));
            Assert.AreEqual(2,views.Count);
            Assert.AreEqual(40,views[0].Tasks);
            Assert.AreEqual(40, views[1].Tasks);
            Assert.AreEqual(44, views[0].TotalTasks);
            Assert.AreEqual(44, views[1].TotalTasks);
            Assert.AreEqual(averageTaskTime, views[0].AverageTaskTime);
            Assert.AreEqual(averageTaskTime, views[1].AverageTaskTime);
            Assert.AreEqual(averageAfterTaskTime, views[0].AverageAfterTaskTime);
            Assert.AreEqual(averageAfterTaskTime, views[1].AverageAfterTaskTime);
            Assert.AreEqual(4, views[0].CampaignTasks);
            Assert.AreEqual(4, views[1].CampaignTasks);
            Assert.AreEqual(campaign,views[0].CampaignTaskTime);
            Assert.AreEqual(campaign, views[1].CampaignTaskTime);
            Assert.AreEqual(campaignAfter, views[0].CampaignAfterTaskTime);
            Assert.AreEqual(campaignAfter, views[1].CampaignAfterTaskTime);
            Assert.AreEqual(_target.TotalAverageTaskTime, views[0].TotalAverageTaskTime);
            Assert.AreEqual(_target.TotalAverageTaskTime, views[1].TotalAverageTaskTime);
            Assert.AreEqual(_target.TotalAverageAfterTaskTime, views[0].TotalAverageAfterTaskTime);
            Assert.AreEqual(_target.TotalAverageAfterTaskTime, views[1].TotalAverageAfterTaskTime);

            Assert.AreEqual(_target.Parent, views[0].Parent);
            Assert.AreEqual(_target.Parent, views[1].Parent);

            Assert.AreEqual(15, views[0].Period.ElapsedTime().TotalMinutes);
            Assert.AreEqual(15, views[1].Period.ElapsedTime().TotalMinutes);
        }

		[Test]
		public void ShouldNotAllowNegativeValuesInTasks()
		{
			_target.Tasks = 1;
			Assert.AreEqual(1, _target.Tasks);

			_target.Tasks = -2;
			Assert.AreEqual(1, _target.Tasks);
		}

		[Test]
		public void ShouldNotAllowNegativeValuesInAverageTaskTime()
		{
			_target.AverageTaskTime = TimeSpan.FromHours(1);
			Assert.AreEqual(TimeSpan.FromHours(1), _target.AverageTaskTime);

			_target.AverageTaskTime = TimeSpan.FromHours(-2);
			Assert.AreEqual(TimeSpan.FromHours(1), _target.AverageTaskTime);
		}

		[Test]
		public void ShouldNotAllowNegativeValuesInAverageAfterTaskTime()
		{
			_target.AverageAfterTaskTime = TimeSpan.FromHours(1);
			Assert.AreEqual(TimeSpan.FromHours(1), _target.AverageAfterTaskTime);

			_target.AverageAfterTaskTime = TimeSpan.FromHours(-2);
			Assert.AreEqual(TimeSpan.FromHours(1), _target.AverageAfterTaskTime);
		}
    }
}
