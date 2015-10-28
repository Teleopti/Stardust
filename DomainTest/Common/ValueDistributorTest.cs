using System;
using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Common
{
    [TestFixture]
    public class ValueDistributorTest
    {
        /// <summary>
        /// Distributes the values test.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-13
        /// </remarks>
        [Test]
        public void DistributeValuesByPercentTest()
        {
            IList<TestDistributionTarget> targets = GetDistributionTargetsForTest();

            double newTotal = 190d;
            ValueDistributor.Distribute(newTotal, targets, DistributionType.ByPercent);
            for (int i = 0; i < targets.Count; i++)
			{
                Assert.AreEqual(((i + 11) / 155d) * newTotal, targets[i].Tasks);
			}   
        }

	    [Test]
	    public void ShouldDistributeOverrideTasksTest()
	    {
			IList<TestDistributionTarget> targets = GetDistributionTargetsForTest();

			const double newDailyOverrideTasks = 190d;

			ValueDistributor.DistributeOverrideTasks(newDailyOverrideTasks, targets);

			for (var i = 0; i < targets.Count; i++)
			{
				Assert.AreEqual(((i + 11) / 155d) * newDailyOverrideTasks, targets[i].OverrideTasks);
			}
	    }

	    [Test]
	    public void ShouldClearOverrideTasksTest()
	    {
			IList<TestDistributionTarget> targets = GetDistributionTargetsForTest();
			ValueDistributor.DistributeOverrideTasks(null, targets);

			foreach (var target in targets)
			{
				Assert.IsNull(target.OverrideTasks);
			}
	    }

	    /// <summary>
        /// Distributes the values even test.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-13
        /// </remarks>
        [Test]
        public void DistributeValuesEvenTest()
        {
            IList<TestDistributionTarget> targets = GetDistributionTargetsForTest();

            double newTotal = 190d;
            ValueDistributor.Distribute(newTotal, targets, DistributionType.Even);
            for (int i = 0; i < targets.Count; i++)
            {
                //10 is added to the value in the generation of targets
                //3.5 is the expected distribution to each item
                Assert.AreEqual(i + 11 + 3.5d, targets[i].Tasks);
            }
        }

        /// <summary>
        /// Distributes the task times average task time.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-17
        /// </remarks>
        [Test]
        public void DistributeTaskTimesAverageTaskTime()
        {
            IList<TestDistributionTarget> targets = GetDistributionTargetsForTest();

            double percentage = 0.25d;
            ValueDistributor.DistributeTaskTimes(percentage, targets, TaskFieldToDistribute.AverageTaskTime, TimeSpan.FromSeconds(1).Ticks);

            Assert.AreEqual(10, targets.Count);
            Assert.AreEqual(TimeSpan.FromSeconds(1.5d), targets[0].AverageTaskTime);
            Assert.AreEqual(TimeSpan.FromSeconds(3.75d), targets[9].AverageTaskTime);
        }

        /// <summary>
        /// Distributes the task times average task time with zero as old value.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-19
        /// </remarks>
        [Test]
        public void DistributeTaskTimesAverageTaskTimeWithZeroAsOldValue()
        {
            IList<TestDistributionTarget> targets = GetDistributionTargetsForTest();

            foreach (TestDistributionTarget item in targets)
            {
                item.AverageTaskTime = TimeSpan.Zero;
            }

            double percentage = 0.25d;
            ValueDistributor.DistributeTaskTimes(percentage, targets, TaskFieldToDistribute.AverageTaskTime, TimeSpan.FromSeconds(1).Ticks);

            Assert.AreEqual(10, targets.Count);
            Assert.AreEqual(TimeSpan.FromSeconds(1), targets[0].AverageTaskTime);
            Assert.AreEqual(TimeSpan.FromSeconds(1), targets[9].AverageTaskTime);
        }

        /// <summary>
        /// Distributes the task times average after task time with zero as old value.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-19
        /// </remarks>
        [Test]
        public void DistributeTaskTimesAverageAfterTaskTimeWithZeroAsOldValue()
        {
            IList<TestDistributionTarget> targets = GetDistributionTargetsForTest();

            foreach (TestDistributionTarget item in targets)
            {
                item.AverageAfterTaskTime = TimeSpan.Zero;
            }

            double percentage = 0.25d;
            ValueDistributor.DistributeTaskTimes(percentage, targets, TaskFieldToDistribute.AverageAfterTaskTime, TimeSpan.FromSeconds(1).Ticks);

            Assert.AreEqual(10, targets.Count);
            Assert.AreEqual(TimeSpan.FromSeconds(1), targets[0].AverageAfterTaskTime);
            Assert.AreEqual(TimeSpan.FromSeconds(1), targets[9].AverageAfterTaskTime);
        }

        /// <summary>
        /// Distributes the task times average task time with zero as new value.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-19
        /// </remarks>
        [Test]
        public void DistributeTaskTimesAverageTaskTimeWithZeroAsNewValue()
        {
            IList<TestDistributionTarget> targets = GetDistributionTargetsForTest();

            double percentage = 0d;
            ValueDistributor.DistributeTaskTimes(percentage, targets, TaskFieldToDistribute.AverageTaskTime, TimeSpan.FromSeconds(1).Ticks);

            Assert.AreEqual(10, targets.Count);
            Assert.AreEqual(TimeSpan.FromSeconds(0d), targets[0].AverageTaskTime);
            Assert.AreEqual(TimeSpan.FromSeconds(0d), targets[9].AverageTaskTime);
        }

        /// <summary>
        /// Distributes the task times average after task time with zero as new value.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-19
        /// </remarks>
        [Test]
        public void DistributeTaskTimesAverageAfterTaskTimeWithZeroAsNewValue()
        {
            IList<TestDistributionTarget> targets = GetDistributionTargetsForTest();

            double percentage = 0d;
            ValueDistributor.DistributeTaskTimes(percentage, targets, TaskFieldToDistribute.AverageAfterTaskTime, TimeSpan.FromSeconds(1).Ticks);

            Assert.AreEqual(10, targets.Count);
            Assert.AreEqual(TimeSpan.FromSeconds(0d), targets[0].AverageAfterTaskTime);
            Assert.AreEqual(TimeSpan.FromSeconds(0d), targets[9].AverageAfterTaskTime);
        }

        /// <summary>
        /// Distributes the task times average after task time.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-17
        /// </remarks>
        [Test]
        public void DistributeTaskTimesAverageAfterTaskTime()
        {
            IList<TestDistributionTarget> targets = GetDistributionTargetsForTest();

            const double percentage = 0.25d;
            ValueDistributor.DistributeTaskTimes(percentage, targets, TaskFieldToDistribute.AverageAfterTaskTime,TimeSpan.FromSeconds(1).Ticks);

            Assert.AreEqual(10, targets.Count);
            Assert.AreEqual(TimeSpan.FromSeconds(0.25d), targets[0].AverageAfterTaskTime);
            Assert.AreEqual(TimeSpan.FromSeconds(2.5d), targets[9].AverageAfterTaskTime);
        }

        /// <summary>
        /// Distributes the task times with invalid class as target test.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-17
        /// </remarks>
        [Test]
        public void DistributeTaskTimesWithInvalidClassAsTargetTest()
        {
            IList<Color> targets = new List<Color>();
            targets.Add(Color.White);

            ValueDistributor.DistributeTaskTimes(0.19d, targets, TaskFieldToDistribute.AverageTaskTime, TimeSpan.FromSeconds(1).Ticks);

            Assert.AreEqual(1, targets.Count);
            Assert.AreEqual(Color.White, targets[0]);
        }

        /// <summary>
        /// Distributes the values with invalid class as target test.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-13
        /// </remarks>
        [Test]
        public void DistributeValuesWithInvalidClassAsTargetTest()
        {
            IList<Color> targets = new List<Color>();
            targets.Add(Color.White);

            ValueDistributor.Distribute(190d, targets, DistributionType.Even);

            Assert.AreEqual(1, targets.Count);
            Assert.AreEqual(Color.White, targets[0]);
        }

        /// <summary>
        /// Distributes the values with zero tasks test.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-07
        /// </remarks>
        [Test]
        public void DistributeValuesWithZeroTasksTest()
        {
            IList<TestDistributionTarget> targets = GetDistributionTargetsForTest();
            foreach (TestDistributionTarget testDistributionTarget in targets)
            {
                testDistributionTarget.Tasks = 0;
            }

            ValueDistributor.Distribute(75d, targets, DistributionType.ByPercent);

            Assert.AreEqual(10, targets.Count);
            foreach (TestDistributionTarget testDistributionTarget in targets)
            {
                Assert.AreEqual(7.5d, testDistributionTarget.Tasks);
            }
        }

        /// <summary>
        /// Distributes the values with one zero task test.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-07
        /// </remarks>
        [Test]
        public void DistributeValuesWithOneZeroTaskTest()
        {
            IList<TestDistributionTarget> targets = GetDistributionTargetsForTest();
            targets[8].Tasks = 0;

            ValueDistributor.Distribute(75d, targets, DistributionType.ByPercent);

            Assert.AreEqual(10, targets.Count);
            Assert.AreEqual(0d, targets[8].Tasks);
            Assert.GreaterOrEqual(targets[7].Tasks, 6);
            Assert.GreaterOrEqual(targets[9].Tasks, 6);
        }

        [Test]
        public void DistributeTaskTimesEvenAverageAfterTaskTime()
        {
            IList<TestDistributionTarget> targets = GetDistributionTargetsForTest();

            ValueDistributor.DistributeTaskTimes(0d, TimeSpan.FromSeconds(5), targets, TaskFieldToDistribute.AverageAfterTaskTime, DistributionType.Even, TimeSpan.FromSeconds(1).Ticks);

            Assert.AreEqual(10, targets.Count);
            Assert.AreEqual(TimeSpan.FromSeconds(5d), targets[0].AverageAfterTaskTime);
            Assert.AreEqual(TimeSpan.FromSeconds(5d), targets[9].AverageAfterTaskTime);
        }

        [Test]
        public void DistributeTaskTimesEvenAverageTaskTime()
        {
            IList<TestDistributionTarget> targets = GetDistributionTargetsForTest();

            ValueDistributor.DistributeTaskTimes(0d, TimeSpan.FromSeconds(10), targets, TaskFieldToDistribute.AverageTaskTime, DistributionType.Even, TimeSpan.FromSeconds(1).Ticks);

            Assert.AreEqual(10, targets.Count);
            Assert.AreEqual(TimeSpan.FromSeconds(10d), targets[0].AverageTaskTime);
            Assert.AreEqual(TimeSpan.FromSeconds(10d), targets[9].AverageTaskTime);
        }

        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyDistributeTaskTimesEvenDoesNotAcceptNegativeTime()
        {
            ValueDistributor.DistributeTaskTimes(0d, TimeSpan.FromSeconds(-10), new List<ITaskOwner>(), TaskFieldToDistribute.AverageTaskTime, DistributionType.Even, TimeSpan.FromSeconds(1).Ticks);
        }

        /// <summary>
        /// Gets the distribution targets for test.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-13
        /// </remarks>
        private static IList<TestDistributionTarget> GetDistributionTargetsForTest()
        {
            IList<TestDistributionTarget> targets = new List<TestDistributionTarget>();
            for (int i = 0; i < 10; i++)
			{
                TestDistributionTarget target = new TestDistributionTarget() 
                { 
                    Tasks = i + 11,
                    AverageAfterTaskTime = TimeSpan.FromSeconds(i + 1),
                    AverageTaskTime = TimeSpan.FromSeconds(i + 6),
					OverrideTasks = i + 12
                };
                targets.Add(target);
			}

            return targets;
        }

        /// <summary>
        /// New class to test distribution targets
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-13
        /// </remarks>
        private class TestDistributionTarget : ITaskOwner
        {
            #region ITaskOwner Members

            /// <summary>
            /// Gets or sets the total tasks.
            /// </summary>
            /// <value>The total tasks.</value>
            /// <remarks>
            /// Created by: robink
            /// Created date: 2008-01-22
            /// </remarks>
            public double Tasks
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the average after task time.
            /// </summary>
            /// <value>The average after task time.</value>
            /// <remarks>
            /// Created by: robink
            /// Created date: 2007-12-17
            /// </remarks>
            public TimeSpan AverageAfterTaskTime
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the average task time.
            /// </summary>
            /// <value>The average task time.</value>
            /// <remarks>
            /// Created by: robink
            /// Created date: 2007-12-17
            /// </remarks>
            public TimeSpan AverageTaskTime
            {
                get;
                set;
            }

            /// <summary>
            /// Locks this instance.
            /// </summary>
            /// <remarks>
            /// Created by: robink
            /// Created date: 2008-01-22
            /// </remarks>
            public void Lock()
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Releases this instance.
            /// </summary>
            /// <remarks>
            /// Created by: robink
            /// Created date: 2008-01-22
            /// </remarks>
            public void Release()
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Recalcs the dayly average times.
            /// </summary>
            /// <remarks>
            /// Created by: robink
            /// Created date: 2007-12-18
            /// </remarks>
            public void RecalculateDailyAverageTimes()
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Recalcs the dayly tasks.
            /// </summary>
            /// <remarks>
            /// Created by: robink
            /// Created date: 2007-12-18
            /// </remarks>
            public void RecalculateDailyTasks()
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Sets the entity as dirty.
            /// </summary>
            /// <remarks>
            /// Created by: robink
            /// Created date: 2008-01-23
            /// </remarks>
            public void SetDirty()
            {
                throw new NotImplementedException();
            }

            public void ClearParents()
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Gets the current date.
            /// </summary>
            /// <value>The current date.</value>
            /// <remarks>
            /// Created by: robink
            /// Created date: 2008-01-25
            /// </remarks>
            public DateOnly CurrentDate
            {
                get { throw new NotImplementedException(); }
            }

            /// <summary>
            /// Gets a value indicating whether this instance is closed.
            /// </summary>
            /// <value><c>true</c> if this instance is closed; otherwise, <c>false</c>.</value>
            /// <remarks>
            /// Created by: robink
            /// Created date: 2008-01-25
            /// </remarks>
            public OpenForWork OpenForWork
            {
                get { throw new NotImplementedException(); }
            }

            /// <summary>
            /// Gets a value indicating whether this instance is locked.
            /// </summary>
            /// <value><c>true</c> if this instance is locked; otherwise, <c>false</c>.</value>
            /// <remarks>
            /// Created by: robink
            /// Created date: 2008-01-25
            /// </remarks>
            public bool IsLocked
            {
                get { throw new NotImplementedException(); }
            }

            public void UpdateTemplateName()
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Adds the parent.
            /// </summary>
            /// <param name="parent">The parent.</param>
            /// <remarks>
            /// Created by: robink
            /// Created date: 2008-01-25
            /// </remarks>
            public void AddParent(ITaskOwner parent)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Removes the parent.
            /// </summary>
            /// <param name="parent">The parent.</param>
            /// <remarks>
            /// Created by: robink
            /// Created date: 2008-01-25
            /// </remarks>
            public void RemoveParent(ITaskOwner parent)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Gets the total statistic calculated tasks.
            /// </summary>
            /// <value>The total statistic calculated tasks.</value>
            /// <remarks>
            /// Created by: robink
            /// Created date: 2008-03-03
            /// </remarks>
            public double TotalStatisticCalculatedTasks
            {
                get { throw new NotImplementedException(); }
            }

            /// <summary>
            /// Gets the total statistic answered tasks.
            /// </summary>
            /// <value>The total statistic answered tasks.</value>
            /// <remarks>
            /// Created by: robink
            /// Created date: 2008-03-03
            /// </remarks>
            public double TotalStatisticAnsweredTasks
            {
                get { throw new NotImplementedException(); }
            }

            /// <summary>
            /// Gets the total statistic abandoned tasks.
            /// </summary>
            /// <value>The total statistic abandoned tasks.</value>
            /// <remarks>
            /// Created by: robink
            /// Created date: 2008-03-03
            /// </remarks>
            public double TotalStatisticAbandonedTasks
            {
                get { throw new NotImplementedException(); }
            }

            /// <summary>
            /// Gets the total statistic average task time.
            /// </summary>
            /// <value>The total statistic average task time.</value>
            /// <remarks>
            /// Created by: robink
            /// Created date: 2008-03-03
            /// </remarks>
            public TimeSpan TotalStatisticAverageTaskTime
            {
                get { throw new NotImplementedException(); }
            }

            /// <summary>
            /// Gets the total statistic average after task time.
            /// </summary>
            /// <value>The total statistic average after task time.</value>
            /// <remarks>
            /// Created by: robink
            /// Created date: 2008-03-03
            /// </remarks>
            public TimeSpan TotalStatisticAverageAfterTaskTime
            {
                get { throw new NotImplementedException(); }
            }

            /// <summary>
            /// Recalculates the daily average statistic times.
            /// </summary>
            /// <remarks>
            /// Created by: robink
            /// Created date: 2008-03-03
            /// </remarks>
            public void RecalculateDailyAverageStatisticTimes()
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Recalculates the daily statistic tasks.
            /// </summary>
            /// <remarks>
            /// Created by: robink
            /// Created date: 2008-03-03
            /// </remarks>
            public void RecalculateDailyStatisticTasks()
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Gets the total average after task time.
            /// </summary>
            /// <value>The total average after task time.</value>
            /// <remarks>
            /// Created by: robink
            /// Created date: 2008-03-04
            /// </remarks>
            public TimeSpan TotalAverageAfterTaskTime
            {
                get { throw new NotImplementedException(); }
            }

            /// <summary>
            /// Gets the total average task time.
            /// </summary>
            /// <value>The total average task time.</value>
            /// <remarks>
            /// Created by: robink
            /// Created date: 2008-03-04
            /// </remarks>
            public TimeSpan TotalAverageTaskTime
            {
                get { throw new NotImplementedException(); }
            }

            /// <summary>
            /// Gets or sets the tasks.
            /// </summary>
            /// <value>The tasks.</value>
            /// <remarks>
            /// Created by: robink
            /// Created date: 2008-03-04
            /// </remarks>
            public double TotalTasks
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

	        public double? OverrideTasks { get; set; }

	        /// <summary>
            /// Gets or sets the campaign tasks.
            /// </summary>
            /// <value>The campaign tasks.</value>
            /// <remarks>
            /// Created by: robink
            /// Created date: 2008-03-04
            /// </remarks>
            public Percent CampaignTasks
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    throw new NotImplementedException();
                }
            }

            /// <summary>
            /// Gets or sets the campaign task time.
            /// </summary>
            /// <value>The campaign task time.</value>
            /// <remarks>
            /// Created by: robink
            /// Created date: 2008-03-04
            /// </remarks>
            public Percent CampaignTaskTime
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    throw new NotImplementedException();
                }
            }

            /// <summary>
            /// Gets or sets the campaign after task time.
            /// </summary>
            /// <value>The campaign after task time.</value>
            /// <remarks>
            /// Created by: robink
            /// Created date: 2008-03-04
            /// </remarks>
            public Percent CampaignAfterTaskTime
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    throw new NotImplementedException();
                }
            }

            /// <summary>
            /// Recalcs the dayly tasks.
            /// </summary>
            /// <remarks>
            /// Created by: robink
            /// Created date: 2007-12-18
            /// </remarks>
            public void RecalculateDailyCampaignTasks()
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Recalcs the dayly average times.
            /// </summary>
            /// <remarks>
            /// Created by: robink
            /// Created date: 2007-12-18
            /// </remarks>
            public void RecalculateDailyAverageCampaignTimes()
            {
                throw new NotImplementedException();
            }
            public void ResetTaskOwner()
            {
                throw new NotImplementedException();
            }
            #endregion
        }
    }
}
