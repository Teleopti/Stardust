using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Overtime
{
	[TestFixture]
	public class CalculateBestOvertimeBeforeOrAfterTest
	{
		private CalculateBestOvertimeBeforeOrAfter _target;
		private List<OvertimePeriodValue> _mappedData;
		private DateTimePeriod _period1;
		private DateTimePeriod _period2;
		private DateTimePeriod _period3;
		private DateTimePeriod _period4;
		private DateTimePeriod _period5;
		private DateTimePeriod _period6;
		private IScheduleDay _scheduleDay;
		//private IProjectionService _projectionService;
		//private IVisualLayerCollection _visualLayerCollection;
		//private DateTimePeriod _dateTimePeriod;
		private MockRepository _mock;
		//private DateTime _shiftEndingTime;
		//private DateTime _shiftStartTime;

		[SetUp]
		public void SetUp()
		{
			_target = new CalculateBestOvertimeBeforeOrAfter();
			_period1 = new DateTimePeriod(new DateTime(2014, 02, 26, 16, 30, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 16, 45, 0, DateTimeKind.Utc));
			_period2 = new DateTimePeriod(new DateTime(2014, 02, 26, 16, 45, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 17, 00, 0, DateTimeKind.Utc));
			_period3 = new DateTimePeriod(new DateTime(2014, 02, 26, 17, 00, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 17, 15, 0, DateTimeKind.Utc));
			_period4 = new DateTimePeriod(new DateTime(2014, 02, 26, 17, 15, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 17, 30, 0, DateTimeKind.Utc));
			_period5 = new DateTimePeriod(new DateTime(2014, 02, 26, 17, 30, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 17, 45, 0, DateTimeKind.Utc));
			_period6 = new DateTimePeriod(new DateTime(2014, 02, 26, 17, 45, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 18, 00, 0, DateTimeKind.Utc));
			_mappedData = new List<OvertimePeriodValue>();
			_mappedData.Add(new OvertimePeriodValue(_period1, 0.78));
			_mappedData.Add(new OvertimePeriodValue(_period2, 1.52));
			_mappedData.Add(new OvertimePeriodValue(_period3, -5.98));
			_mappedData.Add(new OvertimePeriodValue(_period4, -3.55));
			_mappedData.Add(new OvertimePeriodValue(_period5, -6.75));
			_mappedData.Add(new OvertimePeriodValue(_period6, -4.6));
			_mock = new MockRepository();
			_scheduleDay = _mock.StrictMock<IScheduleDay>();
			//_projectionService = _mock.StrictMock<IProjectionService>();
			//_visualLayerCollection = _mock.StrictMock<IVisualLayerCollection>();
			//_shiftStartTime = new DateTime(2014, 02, 26, 15, 30, 0, DateTimeKind.Utc);
			//_shiftEndingTime = new DateTime(2014, 02, 26, 16, 30, 0, DateTimeKind.Utc);
			//_dateTimePeriod = new DateTimePeriod(_shiftStartTime, _shiftEndingTime);
		}

		[Test]
		public void ShouldCalculate()
		{
			var oneHourTimeSpan = new TimeSpan(0, 1, 0, 0);
			var overtimeDuration = new MinMax<TimeSpan>(oneHourTimeSpan, oneHourTimeSpan);

			var result = _target.GetBestOvertime(overtimeDuration, _mappedData, _scheduleDay, 15);
			Assert.AreEqual(0, result.Count);
		}
	}
}
