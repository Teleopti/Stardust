using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Overtime
{
    [TestFixture]
    public class CalculateBestOvertimeTest
    {
        private ICalculateBestOvertime _target;
        private DateTimePeriod _period1;
        private DateTimePeriod _period2;
        private DateTimePeriod _period3;
        private DateTimePeriod _period4;
        private DateTimePeriod _period5;
        private DateTimePeriod _period6;
        private DateTime _shiftEndingTime;
	    private IScheduleDay _scheduleDay;
	    private MockRepository _mock;
        private List<OvertimePeriodValue> _mappedData;
	    private IProjectionService _projectionService;
	    private IVisualLayerCollection _visualLayerCollection;
	    private DateTimePeriod _dateTimePeriod;
	    private IAnalyzePersonAccordingToAvailability _analyzePersonAccordingToAvailability;
		private MinMax<TimeSpan> _overtimeSpecifiedPeriod;
	    private DateTimePeriod _scheduleDayPeriod;
	    private IVisualLayer _visualLayer;
	    private IMultiplicatorDefinitionSet _multiplicatorDefinitionSet;
	    private IList<IVisualLayer> _visualLayers;
	    private IPerson _person;
		
		[SetUp]
        public void Setup()
        {
			_mock = new MockRepository();
	        _analyzePersonAccordingToAvailability = _mock.StrictMock<IAnalyzePersonAccordingToAvailability>();
            _target = new CalculateBestOvertime(_analyzePersonAccordingToAvailability);
            _period1 = new DateTimePeriod(new DateTime(2014, 02, 26, 16, 30, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 16, 45, 0, DateTimeKind.Utc));
            _period2 = new DateTimePeriod(new DateTime(2014, 02, 26, 16, 45, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 17, 00, 0, DateTimeKind.Utc));
            _period3 = new DateTimePeriod(new DateTime(2014, 02, 26, 17, 00, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 17, 15, 0, DateTimeKind.Utc));
            _period4 = new DateTimePeriod(new DateTime(2014, 02, 26, 17, 15, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 17, 30, 0, DateTimeKind.Utc));
            _period5 = new DateTimePeriod(new DateTime(2014, 02, 26, 17, 30, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 17, 45, 0, DateTimeKind.Utc));
            _period6 = new DateTimePeriod(new DateTime(2014, 02, 26, 17, 45, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 18, 00, 0, DateTimeKind.Utc));
            _shiftEndingTime = new DateTime(2014, 02, 26, 16, 30, 0, DateTimeKind.Utc);
            _mappedData = new List<OvertimePeriodValue>();
            _mappedData.Add(new OvertimePeriodValue(_period1, 0.78));
            _mappedData.Add(new OvertimePeriodValue(_period2, 1.52));
            _mappedData.Add(new OvertimePeriodValue(_period3, -5.98));
            _mappedData.Add(new OvertimePeriodValue(_period4, -3.55));
            _mappedData.Add(new OvertimePeriodValue(_period5, -6.75));
            _mappedData.Add(new OvertimePeriodValue(_period6, -4.6));
			
	        _scheduleDay = _mock.StrictMock<IScheduleDay>();
	        _projectionService = _mock.StrictMock<IProjectionService>();
	        _visualLayerCollection = _mock.StrictMock<IVisualLayerCollection>();
			_dateTimePeriod = new DateTimePeriod(_shiftEndingTime.AddHours(-1), _shiftEndingTime);
			_overtimeSpecifiedPeriod = new MinMax<TimeSpan>(TimeSpan.Zero, TimeSpan.FromDays(2));
	        var scheduleDayStart = new DateTime(2014, 02, 26, 0, 0, 0, DateTimeKind.Utc);
	        var scheduleDayEnd = scheduleDayStart.AddDays(1);
			_scheduleDayPeriod = new DateTimePeriod(scheduleDayStart, scheduleDayEnd);
	       
	        _multiplicatorDefinitionSet = _mock.StrictMock<IMultiplicatorDefinitionSet>();
			_person = PersonFactory.CreatePerson("name");
			var activity = ActivityFactory.CreateActivity("activity");
			_visualLayer = new VisualLayer(activity, new DateTimePeriod(_shiftEndingTime.AddMinutes(-15), _shiftEndingTime), activity, _person);
			_visualLayers = new List<IVisualLayer> { _visualLayer };
			
        }

        [Test]
        public void TestForOvertimeWithExactOrEqualLimit()
        {
            var oneHourTimeSpan = new TimeSpan(0, 1, 0, 0);
            var overtimeDurantion = new MinMax<TimeSpan>(oneHourTimeSpan, oneHourTimeSpan);

	        using (_mock.Record())
	        {
		        Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_visualLayerCollection.GetEnumerator()).Return(_visualLayers.GetEnumerator());

		        Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
		        Expect.Call(_scheduleDay.Period).Return(_scheduleDayPeriod);
		        Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
		        Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
	        }

	        using (_mock.Playback())
	        {
				Assert.AreEqual(_target.GetBestOvertime(overtimeDurantion, _overtimeSpecifiedPeriod, _mappedData, _scheduleDay, 15, false, null).First().ElapsedTime(), oneHourTimeSpan);	   
	        }   
        }

		[Test]
	    public void ShouldReturnEmptyListWhenLastLayerIsOvertime()
	    {
			var oneHourTimeSpan = new TimeSpan(0, 1, 0, 0);
			var overtimeDurantion = new MinMax<TimeSpan>(oneHourTimeSpan, oneHourTimeSpan);
			((VisualLayer) _visualLayer).DefinitionSet = _multiplicatorDefinitionSet;
			_visualLayers = new List<IVisualLayer>{_visualLayer};

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.GetEnumerator()).Return(_visualLayers.GetEnumerator());
				Expect.Call(_multiplicatorDefinitionSet.MultiplicatorType).Return(MultiplicatorType.Overtime);
			}

			using (_mock.Playback())
			{
				var result = _target.GetBestOvertime(overtimeDurantion, _overtimeSpecifiedPeriod, _mappedData, _scheduleDay, 15, false, null);
				Assert.AreEqual(0, result.Count);
			}
	    }

		[Test]
	    public void ShouldReturnEmptyListWhenLastLayerIsAbsence()
	    {
			var oneHourTimeSpan = new TimeSpan(0, 1, 0, 0);
			var overtimeDurantion = new MinMax<TimeSpan>(oneHourTimeSpan, oneHourTimeSpan);
		    var absence = AbsenceFactory.CreateAbsence("absence");
			_visualLayer = new VisualLayer(absence, new DateTimePeriod(_shiftEndingTime.AddMinutes(-15), _shiftEndingTime), ActivityFactory.CreateActivity("activity"), _person);
		    ((VisualLayer) _visualLayer).HighestPriorityAbsence = absence; 
			_visualLayers = new List<IVisualLayer> { _visualLayer };

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.GetEnumerator()).Return(_visualLayers.GetEnumerator());
			}

			using (_mock.Playback())
			{
				var result = _target.GetBestOvertime(overtimeDurantion, _overtimeSpecifiedPeriod, _mappedData, _scheduleDay, 15, false, null);
				Assert.AreEqual(0, result.Count);
			}    
	    }

        [Test]
        public void TestForOvertimeFrom0To1Hour()
        {
            var oneHourTimeSpan = new TimeSpan(0, 0, 0, 0);
            var overtimeDurantion = new MinMax<TimeSpan>(oneHourTimeSpan, oneHourTimeSpan.Add(TimeSpan.FromHours(1)));

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_visualLayerCollection.GetEnumerator()).Return(_visualLayers.GetEnumerator());

				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_scheduleDay.Period).Return(_scheduleDayPeriod);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
			}

	        using (_mock.Playback())
	        {
				Assert.AreEqual(_target.GetBestOvertime(overtimeDurantion, _overtimeSpecifiedPeriod, _mappedData, _scheduleDay, 15, false, null).First().ElapsedTime(), oneHourTimeSpan.Add(TimeSpan.FromHours(1)));   
	        }     
        }

        [Test]
        public void TestForOvertimeFrom15MinutesTo1Hour()
        {
            var overtimeLimitStartTimeSpan = new TimeSpan(0, 0, 15, 0);
            var overtimeLimitEndTimeSpan = new TimeSpan(0, 1, 0, 0);
            var overtimeDurantion = new MinMax<TimeSpan>(overtimeLimitStartTimeSpan, overtimeLimitEndTimeSpan);

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_visualLayerCollection.GetEnumerator()).Return(_visualLayers.GetEnumerator());

				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_scheduleDay.Period).Return(_scheduleDayPeriod);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
			}

			using (_mock.Playback())
			{
				Assert.AreEqual(_target.GetBestOvertime(overtimeDurantion, _overtimeSpecifiedPeriod, _mappedData, _scheduleDay, 15, false, null).First().ElapsedTime(), overtimeLimitEndTimeSpan);
			}    
        }

        [Test]
        public void TestForOvertimeFrom15MinutesTo3Hour()
        {
            var overtimeLimitStartTimeSpan = new TimeSpan(0, 0, 15, 0);
            var overtimeLimitEndTimeSpan = new TimeSpan(0, 3, 0, 0);
            var overtimeDurantion = new MinMax<TimeSpan>(overtimeLimitStartTimeSpan, overtimeLimitEndTimeSpan);

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_visualLayerCollection.GetEnumerator()).Return(_visualLayers.GetEnumerator());

				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_scheduleDay.Period).Return(_scheduleDayPeriod);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
			}

			using (_mock.Playback())
			{
				Assert.AreEqual(new TimeSpan(0, 1, 30, 0), _target.GetBestOvertime(overtimeDurantion, _overtimeSpecifiedPeriod, _mappedData, _scheduleDay, 15, false, null).First().ElapsedTime());
			}
        }

        [Test]
        public void TestForOvertimeWithLongerLimit()
        {
            var overtimeLimitStartTimeSpan = new TimeSpan(0, 3, 0, 0);
            var overtimeLimitEndTimeSpan = new TimeSpan(0, 3, 0, 0);
            var overtimeDurantion = new MinMax<TimeSpan>(overtimeLimitStartTimeSpan, overtimeLimitEndTimeSpan);

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_visualLayerCollection.GetEnumerator()).Return(_visualLayers.GetEnumerator());

				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_scheduleDay.Period).Return(_scheduleDayPeriod);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
			}

			using (_mock.Playback())
			{
				Assert.AreEqual(0, _target.GetBestOvertime(overtimeDurantion, _overtimeSpecifiedPeriod, _mappedData, _scheduleDay, 15, false, null).Count);
			}    
        }

        [Test]
        public void TestForOvertimeWithNoOvertimeFound()
        {
            var overtimeLimitStartTimeSpan = new TimeSpan(0, 0, 0, 0);
            var overtimeLimitEndTimeSpan = new TimeSpan(0, 3, 0, 0);
            var overtimeDurantion = new MinMax<TimeSpan>(overtimeLimitStartTimeSpan, overtimeLimitEndTimeSpan);

            var mappedData = new List<OvertimePeriodValue>();
            mappedData.Add(new OvertimePeriodValue(_period1, 0.78));
            mappedData.Add(new OvertimePeriodValue(_period2, 1.52));
            mappedData.Add(new OvertimePeriodValue(_period3, 5.98));
            mappedData.Add(new OvertimePeriodValue(_period4, 3.55));

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_visualLayerCollection.GetEnumerator()).Return(_visualLayers.GetEnumerator());

				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_scheduleDay.Period).Return(_scheduleDayPeriod);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
			}

			using (_mock.Playback())
			{
				Assert.AreEqual(0, _target.GetBestOvertime(overtimeDurantion, _overtimeSpecifiedPeriod, mappedData, _scheduleDay, 15, false, null).Count);
			}    
        }

        [Test]
        public void TestForOvertimeWithDiffRange()
        {
            var overtimeLimitStartTimeSpan = new TimeSpan(0, 0, 30, 0);
            var overtimeLimitEndTimeSpan = new TimeSpan(0, 0, 45, 0);
            var overtimeDurantion = new MinMax<TimeSpan>(overtimeLimitStartTimeSpan, overtimeLimitEndTimeSpan);

            var mappedData = new List<OvertimePeriodValue>();
            mappedData.Add(new OvertimePeriodValue(_period1, 0.78));
            mappedData.Add(new OvertimePeriodValue(_period2, 1.52));
            mappedData.Add(new OvertimePeriodValue(_period3, -1));
            mappedData.Add(new OvertimePeriodValue(_period4, -3.55));

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_visualLayerCollection.GetEnumerator()).Return(_visualLayers.GetEnumerator());

				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_scheduleDay.Period).Return(_scheduleDayPeriod);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
			}

			using (_mock.Playback())
			{
				Assert.AreEqual(0, _target.GetBestOvertime(overtimeDurantion, _overtimeSpecifiedPeriod, mappedData, _scheduleDay, 15, false, null).Count);
			}     
        }

        [Test]
        public void ShouldAllwaysConsiderAllInteralsFromEndOfTheShift()
        {
            var mappedData = new List<OvertimePeriodValue>();
            mappedData.Add(new OvertimePeriodValue(_period1, -1));
            mappedData.Add(new OvertimePeriodValue(_period2, 2));
            mappedData.Add(new OvertimePeriodValue(_period3, -5.98));
            mappedData.Add(new OvertimePeriodValue(_period4, -3.55));

            var overtimeLimitStartTimeSpan = new TimeSpan(0, 0, 0, 0);
            var overtimeLimitEndTimeSpan = new TimeSpan(0, 0, 30, 0);
            var overtimeDurantion = new MinMax<TimeSpan>(overtimeLimitStartTimeSpan, overtimeLimitEndTimeSpan);

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_visualLayerCollection.GetEnumerator()).Return(_visualLayers.GetEnumerator());

				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_scheduleDay.Period).Return(_scheduleDayPeriod);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
			}

			using (_mock.Playback())
			{
				Assert.AreEqual(TimeSpan.FromMinutes(15), _target.GetBestOvertime(overtimeDurantion, _overtimeSpecifiedPeriod, mappedData, _scheduleDay, 15, false, null).First().ElapsedTime());
			}  
        }

		[Test]
	    public void ShouldReturnEmptyListWhenNoDuration()
	    {
			var mappedData = new List<OvertimePeriodValue>();
			mappedData.Add(new OvertimePeriodValue(_period1, -1));
			mappedData.Add(new OvertimePeriodValue(_period2, 2));
			mappedData.Add(new OvertimePeriodValue(_period3, -5.98));
			mappedData.Add(new OvertimePeriodValue(_period4, -3.55));

			var overtimeLimitStartTimeSpan = new TimeSpan(0, 0, 30, 0);
			var overtimeLimitEndTimeSpan = new TimeSpan(0, 0, 30, 0);
			var overtimeDurantion = new MinMax<TimeSpan>(overtimeLimitStartTimeSpan, overtimeLimitEndTimeSpan);

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_visualLayerCollection.GetEnumerator()).Return(_visualLayers.GetEnumerator());

				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_scheduleDay.Period).Return(_scheduleDayPeriod);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
			}

			using (_mock.Playback())
			{
				Assert.AreEqual(0, _target.GetBestOvertime(overtimeDurantion, _overtimeSpecifiedPeriod, mappedData, _scheduleDay, 15, false, null).Count);
			}      
	    }

        [Test]
        public void ShouldOnlyPutOvertimeIfSumOfRelativeDifferencesIsNegative()
        {
            var mappedData = new List<OvertimePeriodValue>();
            mappedData.Add(new OvertimePeriodValue(_period1, 1));
            mappedData.Add(new OvertimePeriodValue(_period2, 2));
            mappedData.Add(new OvertimePeriodValue(_period3, 3));
            mappedData.Add(new OvertimePeriodValue(_period4, -6));

            var overtimeLimitStartTimeSpan = new TimeSpan(0, 1, 0, 0);
            var overtimeLimitEndTimeSpan = new TimeSpan(0, 1, 0, 0);
            var overtimeDurantion = new MinMax<TimeSpan>(overtimeLimitStartTimeSpan, overtimeLimitEndTimeSpan);

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_visualLayerCollection.GetEnumerator()).Return(_visualLayers.GetEnumerator());

				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_scheduleDay.Period).Return(_scheduleDayPeriod);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
			}

			using (_mock.Playback())
			{
				Assert.AreEqual(0, _target.GetBestOvertime(overtimeDurantion, _overtimeSpecifiedPeriod, mappedData, _scheduleDay, 15, false, null).Count);
			}
        }

        [Test]
        public void ShouldPutOvertimeIfSumOfRelativeDifferencesIsNegative()
        {
            var mappedData = new List<OvertimePeriodValue>();
            mappedData.Add(new OvertimePeriodValue(_period1, 1));
            mappedData.Add(new OvertimePeriodValue(_period2, 2));
            mappedData.Add(new OvertimePeriodValue(_period3, 3));
            mappedData.Add(new OvertimePeriodValue(_period4, -7));

            var overtimeLimitStartTimeSpan = new TimeSpan(0, 0, 0, 0);
            var overtimeLimitEndTimeSpan = new TimeSpan(0, 1, 0, 0);
            var overtimeDurantion = new MinMax<TimeSpan>(overtimeLimitStartTimeSpan, overtimeLimitEndTimeSpan);

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_visualLayerCollection.GetEnumerator()).Return(_visualLayers.GetEnumerator());

				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_scheduleDay.Period).Return(_scheduleDayPeriod);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
			}

			using (_mock.Playback())
			{
				Assert.AreEqual(TimeSpan.FromHours(1), _target.GetBestOvertime(overtimeDurantion, _overtimeSpecifiedPeriod, mappedData, _scheduleDay, 15, false, null).First().ElapsedTime());
			}
        }

        [Test]
        public void ShouldHandleMinSettingOfZeroMaxSettingOfZero()
        {
            var mappedData = new List<OvertimePeriodValue>();
            mappedData.Add(new OvertimePeriodValue(_period1, -1));
            mappedData.Add(new OvertimePeriodValue(_period2, 2));
            mappedData.Add(new OvertimePeriodValue(_period3, -5.98));
            mappedData.Add(new OvertimePeriodValue(_period4, -3.55));

            var overtimeLimitStartTimeSpan = new TimeSpan(0, 0, 0, 0);
            var overtimeLimitEndTimeSpan = new TimeSpan(0, 0, 0, 0);
            var overtimeDurantion = new MinMax<TimeSpan>(overtimeLimitStartTimeSpan, overtimeLimitEndTimeSpan);

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_visualLayerCollection.GetEnumerator()).Return(_visualLayers.GetEnumerator());

				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_scheduleDay.Period).Return(_scheduleDayPeriod);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
			}

			using (_mock.Playback())
			{
				Assert.AreEqual(0, _target.GetBestOvertime(overtimeDurantion, _overtimeSpecifiedPeriod, mappedData, _scheduleDay, 15, false, null).Count);
			}
        }

		[Test]
		public void ShouldAdjustPeriodToOvertimeAvailability()
		{
			var overtimeLimitStartTimeSpan = new TimeSpan(0, 1, 0, 0);
			var overtimeLimitEndTimeSpan = new TimeSpan(0, 1, 0, 0);
			var overtimeDurantion = new MinMax<TimeSpan>(overtimeLimitStartTimeSpan, overtimeLimitEndTimeSpan.Add(TimeSpan.FromMinutes(15)));
			var person = PersonFactory.CreatePersonWithBasicPermissionInfo("logon", "password");
			var dateOnly = new DateOnly(_shiftEndingTime);
			IDateOnlyAsDateTimePeriod dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(dateOnly, person.PermissionInformation.DefaultTimeZone());
			var dateTimePeriod = new DateTimePeriod(_dateTimePeriod.EndDateTime, _dateTimePeriod.EndDateTime.AddMinutes(45));

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_visualLayerCollection.GetEnumerator()).Return(_visualLayers.GetEnumerator());

				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_scheduleDay.Period).Return(_scheduleDayPeriod);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
				Expect.Call(_scheduleDay.Person).Return(person).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(dateOnlyAsDateTimePeriod).Repeat.AtLeastOnce();
				Expect.Call(_analyzePersonAccordingToAvailability.AdustOvertimeAvailability(
						_scheduleDay, dateOnly, person.PermissionInformation.DefaultTimeZone(),
						new List<DateTimePeriod>())).Return(new List<DateTimePeriod>{dateTimePeriod}).IgnoreArguments().Repeat.AtLeastOnce();
			}

			using (_mock.Playback())
			{
				Assert.AreEqual(_target.GetBestOvertime(overtimeDurantion, _overtimeSpecifiedPeriod, _mappedData, _scheduleDay, 15, true, null).First(), dateTimePeriod);
			}       
		}

		[Test]
	    public void ShouldNotAddOvertimeOutsideDefaultPeriod()
	    {
			var overtimeLimitStartTimeSpan = new TimeSpan(0, 1, 0, 0);
			var overtimeLimitEndTimeSpan = new TimeSpan(0, 1, 0, 0);
			var overtimeDurantion = new MinMax<TimeSpan>(overtimeLimitStartTimeSpan, overtimeLimitEndTimeSpan);
			_overtimeSpecifiedPeriod = new MinMax<TimeSpan>(TimeSpan.Zero, TimeSpan.FromHours(10));

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_visualLayerCollection.GetEnumerator()).Return(_visualLayers.GetEnumerator());

				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_scheduleDay.Period).Return(_scheduleDayPeriod);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
			}

			using (_mock.Playback())
			{
				Assert.AreEqual(0, _target.GetBestOvertime(overtimeDurantion, _overtimeSpecifiedPeriod, _mappedData, _scheduleDay, 15, false, null).Count);
			}   
	    }
    }
}
