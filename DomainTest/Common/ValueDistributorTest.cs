using System;
using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


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
		public void DistributeValuesByPercent()
		{
			IList<testDistributionTarget> targets = getDistributionTargetsForTest();

			const double newTotal = 190d;
			ValueDistributor.Distribute(newTotal, targets, DistributionType.ByPercent);
			for (int i = 0; i < targets.Count; i++)
			{
				Assert.AreEqual(((i + 11) / 155d) * newTotal, targets[i].Tasks);
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
		public void DistributeValuesEvenly()
		{
			IList<testDistributionTarget> targets = getDistributionTargetsForTest();

			const double newTotal = 190d;
			ValueDistributor.Distribute(newTotal, targets, DistributionType.Even);
			for (int i = 0; i < targets.Count; i++)
			{
				//10 is added to the value in the generation of targets
				//3.5 is the expected distribution to each item
				Assert.AreEqual(i + 11 + 3.5d, targets[i].Tasks);
			}
		}

		/// <summary>
		/// Distributes the values with invalid class as target test.
		/// </summary>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2007-12-13
		/// </remarks>
		[Test]
		public void DistributeValuesWithInvalidClassAsTarget()
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
		public void DistributeValuesWithZeroTasks()
		{
			IList<testDistributionTarget> targets = getDistributionTargetsForTest();
			foreach (testDistributionTarget testDistributionTarget in targets)
			{
				testDistributionTarget.Tasks = 0;
			}

			ValueDistributor.Distribute(75d, targets, DistributionType.ByPercent);

			Assert.AreEqual(10, targets.Count);
			foreach (testDistributionTarget testDistributionTarget in targets)
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
		public void DistributeValuesWithOneZeroTask()
		{
			IList<testDistributionTarget> targets = getDistributionTargetsForTest();
			targets[8].Tasks = 0;

			ValueDistributor.Distribute(75d, targets, DistributionType.ByPercent);

			Assert.AreEqual(10, targets.Count);
			Assert.AreEqual(0d, targets[8].Tasks);
			Assert.GreaterOrEqual(targets[7].Tasks, 6);
			Assert.GreaterOrEqual(targets[9].Tasks, 6);
		}

		[Test]
		public void DistributeAverageTaskTimeEvenly()
		{
			IList<testDistributionTarget> targets = getDistributionTargetsForTest();

			ValueDistributor.DistributeAverageTaskTime(0d, TimeSpan.FromSeconds(10), targets, DistributionType.Even);

			Assert.AreEqual(10, targets.Count);
			Assert.AreEqual(TimeSpan.FromSeconds(10d), targets[0].AverageTaskTime);
			Assert.AreEqual(TimeSpan.FromSeconds(10d), targets[9].AverageTaskTime);
		}

		[Test]
		public void DistributeAverageTaskTimeByPercentage()
		{
			IList<testDistributionTarget> targets = new List<testDistributionTarget>();

			targets.Add(new testDistributionTarget { AverageTaskTime = TimeSpan.FromSeconds(60) });
			targets.Add(new testDistributionTarget { AverageTaskTime = TimeSpan.FromSeconds(120) });

			ValueDistributor.DistributeAverageTaskTime(0.5d, TimeSpan.FromSeconds(30), targets, DistributionType.ByPercent);

			Assert.AreEqual(TimeSpan.FromSeconds(60).Ticks * 0.5d, targets[0].AverageTaskTime.Ticks);
			Assert.AreEqual(TimeSpan.FromSeconds(120).Ticks * 0.5d, targets[1].AverageTaskTime.Ticks);
		}

		[Test]
		public void DistributeAverageTaskTimeByPercentageWhenOriginalValueIsZero()
		{
			IList<testDistributionTarget> targets = new List<testDistributionTarget>();

			targets.Add(new testDistributionTarget { AverageTaskTime = TimeSpan.FromSeconds(0) });

			ValueDistributor.DistributeAverageTaskTime(0.5d, TimeSpan.FromSeconds(30), targets, DistributionType.ByPercent);

			Assert.AreEqual(TimeSpan.FromSeconds(30).Ticks, targets[0].AverageTaskTime.Ticks);
		}

		[Test]
		public void VerifyDistributeAverageTaskTimesEvenDoesNotAcceptNegativeTime()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => ValueDistributor.DistributeAverageTaskTime(0d, TimeSpan.FromSeconds(-10), new List<ITaskOwner>(), DistributionType.Even));
		}

		[Test]
		public void DistributeAverageAfterTaskTimeEvenly()
		{
			IList<testDistributionTarget> targets = getDistributionTargetsForTest();

			ValueDistributor.DistributeAverageAfterTaskTime(0d, TimeSpan.FromSeconds(5), targets, DistributionType.Even);

			Assert.AreEqual(10, targets.Count);
			Assert.AreEqual(TimeSpan.FromSeconds(5d), targets[0].AverageAfterTaskTime);
			Assert.AreEqual(TimeSpan.FromSeconds(5d), targets[9].AverageAfterTaskTime);
		}

		[Test]
		public void DistributeAverageAfterTaskTimeByPercentage()
		{
			IList<testDistributionTarget> targets = new List<testDistributionTarget>();

			targets.Add(new testDistributionTarget { AverageAfterTaskTime = TimeSpan.FromSeconds(60) });
			targets.Add(new testDistributionTarget { AverageAfterTaskTime = TimeSpan.FromSeconds(120) });

			ValueDistributor.DistributeAverageAfterTaskTime(0.5d, TimeSpan.FromSeconds(30), targets, DistributionType.ByPercent);

			Assert.AreEqual(TimeSpan.FromSeconds(60).Ticks * 0.5d, targets[0].AverageAfterTaskTime.Ticks);
			Assert.AreEqual(TimeSpan.FromSeconds(120).Ticks * 0.5d, targets[1].AverageAfterTaskTime.Ticks);
		}

		[Test]
		public void DistributeAverageAfterTaskTimeByPercentageWhenOriginalValueIsZero()
		{
			IList<testDistributionTarget> targets = new List<testDistributionTarget>();

			targets.Add(new testDistributionTarget { AverageAfterTaskTime = TimeSpan.FromSeconds(0) });

			ValueDistributor.DistributeAverageAfterTaskTime(0.5d, TimeSpan.FromSeconds(30), targets, DistributionType.ByPercent);

			Assert.AreEqual(TimeSpan.FromSeconds(30).Ticks, targets[0].AverageAfterTaskTime.Ticks);
		}

		[Test]
		public void VerifyDistributeAverageAfterTaskTimesEvenDoesNotAcceptNegativeTime()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => ValueDistributor.DistributeAverageAfterTaskTime(0d, TimeSpan.FromSeconds(-10), new List<ITaskOwner>(), DistributionType.Even));
		}

		[Test]
		public void ShouldDistributeToFirstOpenPeriod()
		{
			var targets = new List<ITemplateTaskPeriod>
			{
				new TemplateTaskPeriod(new Task(5, TimeSpan.Zero, TimeSpan.Zero), new DateTimePeriod(2015, 10, 15, 8, 2015, 10, 15, 9)),
				new TemplateTaskPeriod(new Task(5, TimeSpan.Zero, TimeSpan.Zero), new DateTimePeriod(2015, 10, 15, 9, 2015, 10, 15, 10)),
				new TemplateTaskPeriod(new Task(5, TimeSpan.Zero, TimeSpan.Zero), new DateTimePeriod(2015, 10, 15, 10, 2015, 10, 15, 11))
			}.ToArray();

			var openHours = new List<TimePeriod>
			{
				new TimePeriod(new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0))
			};

			ValueDistributor.DistributeToFirstOpenPeriod(6, targets, openHours, TimeZoneInfo.Utc);

			targets[0].Tasks.Should().Be.EqualTo(0);
			targets[1].Tasks.Should().Be.EqualTo(6);
			targets[2].Tasks.Should().Be.EqualTo(0);

		}

		/// <summary>
		/// Gets the distribution targets for test.
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2007-12-13
		/// </remarks>
		private static IList<testDistributionTarget> getDistributionTargetsForTest()
		{
			IList<testDistributionTarget> targets = new List<testDistributionTarget>();
			for (int i = 0; i < 10; i++)
			{
				var target = new testDistributionTarget()
				{
					Tasks = i + 11,
					AverageAfterTaskTime = TimeSpan.FromSeconds(i + 1),
					AverageTaskTime = TimeSpan.FromSeconds(i + 6),
					Period = new DateTimePeriod(DateTime.SpecifyKind(DateTime.MinValue.Date.AddMinutes(15 * i), DateTimeKind.Utc), DateTime.SpecifyKind(DateTime.MinValue.Date.AddMinutes(15 * (i + 1)), DateTimeKind.Utc))
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
		private sealed class testDistributionTarget : ITaskOwner, IPeriodized
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

			public void ClearTemplateName()
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

			public DateTimePeriod Period { get; set; }
		}
	}
}
