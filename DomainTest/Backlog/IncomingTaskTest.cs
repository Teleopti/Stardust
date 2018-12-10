using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Backlog;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;



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
		public void GetOverstaffTimeOnDateShouldWorkForScheduledTime()
		{
			setupOpenDays();

			Assert.AreEqual(TimeSpan.Zero, _target.GetOverstaffTimeOnDate(new DateOnly(2015, 06, 4)));

			_target.SetTimeOnDate(new DateOnly(2015, 6, 1), TimeSpan.FromHours(30), PlannedTimeTypeEnum.Scheduled);
			_target.SetTimeOnDate(new DateOnly(2015, 6, 2), TimeSpan.FromHours(30), PlannedTimeTypeEnum.Scheduled);
			_target.SetTimeOnDate(new DateOnly(2015, 6, 3), TimeSpan.FromHours(30), PlannedTimeTypeEnum.Scheduled);
			_target.SetTimeOnDate(new DateOnly(2015, 6, 4), TimeSpan.FromHours(30), PlannedTimeTypeEnum.Scheduled);

			Assert.AreEqual(TimeSpan.FromHours(20), _target.GetOverstaffTimeOnDate(new DateOnly(2015, 06, 4)));
		}

		[Test]
		public void GetScheduledTimeOnDateShouldNotIncludeOverstaffed()
		{
			setupOpenDays();

			Assert.AreEqual(TimeSpan.Zero, _target.GetOverstaffTimeOnDate(new DateOnly(2015, 06, 4)));

			_target.SetTimeOnDate(new DateOnly(2015, 6, 1), TimeSpan.FromHours(30), PlannedTimeTypeEnum.Scheduled);
			_target.SetTimeOnDate(new DateOnly(2015, 6, 2), TimeSpan.FromHours(30), PlannedTimeTypeEnum.Scheduled);
			_target.SetTimeOnDate(new DateOnly(2015, 6, 3), TimeSpan.FromHours(30), PlannedTimeTypeEnum.Scheduled);
			_target.SetTimeOnDate(new DateOnly(2015, 6, 4), TimeSpan.FromHours(30), PlannedTimeTypeEnum.Scheduled);

			Assert.AreEqual(TimeSpan.FromHours(10), _target.GetScheduledTimeOnDate(new DateOnly(2015, 06, 4)));
		}

		[Test]
		public void GetOverstaffTimeOnDateShouldWorkForManualTime()
		{
			setupOpenDays();

			Assert.AreEqual(TimeSpan.Zero, _target.GetOverstaffTimeOnDate(new DateOnly(2015, 06, 4)));

			_target.SetTimeOnDate(new DateOnly(2015, 6, 1), TimeSpan.FromHours(30), PlannedTimeTypeEnum.Manual);
			_target.SetTimeOnDate(new DateOnly(2015, 6, 2), TimeSpan.FromHours(30), PlannedTimeTypeEnum.Manual);
			_target.SetTimeOnDate(new DateOnly(2015, 6, 3), TimeSpan.FromHours(30), PlannedTimeTypeEnum.Manual);
			_target.SetTimeOnDate(new DateOnly(2015, 6, 4), TimeSpan.FromHours(30), PlannedTimeTypeEnum.Manual);

			Assert.AreEqual(TimeSpan.FromHours(20), _target.GetOverstaffTimeOnDate(new DateOnly(2015, 06, 4)));
		}

		[Test]
		public void IfThereIsABacklogAfterLastDateItShouldBeReportedByGetTimeOutsideSLA()
		{
			setupOpenDays();

			_target.SetTimeOnDate(new DateOnly(2015, 6, 1), TimeSpan.FromHours(10), PlannedTimeTypeEnum.Scheduled);
			_target.SetTimeOnDate(new DateOnly(2015, 6, 2), TimeSpan.FromHours(10), PlannedTimeTypeEnum.Scheduled);
			_target.SetTimeOnDate(new DateOnly(2015, 6, 3), TimeSpan.FromHours(10), PlannedTimeTypeEnum.Scheduled);
			_target.SetTimeOnDate(new DateOnly(2015, 6, 4), TimeSpan.FromHours(10), PlannedTimeTypeEnum.Scheduled);
			_target.SetTimeOnDate(new DateOnly(2015, 6, 5), TimeSpan.FromHours(10), PlannedTimeTypeEnum.Scheduled);

			Assert.AreEqual(TimeSpan.FromHours(50), _target.GetTimeOutsideSLA());
		}

		[Test]
		public void ShouldGetBacklogForLastDay()
		{
			setupOpenDays();
			_target.SetTimeOnDate(new DateOnly(2015, 6, 1), TimeSpan.FromHours(10), PlannedTimeTypeEnum.Scheduled);

			_target.GetBacklogOnDate(new DateOnly(2015, 6, 7)).Should().Be.EqualTo(new TimeSpan(10, 0, 0));
		}

		[Test]
		public void ShouldModifyRealPlanTimeIfItExist()
		{
			var date = new DateOnly(2015, 6, 1);
			_target.SetRealPlannedTimeOnDate(date, new TimeSpan(20, 0, 0));
			_target.SetRealPlannedTimeOnDate(date, new TimeSpan(10, 0, 0));

			_target.GetRealPlannedTimeOnDate(date).Should().Be.EqualTo(new TimeSpan(10, 0, 0));
		}		
		
		[Test]
		public void ShouldModifyRealScheduledTimeIfItExist()
		{
			var date = new DateOnly(2015, 6, 1);
			_target.SetRealScheduledTimeOnDate(date, new TimeSpan(20, 0, 0));
			_target.SetRealScheduledTimeOnDate(date, new TimeSpan(10, 0, 0));

			_target.GetRealScheduledTimeOnDate(date).Should().Be.EqualTo(new TimeSpan(10, 0, 0));
		}		
		
		[Test]
		public void ShouldModifyManualPlannedTimeIfItExist()
		{
			var date = new DateOnly(2015, 6, 1);
			_target.SetManualPlannedInfoOnDate(date, true);
			_target.SetManualPlannedInfoOnDate(date, true);

			_target.GetManualPlannedInfoOnDate(date).Should().Be.True();
		}

		private void setupOpenDays()
		{
			_target.Open(new DateOnly(2015, 6, 1));
			_target.Open(new DateOnly(2015, 6, 2));
			_target.Open(new DateOnly(2015, 6, 3));
			_target.Open(new DateOnly(2015, 6, 4));
			_target.Open(new DateOnly(2015, 6, 5));
			_target.Close(new DateOnly(2015, 6, 6));
			_target.Close(new DateOnly(2015, 6, 7));
			_target.RecalculateDistribution();
		}
	}
}