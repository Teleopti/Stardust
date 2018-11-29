using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
    /// <summary>
    /// Tests for the ValidatedVolumeDay class
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-03-27
    /// </remarks>
    [TestFixture]
    public class ValidatedVolumeDayTest
    {
        ValidatedVolumeDay target;
        IWorkload _workload;
        DateOnly _date;
        MockRepository mocks;

        /// <summary>
        /// Setups this instance.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-27
        /// </remarks>
        [SetUp]
        public void Setup()
        {
            ISkill skill = SkillFactory.CreateSkill("testSkill");
            _workload = WorkloadFactory.CreateWorkload(skill);
            _date = new DateOnly(2007,8,1);
            target = new ValidatedVolumeDay(_workload,_date);
            mocks = new MockRepository();
        }

        /// <summary>
        /// Tears the down.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        [TearDown]
        public void Teardown()
        {
            mocks.VerifyAll();
        }

        /// <summary>
        /// Verifies the empty constructor.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-27
        /// </remarks>
        [Test]
        public void VerifyEmptyConstructor()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(target.GetType()));
        }

        /// <summary>
        /// Verifies the instance is created and properties.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-27
        /// </remarks>
        [Test]
        public void VerifyInstanceIsCreatedAndProperties()
        {
            Assert.IsNotNull(target);
            Assert.AreEqual(_workload,target.Workload);
            Assert.AreEqual(_date, target.VolumeDayDate);
        }

        /// <summary>
        /// Verifies the workload cannot be null.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-27
        /// </remarks>
        [Test]
        public void VerifyWorkloadCannotBeNull()
        {
			Assert.Throws<ArgumentNullException>(() => target = new ValidatedVolumeDay(null, _date));
        }

        /// <summary>
        /// Verifies the validated task has values.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        [Test]
        public void VerifyValidatedTaskHasValues()
        {
            Assert.IsFalse(target.HasValues);
            target.ValidatedTasks = 10d;
            Assert.IsTrue(target.HasValues);
        }

        /// <summary>
        /// Verifies the validated average task time has values.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        [Test]
        public void VerifyValidatedAverageTaskTimeHasValues()
        {
            Assert.IsFalse(target.HasValues);
            target.ValidatedAverageTaskTime = TimeSpan.FromSeconds(10d);
            Assert.IsTrue(target.HasValues);
        }

        /// <summary>
        /// Verifies the validated average after task time has values.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        [Test]
        public void VerifyValidatedAverageAfterTaskTimeHasValues()
        {
            Assert.IsFalse(target.HasValues);
            target.ValidatedAverageAfterTaskTime = TimeSpan.FromSeconds(10d);
            Assert.IsTrue(target.HasValues);
        }

        /// <summary>
        /// Verifies the validated tasks.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-27
        /// </remarks>
        [Test]
        public void VerifyValidatedTasks()
        {
            ITaskOwner taskOwnerMock = mocks.StrictMock<ITaskOwner>();

            SetupResult.For(taskOwnerMock.TotalStatisticCalculatedTasks).Return(25d).Repeat.Once();

            mocks.ReplayAll();

            Assert.AreEqual(0d, target.ValidatedTasks);
            target.TaskOwner = taskOwnerMock;
            Assert.AreEqual(25d, target.ValidatedTasks);
            target.ValidatedTasks = 21d;
            Assert.AreEqual(21d, target.ValidatedTasks);
        }

        /// <summary>
        /// Verifies the validated average task time.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-27
        /// </remarks>
        [Test]
        public void VerifyValidatedAverageTaskTime()
        {
            ITaskOwner taskOwnerMock = mocks.StrictMock<ITaskOwner>();

            SetupResult.For(taskOwnerMock.TotalStatisticAverageTaskTime).Return(TimeSpan.FromSeconds(35d)).Repeat.Once();

            mocks.ReplayAll();

            Assert.AreEqual(TimeSpan.Zero, target.ValidatedAverageTaskTime);
            target.TaskOwner = taskOwnerMock;
            Assert.AreEqual(TimeSpan.FromSeconds(35d), target.ValidatedAverageTaskTime);
            target.ValidatedAverageTaskTime = TimeSpan.FromSeconds(22d);
            Assert.AreEqual(TimeSpan.FromSeconds(22d), target.ValidatedAverageTaskTime);
        }

        /// <summary>
        /// Verifies the validated average after task time.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-27
        /// </remarks>
        [Test]
        public void VerifyValidatedAverageAfterTaskTime()
        {
            ITaskOwner taskOwnerMock = mocks.StrictMock<ITaskOwner>();

            SetupResult.For(taskOwnerMock.TotalStatisticAverageAfterTaskTime).Return(TimeSpan.FromSeconds(45d)).Repeat.Once();

            mocks.ReplayAll();

            Assert.AreEqual(TimeSpan.Zero, target.ValidatedAverageAfterTaskTime);
            target.TaskOwner = taskOwnerMock;
            Assert.AreEqual(TimeSpan.FromSeconds(45d), target.ValidatedAverageAfterTaskTime);
            target.ValidatedAverageAfterTaskTime = TimeSpan.FromSeconds(32d);
            Assert.AreEqual(TimeSpan.FromSeconds(32d), target.ValidatedAverageAfterTaskTime);
        }

        /// <summary>
        /// Verifies the skill day property works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-27
        /// </remarks>
        [Test]
        public void VerifySkillDayPropertyWorks()
        {
            ITaskOwner taskOwnerMock = mocks.StrictMock<ITaskOwner>();

            mocks.ReplayAll();

            target.TaskOwner = taskOwnerMock;

            Assert.AreSame(taskOwnerMock, target.TaskOwner);
        }

        /// <summary>
        /// Verifies the total statistics.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        [Test]
        public void VerifyTotalStatistics()
        {
            ITaskOwner taskOwnerMock = mocks.StrictMock<ITaskOwner>();

            Expect.Call(taskOwnerMock.TotalStatisticAbandonedTasks).Return(210d).Repeat.Twice();
            Expect.Call(taskOwnerMock.TotalStatisticAnsweredTasks).Return(220d).Repeat.Twice();
            Expect.Call(taskOwnerMock.TotalStatisticCalculatedTasks).Return(230d).Repeat.Once();
            Expect.Call(taskOwnerMock.TotalStatisticAverageTaskTime).Return(TimeSpan.FromSeconds(20d)).Repeat.Once();
            Expect.Call(taskOwnerMock.TotalStatisticAverageAfterTaskTime).Return(TimeSpan.FromSeconds(240d)).Repeat.Once();

            mocks.ReplayAll();

            target.TaskOwner = taskOwnerMock;
            Assert.AreEqual(210d, target.TotalStatisticAbandonedTasks);
            Assert.AreEqual(220d, target.TotalStatisticAnsweredTasks);
            Assert.AreEqual(230d, target.TotalStatisticCalculatedTasks);
            Assert.AreEqual(TimeSpan.FromSeconds(20d), target.TotalStatisticAverageTaskTime);
            Assert.AreEqual(TimeSpan.FromSeconds(240d), target.TotalStatisticAverageAfterTaskTime);

            target.ValidatedTasks = 110;
            target.ValidatedAverageTaskTime = TimeSpan.FromSeconds(40);
            target.ValidatedAverageAfterTaskTime = TimeSpan.FromSeconds(300);

            Assert.AreEqual(210d, target.TotalStatisticAbandonedTasks);
            Assert.AreEqual(220d, target.TotalStatisticAnsweredTasks);
            Assert.AreEqual(110d, target.TotalStatisticCalculatedTasks);
            Assert.AreEqual(TimeSpan.FromSeconds(40d), target.TotalStatisticAverageTaskTime);
            Assert.AreEqual(TimeSpan.FromSeconds(300d), target.TotalStatisticAverageAfterTaskTime);
        }

		[Test]
		public void ShouldNotAcceptNegativeValuesInValidatedAverageAfterTaskTime()
		{
			target.ValidatedAverageAfterTaskTime = TimeSpan.FromHours(1);
			Assert.AreEqual(TimeSpan.FromHours(1), target.ValidatedAverageAfterTaskTime);

			target.ValidatedAverageAfterTaskTime = TimeSpan.FromHours(-2);
			Assert.AreEqual(TimeSpan.FromHours(1), target.ValidatedAverageAfterTaskTime);
		}

		[Test]
		public void ShouldNotAcceptNegativeValuesInValidatedAverageTaskTime()
		{
			target.ValidatedAverageTaskTime = TimeSpan.FromHours(1);
			Assert.AreEqual(TimeSpan.FromHours(1), target.ValidatedAverageTaskTime);

			target.ValidatedAverageTaskTime = TimeSpan.FromHours(-2);
			Assert.AreEqual(TimeSpan.FromHours(1), target.ValidatedAverageTaskTime);
		}

		[Test]
		public void ShouldNotAcceptNegativeValuesInValidatedTasks()
		{
			target.ValidatedTasks = 1;
			Assert.AreEqual(1, target.ValidatedTasks);

			target.ValidatedTasks = -2;
			Assert.AreEqual(1, target.ValidatedTasks);
		}


        #region ITaskOwner tests

        [Test]
        public void CanReset()
        {
			Assert.Throws<NotImplementedException>(() => target.ResetTaskOwner());
        }
        /// <summary>
        /// Verifies the total tasks.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        [Test]
        public void VerifyTotalTasks()
        {
            ITaskOwner taskOwnerMock = mocks.StrictMock<ITaskOwner>();

            double value = 10d;
            Expect.Call(taskOwnerMock.TotalTasks).Return(value).Repeat.Once();

            mocks.ReplayAll();

            target.TaskOwner = taskOwnerMock;
            Assert.AreEqual(value, target.TotalTasks);
        }

        /// <summary>
        /// Verifies the total average task time.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        [Test]
        public void VerifyTotalAverageTaskTime()
        {
            ITaskOwner taskOwnerMock = mocks.StrictMock<ITaskOwner>();

            TimeSpan time = TimeSpan.FromSeconds(60);
            Expect.Call(taskOwnerMock.TotalAverageTaskTime).Return(time).Repeat.Once();

            mocks.ReplayAll();

            target.TaskOwner = taskOwnerMock;
            Assert.AreEqual(time, target.TotalAverageTaskTime);
        }

        /// <summary>
        /// Verifies the total average after task time.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        [Test]
        public void VerifyTotalAverageAfterTaskTime()
        {
            ITaskOwner taskOwnerMock = mocks.StrictMock<ITaskOwner>();

            TimeSpan time = TimeSpan.FromSeconds(70);
            Expect.Call(taskOwnerMock.TotalAverageAfterTaskTime).Return(time).Repeat.Once();

            mocks.ReplayAll();

            target.TaskOwner = taskOwnerMock;
            Assert.AreEqual(time, target.TotalAverageAfterTaskTime);
        }

        /// <summary>
        /// Verifies the campaign tasks.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        [Test]
        public void VerifyCampaignTasks()
        {
            ITaskOwner taskOwnerMock = mocks.StrictMock<ITaskOwner>();

            Percent percent = new Percent(0.2);
            Expect.Call(taskOwnerMock.CampaignTasks).Return(percent).Repeat.Once();

            taskOwnerMock.CampaignTasks = new Percent(percent.Value + 0.1);
            LastCall.Repeat.Once();

            mocks.ReplayAll();

            target.TaskOwner = taskOwnerMock;
            target.CampaignTasks = new Percent(percent.Value + 0.1);
            
            Assert.AreEqual(percent, target.CampaignTasks);
        }

        /// <summary>
        /// Verifies the campaign task time.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        [Test]
        public void VerifyCampaignTaskTime()
        {
            ITaskOwner taskOwnerMock = mocks.StrictMock<ITaskOwner>();

            Percent percent = new Percent(0.25);
            Expect.Call(taskOwnerMock.CampaignTaskTime).Return(percent).Repeat.Once();

            taskOwnerMock.CampaignTaskTime = new Percent(percent.Value + 0.1);
            LastCall.Repeat.Once();

            mocks.ReplayAll();

            target.TaskOwner = taskOwnerMock;
            target.CampaignTaskTime = new Percent(percent.Value + 0.1);

            Assert.AreEqual(percent, target.CampaignTaskTime);
        }

        /// <summary>
        /// Verifies the campaign after task time.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        [Test]
        public void VerifyCampaignAfterTaskTime()
        {
            ITaskOwner taskOwnerMock = mocks.StrictMock<ITaskOwner>();

            Percent percent = new Percent(0.3);
            Expect.Call(taskOwnerMock.CampaignAfterTaskTime).Return(percent).Repeat.Once();

            taskOwnerMock.CampaignAfterTaskTime = new Percent(percent.Value + 0.1);
            LastCall.Repeat.Once();

            mocks.ReplayAll();

            target.TaskOwner = taskOwnerMock;
            target.CampaignAfterTaskTime = new Percent(percent.Value + 0.1);

            Assert.AreEqual(percent, target.CampaignAfterTaskTime);
        }

        /// <summary>
        /// Verifies the tasks.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        [Test]
        public void VerifyTasks()
        {
            ITaskOwner taskOwnerMock = mocks.StrictMock<ITaskOwner>();
            
            double value = 40d;
            Expect.Call(taskOwnerMock.Tasks).Return(value).Repeat.Once();

            taskOwnerMock.Tasks = value + 0.1;
            LastCall.Repeat.Once();

            mocks.ReplayAll();

            target.TaskOwner = taskOwnerMock;
            target.Tasks = value + 0.1;

            Assert.AreEqual(value, target.Tasks);
        }

        /// <summary>
        /// Verifies the average task time.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        [Test]
        public void VerifyAverageTaskTime()
        {
            ITaskOwner taskOwnerMock = mocks.StrictMock<ITaskOwner>();

            TimeSpan value = TimeSpan.FromSeconds(40d);
            Expect.Call(taskOwnerMock.AverageTaskTime).Return(value).Repeat.Once();
            Expect.Call(taskOwnerMock.AverageAfterTaskTime).Return(value.Add(TimeSpan.FromSeconds(20d))).Repeat.Once();

            taskOwnerMock.AverageTaskTime = value.Add(TimeSpan.FromSeconds(1d));
            LastCall.Repeat.Once();

            taskOwnerMock.AverageAfterTaskTime = value.Add(TimeSpan.FromSeconds(21d));
            LastCall.Repeat.Once();

            mocks.ReplayAll();

            target.TaskOwner = taskOwnerMock;
            target.AverageTaskTime = value.Add(TimeSpan.FromSeconds(1d));
            target.AverageAfterTaskTime = value.Add(TimeSpan.FromSeconds(21d));

            Assert.AreEqual(value, target.AverageTaskTime);
            Assert.AreEqual(value.Add(TimeSpan.FromSeconds(20d)), target.AverageAfterTaskTime);
        }

        /// <summary>
        /// Verifies the current date.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        [Test]
        public void VerifyCurrentDate()
        {
            ITaskOwner taskOwnerMock = mocks.StrictMock<ITaskOwner>();

            var value = new DateOnly(2008,7,16);
            Expect.Call(taskOwnerMock.CurrentDate).Return(value).Repeat.Once();

            mocks.ReplayAll();

            target.TaskOwner = taskOwnerMock;

            Assert.AreEqual(value, target.CurrentDate);
        }

        /// <summary>
        /// Verifies the is closed.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        [Test]
        public void VerifyIsClosed()
        {
            ITaskOwner taskOwnerMock = mocks.StrictMock<ITaskOwner>();

            Expect.Call(taskOwnerMock.OpenForWork).Return(new OpenForWork());

            mocks.ReplayAll();

            target.TaskOwner = taskOwnerMock;

            Assert.IsFalse(target.OpenForWork.IsOpen);
        }

        /// <summary>
        /// Verifies the is locked.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        [Test]
        public void VerifyIsLocked()
        {
            ITaskOwner taskOwnerMock = mocks.StrictMock<ITaskOwner>();

            Expect.Call(taskOwnerMock.IsLocked).Return(false).Repeat.Once();

            mocks.ReplayAll();

            target.TaskOwner = taskOwnerMock;

            Assert.IsFalse(target.IsLocked);
        }

        /// <summary>
        /// Verifies the recalculation methods.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        [Test]
        public void VerifyRecalculationMethods()
        {
            ITaskOwner taskOwnerMock = mocks.StrictMock<ITaskOwner>();

            taskOwnerMock.RecalculateDailyAverageCampaignTimes();
            LastCall.Repeat.Once();

            taskOwnerMock.RecalculateDailyAverageStatisticTimes();
            LastCall.Repeat.Once();

            taskOwnerMock.RecalculateDailyAverageTimes();
            LastCall.Repeat.Once();
            
            taskOwnerMock.RecalculateDailyCampaignTasks();
            LastCall.Repeat.Once();
            
            taskOwnerMock.RecalculateDailyStatisticTasks();
            LastCall.Repeat.Once();
            
            taskOwnerMock.RecalculateDailyTasks();
            LastCall.Repeat.Once();

            mocks.ReplayAll();

            target.TaskOwner = taskOwnerMock;

            target.RecalculateDailyAverageCampaignTimes();
            target.RecalculateDailyAverageStatisticTimes();
            target.RecalculateDailyAverageTimes();
            target.RecalculateDailyCampaignTasks();
            target.RecalculateDailyStatisticTasks();
            target.RecalculateDailyTasks();

            Assert.IsNotNull(target);
        }

        /// <summary>
        /// Verifies the lock and release.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        [Test]
        public void VerifyLockAndRelease()
        {
            ITaskOwner taskOwnerMock = mocks.StrictMock<ITaskOwner>();

            taskOwnerMock.Lock();
            LastCall.Repeat.Once();

            taskOwnerMock.Release();
            LastCall.Repeat.Once();
            
            mocks.ReplayAll();

            target.TaskOwner = taskOwnerMock;
            target.Lock();
            target.Release();

            Assert.IsNotNull(target);
        }

        /// <summary>
        /// Verifies the parent is working.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        [Test]
        public void VerifyParentWorks()
        {
            ITaskOwner taskOwnerMock = mocks.StrictMock<ITaskOwner>();
            ITaskOwner parentTaskOwner = mocks.StrictMock<ITaskOwner>();

            taskOwnerMock.AddParent(parentTaskOwner);
            LastCall.Repeat.Once();

            taskOwnerMock.RemoveParent(parentTaskOwner);
            LastCall.Repeat.Once();

            mocks.ReplayAll();

            target.TaskOwner = taskOwnerMock;
            target.AddParent(parentTaskOwner);
            target.RemoveParent(parentTaskOwner);

            Assert.IsNotNull(target);
        }

        /// <summary>
        /// Verifies the set dirty works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        [Test]
        public void VerifySetDirtyWorks()
        {
            ITaskOwner taskOwnerMock = mocks.StrictMock<ITaskOwner>();

            taskOwnerMock.SetDirty();
            LastCall.Repeat.Once();

            mocks.ReplayAll();

            target.TaskOwner = taskOwnerMock;
            target.SetDirty();

            Assert.IsNotNull(target);
        }

        #endregion
    }
}