using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Backlog;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DomainTest.Backlog
{
	[TestFixture]
	public class IncomingTaskTest
	{
		private IncomingTask _target;

		[SetUp]
		public void Setup()
		{
			var taskFactory = new IncomingTaskFactory(new FlatDistributionSetter());
			_target = taskFactory.Create(new DateOnlyPeriod(2015, 4, 1, 2015, 4, 3), 100, TimeSpan.FromMinutes(1));
		}

		//[Test]
		//public void ShouldReturnTimeSPanZeroIfTryingToGetTimeOnDateOutsidePeriod()
		//{
		//	Assert.AreEqual(1,2);
		//}
	}
}