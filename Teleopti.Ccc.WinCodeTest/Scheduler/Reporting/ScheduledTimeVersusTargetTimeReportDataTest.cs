using System;
using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ScheduleReporting;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.Reporting
{
	[TestFixture]
	public class ScheduledTimeVersusTargetTimeReportDataTest
	{
		private ScheduledTimeVersusTargetTimeReportData _reportData;

		[SetUp]
		public void Setup()
		{
			_reportData = new ScheduledTimeVersusTargetTimeReportData();
		}

		[Test]
		public void ShouldGetSetProperties()
		{
			_reportData.PersonName = "name";
			_reportData.PeriodFrom = new DateTime(2011, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			_reportData.PeriodTo = new DateTime(2011, 1, 2, 0, 0, 0, DateTimeKind.Utc);
			_reportData.TargetTime = 9600;
			_reportData.TargetDayOffs = 8;
			_reportData.ScheduledTime = 9540;
			_reportData.ScheduledDayOffs = 7;

			Assert.AreEqual("name", _reportData.PersonName);
			Assert.AreEqual(new DateTime(2011, 1, 1, 0, 0, 0, DateTimeKind.Utc), _reportData.PeriodFrom);
			Assert.AreEqual(new DateTime(2011, 1, 2, 0, 0, 0, DateTimeKind.Utc), _reportData.PeriodTo);
			Assert.AreEqual(9600, _reportData.TargetTime);
			Assert.AreEqual(8, _reportData.TargetDayOffs);
			Assert.AreEqual(9540, _reportData.ScheduledTime);
			Assert.AreEqual(7, _reportData.ScheduledDayOffs);
			Assert.AreEqual(-60, _reportData.DifferenceTime);
			Assert.AreEqual(-1, _reportData.DifferenceDayOffs);
		}
	}
}
