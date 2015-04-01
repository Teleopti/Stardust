using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Backlog;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Backlog
{
	[TestFixture]
	public class TimeDistributorTest
	{
		private TimeDistributor _target;
		private IncomingTask _task1;
		private IncomingTask _task2;

		[SetUp]
		public void Setup()
		{
			_target = new TimeDistributor();
			var taskFactory = new IncomingTaskFactory(new FlatDistributionSetter());
			_task1 = taskFactory.Create(new DateOnlyPeriod(2015, 4, 1, 2015, 4, 2), 10, TimeSpan.FromHours(1));
			_task2 = taskFactory.Create(new DateOnlyPeriod(2015, 4, 2, 2015, 4, 3), 10, TimeSpan.FromHours(1));
		}

		//[Test]
		//public void ShouldDistributeAndPrioritizeOldestTask()
		//{
		//	_target.Distribute(new DateOnly(2015, 4, 2), TimeSpan.FromHours(12), new List<IncomingTask> {_task1, _task2}, PlannedTimeTypeEnum.Manual);
		//	Assert.AreEqual(TimeSpan.Zero, _task1.GetTimeOnDate(new DateOnly(2015, 4, 1)));
		//	Assert.AreEqual(TimeSpan.FromHours(10), _task1.GetTimeOnDate(new DateOnly(2015, 4, 2)));
		//	Assert.AreEqual(TimeSpan.FromHours(2), _task2.GetTimeOnDate(new DateOnly(2015, 4, 2)));
		//	Assert.AreEqual(TimeSpan.FromHours(8), _task2.GetTimeOnDate(new DateOnly(2015, 4, 3)));
		//}

		//[Test]
		//public void ShouldHandleManualOrScheduledOnOtherDaysAsWell()
		//{
		//	_target.Distribute(new DateOnly(2015, 4, 2), TimeSpan.FromHours(13), new List<IncomingTask> { _task1, _task2 }, PlannedTimeTypeEnum.Manual);
		//	_target.Distribute(new DateOnly(2015, 4, 3), TimeSpan.FromHours(3), new List<IncomingTask> { _task2 }, PlannedTimeTypeEnum.Manual);
		//	Assert.AreEqual(TimeSpan.FromHours(4), _task1.GetTimeOnDate(new DateOnly(2015, 4, 1)));
		//	Assert.AreEqual(TimeSpan.FromHours(6), _task1.GetTimeOnDate(new DateOnly(2015, 4, 2)));
		//	Assert.AreEqual(TimeSpan.FromHours(7), _task2.GetTimeOnDate(new DateOnly(2015, 4, 2)));
		//	Assert.AreEqual(TimeSpan.FromHours(3), _task2.GetTimeOnDate(new DateOnly(2015, 4, 3)));
		//}
	}
}