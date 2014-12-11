using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Overtime
{
	[TestFixture]
	public class OvertimeRelativeDifferenceCalculatorTest
	{
		private OvertimeRelativeDifferenceCalculator _target;
		private MockRepository _mock;
		private IAnalyzePersonAccordingToAvailability _analyzePersonAccordingToAvailability;
		private List<OvertimePeriodValue> _mappedData;
		private DateTimePeriod _periodBefore1;
		private DateTimePeriod _periodBefore2;
		private DateTimePeriod _periodBefore3;
		private DateTimePeriod _periodBefore4;
		private DateTimePeriod _periodBefore5;
		private DateTimePeriod _periodBefore6;
		private DateTimePeriod _periodAfter1;
		private DateTimePeriod _periodAfter2;
		private DateTimePeriod _periodAfter3;
		private DateTimePeriod _periodAfter4;
		private DateTimePeriod _periodAfter5;
		private DateTimePeriod _periodAfter6;
		private IScheduleDay _scheduleDay;
		private DateTime _shiftEndingTime;

		[SetUp]
		public void SetUp()
		{
			_mock = new MockRepository();
			_analyzePersonAccordingToAvailability = _mock.StrictMock<IAnalyzePersonAccordingToAvailability>();
			_target = new OvertimeRelativeDifferenceCalculator(_analyzePersonAccordingToAvailability);

			_periodBefore1 = new DateTimePeriod(new DateTime(2014, 02, 26, 14, 00, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 14, 15, 0, DateTimeKind.Utc));
			_periodBefore2 = new DateTimePeriod(new DateTime(2014, 02, 26, 14, 15, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 14, 30, 0, DateTimeKind.Utc));
			_periodBefore3 = new DateTimePeriod(new DateTime(2014, 02, 26, 14, 30, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 14, 45, 0, DateTimeKind.Utc));
			_periodBefore4 = new DateTimePeriod(new DateTime(2014, 02, 26, 14, 45, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 15, 00, 0, DateTimeKind.Utc));
			_periodBefore5 = new DateTimePeriod(new DateTime(2014, 02, 26, 15, 00, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 15, 15, 0, DateTimeKind.Utc));
			_periodBefore6 = new DateTimePeriod(new DateTime(2014, 02, 26, 15, 15, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 15, 30, 0, DateTimeKind.Utc));


			_periodAfter1 = new DateTimePeriod(new DateTime(2014, 02, 26, 16, 30, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 16, 45, 0, DateTimeKind.Utc));
			_periodAfter2 = new DateTimePeriod(new DateTime(2014, 02, 26, 16, 45, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 17, 00, 0, DateTimeKind.Utc));
			_periodAfter3 = new DateTimePeriod(new DateTime(2014, 02, 26, 17, 00, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 17, 15, 0, DateTimeKind.Utc));
			_periodAfter4 = new DateTimePeriod(new DateTime(2014, 02, 26, 17, 15, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 17, 30, 0, DateTimeKind.Utc));
			_periodAfter5 = new DateTimePeriod(new DateTime(2014, 02, 26, 17, 30, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 17, 45, 0, DateTimeKind.Utc));
			_periodAfter6 = new DateTimePeriod(new DateTime(2014, 02, 26, 17, 45, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 18, 00, 0, DateTimeKind.Utc));

			_mappedData = new List<OvertimePeriodValue>
			{
				new OvertimePeriodValue(_periodBefore1, -25.0),
				new OvertimePeriodValue(_periodBefore2, -20.0),
				new OvertimePeriodValue(_periodBefore3, -7.0),
				new OvertimePeriodValue(_periodBefore4, -6.0),
				new OvertimePeriodValue(_periodBefore5, 3.0),
				new OvertimePeriodValue(_periodBefore6, 2.0),
				new OvertimePeriodValue(_periodAfter1, 1.0),
				new OvertimePeriodValue(_periodAfter2, -2.0),
				new OvertimePeriodValue(_periodAfter3, -7.0),
				new OvertimePeriodValue(_periodAfter4, -8.0),
				new OvertimePeriodValue(_periodAfter5, -9.0),
				new OvertimePeriodValue(_periodAfter6, -10.0)
			};

			_scheduleDay = _mock.StrictMock<IScheduleDay>();
			_shiftEndingTime = new DateTime(2014, 02, 26, 16, 30, 0, DateTimeKind.Utc);
		}

		[Test]
		public void ShouldCalculate()
		{
			var dateTimePeriodBefore = new DateTimePeriod(_periodBefore1.StartDateTime, _periodBefore6.EndDateTime);
			var overtimePeriodHolderBefore = new OvertimeDateTimePeriodHolder();
			overtimePeriodHolderBefore.Add(dateTimePeriodBefore);

			var dateTimePeriodAfter = new DateTimePeriod(_periodAfter1.StartDateTime, _periodAfter6.EndDateTime);
			var overtimePeriodHolderAfter = new OvertimeDateTimePeriodHolder();
			overtimePeriodHolderAfter.Add(dateTimePeriodAfter);

			var overtimePeriodHolders = new List<IOvertimeDateTimePeriodHolder> { overtimePeriodHolderBefore, overtimePeriodHolderAfter };

			var result = _target.Calculate(overtimePeriodHolders, _mappedData, false, _scheduleDay);
			Assert.AreEqual(2, result.Count);
			Assert.AreEqual(-53d, result[0].TotalValue());
			Assert.AreEqual(-35d, result[1].TotalValue());
		}

		[Test]
		public void ShouldConsiderOvertimeAvailability()
		{
			var dateTimePeriodBefore = new DateTimePeriod(_periodBefore1.StartDateTime, _periodBefore6.EndDateTime);
			var overtimePeriodHolderBefore = new OvertimeDateTimePeriodHolder();
			overtimePeriodHolderBefore.Add(dateTimePeriodBefore);
			var overtimePeriodHolders = new List<IOvertimeDateTimePeriodHolder> { overtimePeriodHolderBefore};
			var dateOnly = new DateOnly(_shiftEndingTime);
			var person = PersonFactory.CreatePersonWithBasicPermissionInfo("logon", "password");
			IDateOnlyAsDateTimePeriod dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(dateOnly, person.PermissionInformation.DefaultTimeZone());
			var dateTimePeriod = new DateTimePeriod(_shiftEndingTime, _shiftEndingTime.AddMinutes(45));

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.Person).Return(person).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(dateOnlyAsDateTimePeriod).Repeat.AtLeastOnce();
				Expect.Call(_analyzePersonAccordingToAvailability.AdustOvertimeAvailability(_scheduleDay, dateOnly, person.PermissionInformation.DefaultTimeZone(), new List<DateTimePeriod>())).Return(new List<DateTimePeriod> { dateTimePeriod }).IgnoreArguments().Repeat.AtLeastOnce();	
			}

			using (_mock.Playback())
			{
				var result = _target.Calculate(overtimePeriodHolders, _mappedData, true, _scheduleDay);
				Assert.AreEqual(1, result.Count);
				Assert.AreEqual(-8d, result[0].TotalValue());
			}	
		}

		[Test]
		public void ShouldSkipWhenNoSkillData()
		{
			var dateTimePeriodBefore = new DateTimePeriod(_periodBefore1.StartDateTime, _periodBefore6.EndDateTime);
			var overtimePeriodHolderBefore = new OvertimeDateTimePeriodHolder();
			overtimePeriodHolderBefore.Add(dateTimePeriodBefore);

			var dateTimePeriodAfter = new DateTimePeriod(_periodAfter1.StartDateTime, _periodAfter6.EndDateTime);
			var overtimePeriodHolderAfter = new OvertimeDateTimePeriodHolder();
			overtimePeriodHolderAfter.Add(dateTimePeriodAfter);

			_mappedData.Clear();

			var overtimePeriodHolders = new List<IOvertimeDateTimePeriodHolder> { overtimePeriodHolderBefore, overtimePeriodHolderAfter };

			var result = _target.Calculate(overtimePeriodHolders, _mappedData, false, _scheduleDay);
			Assert.AreEqual(0, result.Count);
		}
	}
}
