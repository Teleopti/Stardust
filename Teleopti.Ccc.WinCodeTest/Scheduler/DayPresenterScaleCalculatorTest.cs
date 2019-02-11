using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Scheduling;


namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
    [TestFixture]
    public class DayPresenterScaleCalculatorTest
    {

        private MockRepository _mocks;
        private ISchedulerStateHolder _stateHolder;
        private IDayPresenterScaleCalculator _target;
        private IDictionary<Guid, IPerson> _persons;
        private IPerson _person;
        private IScheduleDictionary _scheduleDictionary;
        private IScheduleRange _range;
        private IScheduleDay _scheduleDay1;
	    private IVisualLayerCollection _visualLayerCollection1;
	    private IVisualLayerCollection _visualLayerCollection2;
	    private IProjectionService _projectionService1;
	    private IProjectionService _projectionService2;
	    private IScheduleDay _scheduleDay2;


    	[SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _stateHolder = _mocks.StrictMock<ISchedulerStateHolder>();
            _target = new DayPresenterScaleCalculator();
            _person = PersonFactory.CreatePerson();
            _persons = new Dictionary<Guid, IPerson>();
            _persons.Add(Guid.NewGuid(), _person);
            _scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
            _range = _mocks.StrictMock<IScheduleRange>();
            _scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
			TimeZoneGuardForDesktop.Instance.Set(TimeZoneInfo.FindSystemTimeZoneById("UTC"));

    		_scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
    		_visualLayerCollection1 = _mocks.StrictMock<IVisualLayerCollection>();
    		_visualLayerCollection2 = _mocks.StrictMock<IVisualLayerCollection>();
    		_projectionService1 = _mocks.StrictMock<IProjectionService>();
    		_projectionService2 = _mocks.StrictMock<IProjectionService>();
        }

		[TearDown]
		public void Teardown()
		{
			TimeZoneGuardForDesktop.Instance.Set(null);
		}

		[Test]
        public void ScalePeriodShouldAddOneHourToBeginningAndEnd()
        {
            var assPeriod1 = new DateTimePeriod(new DateTime(2011, 1, 1, 8, 0, 0, 0, DateTimeKind.Utc), new DateTime(2011, 1, 1, 16, 0, 0, 0, DateTimeKind.Utc));
            var assPeriod2 = new DateTimePeriod(new DateTime(2011, 1, 2, 8, 0, 0, 0, DateTimeKind.Utc), new DateTime(2011, 1, 2, 16, 0, 0, 0, DateTimeKind.Utc));
        
            using (_mocks.Record())
            {
                Expect.Call(_stateHolder.FilteredCombinedAgentsDictionary).Return(_persons);
                Expect.Call(_stateHolder.Schedules).Return(_scheduleDictionary);
                Expect.Call(_scheduleDictionary[_person]).Return(_range);

                Expect.Call(_range.ScheduledDay(new DateOnly(2011, 01, 01))).Return(_scheduleDay1); 
	            Expect.Call(_scheduleDay1.ProjectionService()).Return(_projectionService1);
	            Expect.Call(_projectionService1.CreateProjection()).Return(_visualLayerCollection1);
	            Expect.Call(_visualLayerCollection1.HasLayers).Return(true);
	            Expect.Call(_visualLayerCollection1.Period()).Return(assPeriod1);

                Expect.Call(_range.ScheduledDay(new DateOnly(2011, 01, 02))).Return(_scheduleDay2);
				Expect.Call(_scheduleDay2.ProjectionService()).Return(_projectionService2);
				Expect.Call(_projectionService2.CreateProjection()).Return(_visualLayerCollection2);
	            Expect.Call(_visualLayerCollection2.HasLayers).Return(true);
				Expect.Call(_visualLayerCollection2.Period()).Return(assPeriod2);
			}

            DateTimePeriod result;

            using (_mocks.Playback())
            {
                result = _target.CalculateScalePeriod(_stateHolder, new DateOnly(2011, 01, 02));
            }

            var expected =
                new DateTimePeriod(new DateTime(2011, 1, 2, 7, 0, 0, 0, DateTimeKind.Utc),
                                   new DateTime(2011, 1, 2, 17, 0, 0, 0, DateTimeKind.Utc));
            Assert.AreEqual(expected.StartDateTime, result.StartDateTime);
            Assert.AreEqual(expected.EndDateTime, result.EndDateTime);
        }

        [Test]
        public void ScalePeriodShouldStartAtZeroIfNightshiftDayBefore()
        {
            var assPeriod1 = new DateTimePeriod(new DateTime(2011, 1, 1, 22, 0, 0, 0, DateTimeKind.Utc), new DateTime(2011, 1, 2, 6, 0, 0, 0, DateTimeKind.Utc));
            var assPeriod2 = new DateTimePeriod(new DateTime(2011, 1, 2, 8, 0, 0, 0, DateTimeKind.Utc), new DateTime(2011, 1, 2, 16, 0, 0, 0, DateTimeKind.Utc));
           
            using (_mocks.Record())
            {
                Expect.Call(_stateHolder.FilteredCombinedAgentsDictionary).Return(_persons);
                Expect.Call(_stateHolder.Schedules).Return(_scheduleDictionary);
                Expect.Call(_scheduleDictionary[_person]).Return(_range);

                Expect.Call(_range.ScheduledDay(new DateOnly(2011, 01, 01))).Return(_scheduleDay1);
				Expect.Call(_scheduleDay1.ProjectionService()).Return(_projectionService1);
				Expect.Call(_projectionService1.CreateProjection()).Return(_visualLayerCollection1);
				Expect.Call(_visualLayerCollection1.HasLayers).Return(true);
				Expect.Call(_visualLayerCollection1.Period()).Return(assPeriod1);

                Expect.Call(_range.ScheduledDay(new DateOnly(2011, 01, 02))).Return(_scheduleDay2);
				Expect.Call(_scheduleDay2.ProjectionService()).Return(_projectionService2);
				Expect.Call(_projectionService2.CreateProjection()).Return(_visualLayerCollection2);
				Expect.Call(_visualLayerCollection2.HasLayers).Return(true);
				Expect.Call(_visualLayerCollection2.Period()).Return(assPeriod2);
            }

            DateTimePeriod result;

            using (_mocks.Playback())
            {
                result = _target.CalculateScalePeriod(_stateHolder, new DateOnly(2011, 01, 02));
            }

            var expected =
                new DateTimePeriod(new DateTime(2011, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc),
                                   new DateTime(2011, 1, 2, 17, 0, 0, 0, DateTimeKind.Utc));
            Assert.AreEqual(new DateTime(2011, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), result.StartDateTime);
            Assert.AreEqual(expected.EndDateTime, result.EndDateTime);
        }

        [Test]
        public void ScalePeriodShouldReturnDefaultForDayWithNoAssignmentAndNightshiftDayBefore()
        {
            var assPeriod1 = new DateTimePeriod(new DateTime(2011, 1, 1, 22, 0, 0, 0, DateTimeKind.Utc), new DateTime(2011, 1, 2, 6, 0, 0, 0, DateTimeKind.Utc));
          
            using (_mocks.Record())
            {
                Expect.Call(_stateHolder.FilteredCombinedAgentsDictionary).Return(_persons);
                Expect.Call(_stateHolder.Schedules).Return(_scheduleDictionary);
                Expect.Call(_scheduleDictionary[_person]).Return(_range);

                Expect.Call(_range.ScheduledDay(new DateOnly(2011, 01, 01))).Return(_scheduleDay1);
				Expect.Call(_scheduleDay1.ProjectionService()).Return(_projectionService1);
				Expect.Call(_projectionService1.CreateProjection()).Return(_visualLayerCollection1);
				Expect.Call(_visualLayerCollection1.HasLayers).Return(true);
				Expect.Call(_visualLayerCollection1.Period()).Return(assPeriod1);

                Expect.Call(_range.ScheduledDay(new DateOnly(2011, 01, 02))).Return(_scheduleDay2);
				Expect.Call(_scheduleDay2.ProjectionService()).Return(_projectionService2);
				Expect.Call(_projectionService2.CreateProjection()).Return(_visualLayerCollection2);
				Expect.Call(_visualLayerCollection2.HasLayers).Return(false);
            }

            DateTimePeriod result;

            using (_mocks.Playback())
            {
                result = _target.CalculateScalePeriod(_stateHolder, new DateOnly(2011, 01, 02));
            }

            var expected =
                new DateTimePeriod(new DateTime(2011, 1, 2, 7, 0, 0, 0, DateTimeKind.Utc),
                                   new DateTime(2011, 1, 2, 16, 0, 0, 0, DateTimeKind.Utc));
            Assert.AreEqual(new DateTime(2011, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), result.StartDateTime);
            Assert.AreEqual(expected.EndDateTimeLocal(TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone), result.EndDateTime);
        }

        [Test]
        public void ScalePeriodShouldReturnDefaultForDayWithNoAssignmentAndNoNightshiftDayBefore()
        {
            var assPeriod1 = new DateTimePeriod(new DateTime(2011, 1, 1, 8, 0, 0, 0, DateTimeKind.Utc), new DateTime(2011, 1, 1, 16, 0, 0, 0, DateTimeKind.Utc));
           
            using (_mocks.Record())
            {
                Expect.Call(_stateHolder.FilteredCombinedAgentsDictionary).Return(_persons);
                Expect.Call(_stateHolder.Schedules).Return(_scheduleDictionary);
                Expect.Call(_scheduleDictionary[_person]).Return(_range);

                Expect.Call(_range.ScheduledDay(new DateOnly(2011, 01, 01))).Return(_scheduleDay1);
				Expect.Call(_scheduleDay1.ProjectionService()).Return(_projectionService1);
				Expect.Call(_projectionService1.CreateProjection()).Return(_visualLayerCollection1);
				Expect.Call(_visualLayerCollection1.HasLayers).Return(true);
				Expect.Call(_visualLayerCollection1.Period()).Return(assPeriod1);
				
                Expect.Call(_range.ScheduledDay(new DateOnly(2011, 01, 02))).Return(_scheduleDay2);
				Expect.Call(_scheduleDay2.ProjectionService()).Return(_projectionService2);
				Expect.Call(_projectionService2.CreateProjection()).Return(_visualLayerCollection2);
				Expect.Call(_visualLayerCollection2.HasLayers).Return(false);
	           
            }

            DateTimePeriod result;

            using (_mocks.Playback())
            {
                result = _target.CalculateScalePeriod(_stateHolder, new DateOnly(2011, 01, 02));
            }

            var expected =
                new DateTimePeriod(new DateTime(2011, 1, 2, 6, 0, 0, 0, DateTimeKind.Utc),
                                   new DateTime(2011, 1, 2, 16, 0, 0, 0, DateTimeKind.Utc));
            Assert.AreEqual(expected.StartDateTimeLocal(TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone), result.StartDateTime);
            Assert.AreEqual(expected.EndDateTimeLocal(TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone), result.EndDateTime);
        }

        [Test]
        public void ScalePeriodShouldReturnDefaultForDayWithOnlyNightshift()
        {
            var assPeriod1 = new DateTimePeriod(new DateTime(2011, 1, 1, 8, 0, 0, 0, DateTimeKind.Utc), new DateTime(2011, 1, 1, 16, 0, 0, 0, DateTimeKind.Utc));
            var assPeriod2 = new DateTimePeriod(new DateTime(2011, 1, 2, 22, 0, 0, 0, DateTimeKind.Utc), new DateTime(2011, 1, 3, 6, 0, 0, 0, DateTimeKind.Utc));
           
            using (_mocks.Record())
            {
                Expect.Call(_stateHolder.FilteredCombinedAgentsDictionary).Return(_persons);
                Expect.Call(_stateHolder.Schedules).Return(_scheduleDictionary);
                Expect.Call(_scheduleDictionary[_person]).Return(_range);
				
				Expect.Call(_range.ScheduledDay(new DateOnly(2011, 01, 01))).Return(_scheduleDay1);
				Expect.Call(_scheduleDay1.ProjectionService()).Return(_projectionService1);
				Expect.Call(_projectionService1.CreateProjection()).Return(_visualLayerCollection1);
				Expect.Call(_visualLayerCollection1.HasLayers).Return(true);
				Expect.Call(_visualLayerCollection1.Period()).Return(assPeriod1);

				Expect.Call(_range.ScheduledDay(new DateOnly(2011, 01, 02))).Return(_scheduleDay2);
				Expect.Call(_scheduleDay2.ProjectionService()).Return(_projectionService2);
				Expect.Call(_projectionService2.CreateProjection()).Return(_visualLayerCollection2);
				Expect.Call(_visualLayerCollection2.HasLayers).Return(true);
				Expect.Call(_visualLayerCollection2.Period()).Return(assPeriod2);
			
			
			}

            DateTimePeriod result;

            using (_mocks.Playback())
            {
                result = _target.CalculateScalePeriod(_stateHolder, new DateOnly(2011, 01, 02));
            }

            var expected =
                new DateTimePeriod(new DateTime(2011, 1, 2, 8, 0, 0, 0, DateTimeKind.Utc),
                                   new DateTime(2011, 1, 3, 7, 0, 0, 0, DateTimeKind.Utc));
            Assert.AreEqual(expected.StartDateTime, result.StartDateTime);
            Assert.AreEqual(expected.EndDateTime, result.EndDateTime);
        }
    }
}