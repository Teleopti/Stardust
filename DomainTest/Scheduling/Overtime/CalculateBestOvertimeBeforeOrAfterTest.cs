using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Overtime
{
	[TestFixture]
	public class CalculateBestOvertimeBeforeOrAfterTest
	{
		private CalculateBestOvertimeBeforeOrAfter _target;
		private List<OvertimePeriodValue> _mappedData;
		
		private IScheduleDay _scheduleDay;
		private IProjectionService _projectionService;
		private IVisualLayerCollection _visualLayerCollection;
		private DateTimePeriod _shiftDateTimePeriod;
		private MockRepository _mock;
		private MinMax<TimeSpan> _overtimeSpecifiedPeriod;
		private DateTimePeriod _scheduleDayPeriod;
		private IOvertimeDateTimePeriodExtractor _overtimeDateTimePeriodExtractor;
		private IOvertimeRelativeDifferenceCalculator _overtimeRelativeDifferenceCalculator;
		private IOvertimePeriodValueMapper _overtimePeriodValueMapper;
		private MinMax<TimeSpan> _overtimeDuration;
		private DateTimePeriod _specifiedPeriod;
		private IList<DateTimePeriod> _overtimePeriodHolders;
		private IList<IOvertimePeriodValue> _overtimePeriodValues;
			
		[SetUp]
		public void SetUp()
		{
			_mock = new MockRepository();
			_overtimeDateTimePeriodExtractor = _mock.StrictMock<IOvertimeDateTimePeriodExtractor>();
			_overtimeRelativeDifferenceCalculator = _mock.StrictMock<IOvertimeRelativeDifferenceCalculator>();
			_overtimePeriodValueMapper = _mock.StrictMock<IOvertimePeriodValueMapper>();
			_target = new CalculateBestOvertimeBeforeOrAfter(_overtimeDateTimePeriodExtractor, _overtimeRelativeDifferenceCalculator, _overtimePeriodValueMapper);
			_mappedData = new List<OvertimePeriodValue>();	
			_scheduleDay = _mock.StrictMock<IScheduleDay>();
			_projectionService = _mock.StrictMock<IProjectionService>();
			_visualLayerCollection = _mock.StrictMock<IVisualLayerCollection>();
			_overtimeSpecifiedPeriod = new MinMax<TimeSpan>(TimeSpan.Zero, TimeSpan.FromDays(1).Add(TimeSpan.FromHours(6)));

			var scheduleDayStart = new DateTime(2014, 02, 26, 0, 0, 0, DateTimeKind.Utc);
			var scheduleDayEnd = scheduleDayStart.AddDays(1);
			_scheduleDayPeriod = new DateTimePeriod(scheduleDayStart, scheduleDayEnd);
			_overtimeDuration = new MinMax<TimeSpan>(new TimeSpan(0, 1, 0, 0), new TimeSpan(0, 1, 0, 0));
			_specifiedPeriod = new DateTimePeriod(_scheduleDayPeriod.StartDateTime, _scheduleDayPeriod.EndDateTime.Add(TimeSpan.FromHours(6)));

			_shiftDateTimePeriod = new DateTimePeriod(new DateTime(2014, 02, 26, 15, 30, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 16, 30, 0, DateTimeKind.Utc));
			var dateTimePeriodBefore = new DateTimePeriod(_shiftDateTimePeriod.StartDateTime.AddMinutes(-90), _shiftDateTimePeriod.StartDateTime);
			var dateTimePeriodAfter = new DateTimePeriod(_shiftDateTimePeriod.EndDateTime, _shiftDateTimePeriod.EndDateTime.AddMinutes(90));

			_overtimePeriodHolders = new List<DateTimePeriod> { dateTimePeriodBefore, dateTimePeriodAfter };

			var overtimePeriodValueAfter = new OvertimePeriodValue(new DateTimePeriod(new DateTime(2014, 02, 26, 16, 30, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 17, 30, 0, DateTimeKind.Utc)), -7d);
			var overtimePeriodValueBefore = new OvertimePeriodValue(new DateTimePeriod(new DateTime(2014, 02, 26, 14, 30, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 15, 30, 0, DateTimeKind.Utc)), -4d);

			_overtimePeriodValues = new List<IOvertimePeriodValue> { overtimePeriodValueBefore, overtimePeriodValueAfter };
		}

		[Test]
		public void ShouldGetBestOvertimesSorted()
		{
			var expected = new DateTimePeriod(new DateTime(2014, 02, 26, 16, 30, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 17, 30, 0, DateTimeKind.Utc));

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_scheduleDay.Period).Return(_scheduleDayPeriod);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_overtimePeriodValueMapper.Map(null)).Return(new List<OvertimePeriodValue>());
				Expect.Call(_overtimeDateTimePeriodExtractor.Extract(15, _overtimeDuration, _visualLayerCollection, _specifiedPeriod, null)).Return(_overtimePeriodHolders);
				Expect.Call(_overtimeRelativeDifferenceCalculator.Calculate(_overtimePeriodHolders, _mappedData, false, _scheduleDay)).Return(_overtimePeriodValues);
			}

			using (_mock.Playback())
			{
				var result = _target.GetBestOvertime(_overtimeDuration, _overtimeSpecifiedPeriod, _scheduleDay, 15, false, null);
				Assert.AreEqual(2, result.Count());
				Assert.AreEqual(expected, result.First());
			}
		}

		
		[Test]
		public void ShouldOnlyConsiderResultvaluesBelowZero()
		{
			var overtimePeriodValueAfter = new OvertimePeriodValue(new DateTimePeriod(new DateTime(2014, 02, 26, 16, 30, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 17, 30, 0, DateTimeKind.Utc)), 1d);
			var overtimePeriodValueBefore = new OvertimePeriodValue(new DateTimePeriod(new DateTime(2014, 02, 26, 14, 30, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 15, 30, 0, DateTimeKind.Utc)), 2d);

			_overtimePeriodValues = new List<IOvertimePeriodValue> { overtimePeriodValueBefore, overtimePeriodValueAfter };

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_scheduleDay.Period).Return(_scheduleDayPeriod);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_overtimePeriodValueMapper.Map(null)).Return(new List<OvertimePeriodValue>());
				Expect.Call(_overtimeDateTimePeriodExtractor.Extract(15, _overtimeDuration, _visualLayerCollection, _specifiedPeriod, null)).Return(_overtimePeriodHolders);
				Expect.Call(_overtimeRelativeDifferenceCalculator.Calculate(_overtimePeriodHolders, _mappedData, false, _scheduleDay)).Return(_overtimePeriodValues);
			}

			using (_mock.Playback())
			{
				var result = _target.GetBestOvertime(_overtimeDuration, _overtimeSpecifiedPeriod, _scheduleDay, 15, false, null);
				Assert.AreEqual(0, result.Count());
			}
		}
	}
}
