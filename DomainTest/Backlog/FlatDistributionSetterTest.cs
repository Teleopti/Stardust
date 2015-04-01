using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Backlog;


namespace Teleopti.Ccc.DomainTest.Backlog
{
	[TestFixture]
	public class FlatDistributionSetterTest
	{
		private FlatDistributionSetter _target;
		private IList<TaskDay> _taskDays;

		[SetUp]
		public void Setup()
		{
			_target = new FlatDistributionSetter();
			_taskDays = new List<TaskDay>{new TaskDay(), new TaskDay(), new TaskDay(), new TaskDay()};
		}

		[Test]
		public void ShouldDistributeEvenlyIfNothingManualOrScheduled()
		{
			_target.Distribute(_taskDays, TimeSpan.FromMinutes(100));
			var index = 0;
			Assert.AreEqual(TimeSpan.FromMinutes(25), _taskDays[index].Time);
			Assert.AreEqual(PlannedTimeTypeEnum.Calculated, _taskDays[index].PlannedTimeType);
			index++;
			Assert.AreEqual(TimeSpan.FromMinutes(25), _taskDays[index].Time);
			Assert.AreEqual(PlannedTimeTypeEnum.Calculated, _taskDays[index].PlannedTimeType);
			index++;
			Assert.AreEqual(TimeSpan.FromMinutes(25), _taskDays[index].Time);
			Assert.AreEqual(PlannedTimeTypeEnum.Calculated, _taskDays[index].PlannedTimeType);
			index++;
			Assert.AreEqual(TimeSpan.FromMinutes(25), _taskDays[index].Time);
			Assert.AreEqual(PlannedTimeTypeEnum.Calculated, _taskDays[index].PlannedTimeType);
		}

		[Test]
		public void ShouldHandleClosedDays()
		{
			_taskDays[0].Close();
			_target.Distribute(_taskDays, TimeSpan.FromMinutes(99));
			var index = 0;
			Assert.AreEqual(TimeSpan.FromMinutes(0), _taskDays[index].Time);
			Assert.AreEqual(PlannedTimeTypeEnum.Closed, _taskDays[index].PlannedTimeType);
			index++;
			Assert.AreEqual(TimeSpan.FromMinutes(33), _taskDays[index].Time);
			Assert.AreEqual(PlannedTimeTypeEnum.Calculated, _taskDays[index].PlannedTimeType);
			index++;
			Assert.AreEqual(TimeSpan.FromMinutes(33), _taskDays[index].Time);
			Assert.AreEqual(PlannedTimeTypeEnum.Calculated, _taskDays[index].PlannedTimeType);
			index++;
			Assert.AreEqual(TimeSpan.FromMinutes(33), _taskDays[index].Time);
			Assert.AreEqual(PlannedTimeTypeEnum.Calculated, _taskDays[index].PlannedTimeType);
		}

		[Test]
		public void ShouldHandleManualTime()
		{
			_taskDays[2].SetTime(TimeSpan.FromMinutes(66), PlannedTimeTypeEnum.Manual);
			_target.Distribute(_taskDays, TimeSpan.FromMinutes(99));
			var index = 0;
			Assert.AreEqual(TimeSpan.FromMinutes(11), _taskDays[index].Time);
			Assert.AreEqual(PlannedTimeTypeEnum.Calculated, _taskDays[index].PlannedTimeType);
			index++;
			Assert.AreEqual(TimeSpan.FromMinutes(11), _taskDays[index].Time);
			Assert.AreEqual(PlannedTimeTypeEnum.Calculated, _taskDays[index].PlannedTimeType);
			index++;
			Assert.AreEqual(TimeSpan.FromMinutes(66), _taskDays[index].Time);
			Assert.AreEqual(PlannedTimeTypeEnum.Manual, _taskDays[index].PlannedTimeType);
			index++;
			Assert.AreEqual(TimeSpan.FromMinutes(11), _taskDays[index].Time);
			Assert.AreEqual(PlannedTimeTypeEnum.Calculated, _taskDays[index].PlannedTimeType);
		}

		[Test]
		public void ShouldHandleScheduledTime()
		{
			_taskDays[2].SetTime(TimeSpan.FromMinutes(66), PlannedTimeTypeEnum.Scheduled);
			_target.Distribute(_taskDays, TimeSpan.FromMinutes(99));
			var index = 0;
			Assert.AreEqual(TimeSpan.FromMinutes(11), _taskDays[index].Time);
			Assert.AreEqual(PlannedTimeTypeEnum.Calculated, _taskDays[index].PlannedTimeType);
			index++;
			Assert.AreEqual(TimeSpan.FromMinutes(11), _taskDays[index].Time);
			Assert.AreEqual(PlannedTimeTypeEnum.Calculated, _taskDays[index].PlannedTimeType);
			index++;
			Assert.AreEqual(TimeSpan.FromMinutes(66), _taskDays[index].Time);
			Assert.AreEqual(PlannedTimeTypeEnum.Scheduled, _taskDays[index].PlannedTimeType);
			index++;
			Assert.AreEqual(TimeSpan.FromMinutes(11), _taskDays[index].Time);
			Assert.AreEqual(PlannedTimeTypeEnum.Calculated, _taskDays[index].PlannedTimeType);
		}

		[Test]
		public void ShouldConsiderActualBacklogHigherThanCalculated()
		{
			_taskDays[1].SetActualBacklog(90, TimeSpan.FromMinutes(1));
			_target.Distribute(_taskDays, TimeSpan.FromMinutes(100));
			var index = 0;
			Assert.AreEqual(TimeSpan.FromMinutes(25), _taskDays[index].Time);
			Assert.AreEqual(PlannedTimeTypeEnum.Calculated, _taskDays[index].PlannedTimeType);
			index++;
			Assert.AreEqual(TimeSpan.FromMinutes(30), _taskDays[index].Time);
			Assert.AreEqual(PlannedTimeTypeEnum.Calculated, _taskDays[index].PlannedTimeType);
			index++;
			Assert.AreEqual(TimeSpan.FromMinutes(30), _taskDays[index].Time);
			Assert.AreEqual(PlannedTimeTypeEnum.Calculated, _taskDays[index].PlannedTimeType);
			index++;
			Assert.AreEqual(TimeSpan.FromMinutes(30), _taskDays[index].Time);
			Assert.AreEqual(PlannedTimeTypeEnum.Calculated, _taskDays[index].PlannedTimeType);
		}

		[Test]
		public void ShouldConsiderActualBacklogLowerThanCalculated()
		{
			_taskDays[1].SetActualBacklog(30, TimeSpan.FromMinutes(1));
			_target.Distribute(_taskDays, TimeSpan.FromMinutes(100));
			var index = 0;
			Assert.AreEqual(TimeSpan.FromMinutes(25), _taskDays[index].Time);
			Assert.AreEqual(PlannedTimeTypeEnum.Calculated, _taskDays[index].PlannedTimeType);
			index++;
			Assert.AreEqual(TimeSpan.FromMinutes(10), _taskDays[index].Time);
			Assert.AreEqual(PlannedTimeTypeEnum.Calculated, _taskDays[index].PlannedTimeType);
			index++;
			Assert.AreEqual(TimeSpan.FromMinutes(10), _taskDays[index].Time);
			Assert.AreEqual(PlannedTimeTypeEnum.Calculated, _taskDays[index].PlannedTimeType);
			index++;
			Assert.AreEqual(TimeSpan.FromMinutes(10), _taskDays[index].Time);
			Assert.AreEqual(PlannedTimeTypeEnum.Calculated, _taskDays[index].PlannedTimeType);
		}
	}
}