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
			_target = taskFactory.Create(new DateOnlyPeriod(2015, 6, 1, 2015, 6, 7), 1000, TimeSpan.FromMinutes(6));
		}

		[Test]
		public void ShouldCalculateOutgoingBacklogForDate()
		{
			_target.Open(new DateOnly(2015, 6, 1));
			_target.Open(new DateOnly(2015, 6, 2));
			_target.Open(new DateOnly(2015, 6, 3));
			_target.Open(new DateOnly(2015, 6, 4));
			_target.Open(new DateOnly(2015, 6, 5));
			_target.Close(new DateOnly(2015, 6, 6));
			_target.Close(new DateOnly(2015, 6, 7));

			_target.RecalculateDistribution();
			Assert.AreEqual(TimeSpan.FromHours(80), _target.GetEstimatedOutgoingBacklogOnDate(new DateOnly(2015, 6, 1)));
			Assert.AreEqual(TimeSpan.FromHours(60), _target.GetEstimatedOutgoingBacklogOnDate(new DateOnly(2015, 6, 2)));
			Assert.AreEqual(TimeSpan.FromHours(40), _target.GetEstimatedOutgoingBacklogOnDate(new DateOnly(2015, 6, 3)));
			Assert.AreEqual(TimeSpan.FromHours(20), _target.GetEstimatedOutgoingBacklogOnDate(new DateOnly(2015, 6, 4)));
			Assert.AreEqual(TimeSpan.Zero, _target.GetEstimatedOutgoingBacklogOnDate(new DateOnly(2015, 6, 5)));
			Assert.AreEqual(TimeSpan.Zero, _target.GetEstimatedOutgoingBacklogOnDate(new DateOnly(2015, 6, 6)));
			Assert.AreEqual(TimeSpan.Zero, _target.GetEstimatedOutgoingBacklogOnDate(new DateOnly(2015, 6, 7)));
		}
	}
}