using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling.TimeLayer
{
	public class MultiplicatorProjectionServiceTest
    {
        private IMultiplicatorProjectionService _target;
        private MockRepository _mocker;
        private IActivity _activity;
        private IList<DateTimePeriod> _layerPeriods;
        private IList<DateTimePeriod> _layerWithMultiplicatorPeriods;
        private IList<DateTimePeriod> _multiplicatorLayersPeriods;
        private IList<IMultiplicatorLayer> _multiplicatorLayers;
        private IMultiplicatorDefinitionSet _definitionSet;
        private IMultiplicator _obTimeMultiplicator;
        private IMultiplicator _overtimeMultiplicator;
        private TimeZoneInfo _timeZone;
        private DateTime _baseDateTime;
        private VisualLayerFactory _factory;
        private IScheduleDay _schedulePart;
        private IProjectionService _projectionService;
        private DateOnly _dateOnly;
        private IPerson _person;

        [SetUp]
        public void Setup()
        {
            _factory = new VisualLayerFactory();
            _timeZone = (TimeZoneInfo.Utc);
            _baseDateTime = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            _dateOnly = new DateOnly(_baseDateTime);
            _obTimeMultiplicator = new Multiplicator(MultiplicatorType.OBTime);
            _overtimeMultiplicator = new Multiplicator(MultiplicatorType.Overtime);
            _activity = ActivityFactory.CreateActivity("for test");
            _activity.InContractTime = true;
            _activity.InPaidTime = true;
            _person = PersonFactory.CreatePersonWithPersonPeriod(_dateOnly, new List<ISkill>());
            _layerPeriods = CreatePeriods(_baseDateTime, 5);
            _layerWithMultiplicatorPeriods = CreatePeriods(_baseDateTime.AddHours(6), 5);
            _mocker = new MockRepository();
            _definitionSet = _mocker.DynamicMock<IMultiplicatorDefinitionSet>();
            _schedulePart = _mocker.StrictMock<IScheduleDay>();
            _projectionService = _mocker.StrictMock<IProjectionService>();
            _person.PermissionInformation.SetDefaultTimeZone(_timeZone);
            _person.Period(_dateOnly).PersonContract.Contract.AddMultiplicatorDefinitionSetCollection(_definitionSet);
        }

        #region OBTime
        [Test]
        public void VerifyProjectionReturnsAllMultiplicatorLayersWithUnderlyingLayersWhenOBTime()
        {
            //Setup for multiplicator-projection:
            //Should Create a projection of following layers:
            // Projected layers      MultiplicatorLayers   Projected w. OBTime multiplic
            // X 04:00-05:00         04:00-05:00
            // X 05:00-06:00         05:00-06:00
            //                       06:00-07:00
            // X                     07:00-08:00             07:00-08:00         
            //                                               08:00-09:00
            //                                               10:00-11:00
            //                                               11:00-12:00
            _multiplicatorLayersPeriods = CreatePeriods(_baseDateTime.AddHours(3), 4);
            _multiplicatorLayers = CreateMultiplicatorLayers(_definitionSet, _multiplicatorLayersPeriods, _obTimeMultiplicator);
            var projection = CreateVisualLayerCollection(_layerPeriods, _layerWithMultiplicatorPeriods, _definitionSet);
            _target = new MultiplicatorProjectionService(_schedulePart, _dateOnly);

            using (_mocker.Record())
            {
                //Expect.Call(_schedulePart.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(_dateOnly, _timeZone)).Repeat.AtLeastOnce();
                Expect.Call(_schedulePart.TimeZone).Return(_timeZone);
                Expect.Call(_schedulePart.Person).Return(_person);
                Expect.Call(_schedulePart.ProjectionService()).Return(_projectionService);
                Expect.Call(_projectionService.CreateProjection()).Return(projection);
                Expect.Call(_definitionSet.CreateProjectionForPeriod(new DateOnlyPeriod(start, end), _timeZone)).Return(_multiplicatorLayers);
                Expect.Call(_definitionSet.MultiplicatorType).Return(MultiplicatorType.OBTime).Repeat.AtLeastOnce();
            }
            using (_mocker.Playback())
            {
                var result = _target.CreateProjection();
                Assert.AreEqual(3,result.Count);
                Assert.IsTrue(result.All(r => r.Payload.MultiplicatorType==MultiplicatorType.OBTime));
            }
        }

        [Test]
        public void VerifyNoOBTimeGeneratedWhenActivityNotInContractTime()
        {
            _activity.InContractTime = false;
            _multiplicatorLayersPeriods = CreatePeriods(_baseDateTime.AddHours(3), 4);
            _multiplicatorLayers = CreateMultiplicatorLayers(_definitionSet, _multiplicatorLayersPeriods, _obTimeMultiplicator);
            var projection = CreateVisualLayerCollection(_layerPeriods, _layerWithMultiplicatorPeriods, _definitionSet);
            _target = new MultiplicatorProjectionService(_schedulePart, _dateOnly);

            using (_mocker.Record())
            {
                Expect.Call(_schedulePart.TimeZone).Return(_timeZone);
                Expect.Call(_schedulePart.Person).Return(_person);
                Expect.Call(_schedulePart.ProjectionService()).Return(_projectionService);
                Expect.Call(_projectionService.CreateProjection()).Return(projection);
                Expect.Call(_definitionSet.CreateProjectionForPeriod(new DateOnlyPeriod(start, end), _timeZone)).Return(_multiplicatorLayers);
                Expect.Call(_definitionSet.MultiplicatorType).Return(MultiplicatorType.OBTime).Repeat.AtLeastOnce();
            }
            using (_mocker.Playback())
            {
                var result = _target.CreateProjection();
                Assert.AreEqual(0, result.Count);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldGenerateShiftAllowanceWhenWorkShiftStartsInAMultiplicatorPeriodStartingYesterday()
        {
            IMultiplicatorDefinitionSet definitionSet = new MultiplicatorDefinitionSet("test", MultiplicatorType.OBTime);
            IMultiplicator multiplicator = new Multiplicator(MultiplicatorType.OBTime);
            TimePeriod timePeriod = new TimePeriod(TimeSpan.FromHours(18), TimeSpan.FromHours(30));
            definitionSet.AddDefinition(new DayOfWeekMultiplicatorDefinition(multiplicator, DayOfWeek.Monday, timePeriod));
            definitionSet.AddDefinition(new DayOfWeekMultiplicatorDefinition(multiplicator, DayOfWeek.Tuesday, timePeriod));
            definitionSet.AddDefinition(new DayOfWeekMultiplicatorDefinition(multiplicator, DayOfWeek.Wednesday, timePeriod));
            definitionSet.AddDefinition(new DayOfWeekMultiplicatorDefinition(multiplicator, DayOfWeek.Thursday, timePeriod));
            definitionSet.AddDefinition(new DayOfWeekMultiplicatorDefinition(multiplicator, DayOfWeek.Friday, timePeriod));
            definitionSet.AddDefinition(new DayOfWeekMultiplicatorDefinition(multiplicator, DayOfWeek.Saturday, timePeriod));
            definitionSet.AddDefinition(new DayOfWeekMultiplicatorDefinition(multiplicator, DayOfWeek.Sunday, timePeriod));

            _person.Period(_dateOnly).PersonContract.Contract.RemoveMultiplicatorDefinitionSetCollection(_definitionSet);
            _person.Period(_dateOnly).PersonContract.Contract.AddMultiplicatorDefinitionSetCollection(definitionSet);

            DateTimePeriod layerPeriod = new DateTimePeriod(_baseDateTime.AddHours(4), _baseDateTime.AddHours(10));
            var projection = CreateVisualLayerCollection(new List<DateTimePeriod> { layerPeriod }, _layerWithMultiplicatorPeriods, definitionSet);
            _target = new MultiplicatorProjectionService(_schedulePart, _dateOnly);

            using (_mocker.Record())
            {
                Expect.Call(_schedulePart.TimeZone).Return(_timeZone);
                Expect.Call(_schedulePart.Person).Return(_person);
                Expect.Call(_schedulePart.ProjectionService()).Return(_projectionService);
                Expect.Call(_projectionService.CreateProjection()).Return(projection);
            }
            using (_mocker.Playback())
            {
                var result = _target.CreateProjection();
                Assert.AreEqual(1, result.Count);
                Assert.AreEqual(TimeSpan.FromHours(2), result[0].Period.ElapsedTime());
            }
        }

        [Test]
        public void ShouldNotGenerateShiftAllowanceWhenDefinitionSetIsDeleted()
        {
            _multiplicatorLayersPeriods = CreatePeriods(_baseDateTime.AddHours(3), 4);
            _multiplicatorLayers = CreateMultiplicatorLayers(_definitionSet, _multiplicatorLayersPeriods, _obTimeMultiplicator);
            var projection = CreateVisualLayerCollection(_layerPeriods, _layerWithMultiplicatorPeriods, _definitionSet);
            _target = new MultiplicatorProjectionService(_schedulePart, _dateOnly);

            using (_mocker.Record())
            {
                Expect.Call(_schedulePart.Person).Return(_person);
                Expect.Call(_schedulePart.ProjectionService()).Return(_projectionService);
                Expect.Call(_projectionService.CreateProjection()).Return(projection);

                Expect.Call(_definitionSet.IsDeleted).Return(true);
            }
            using (_mocker.Playback())
            {
                var result = _target.CreateProjection();
                Assert.AreEqual(0, result.Count);
            }
        }

        [Test]
        public void ShouldGenerateShiftAllowanceWhenDefinitionSetIsNotDeleted()
        {
            _multiplicatorLayersPeriods = CreatePeriods(_baseDateTime.AddHours(3), 4);
            _multiplicatorLayers = CreateMultiplicatorLayers(_definitionSet, _multiplicatorLayersPeriods, _obTimeMultiplicator);
            var projection = CreateVisualLayerCollection(_layerPeriods, _layerWithMultiplicatorPeriods, _definitionSet);
            _target = new MultiplicatorProjectionService(_schedulePart, _dateOnly);

            using (_mocker.Record())
            {
                Expect.Call(_schedulePart.Person).Return(_person);
                Expect.Call(_schedulePart.ProjectionService()).Return(_projectionService);
                Expect.Call(_projectionService.CreateProjection()).Return(projection);

                Expect.Call(_definitionSet.IsDeleted).Return(false);

                // see the previous test, this is the extra
                Expect.Call(_schedulePart.TimeZone).Return(_timeZone);
                Expect.Call(_definitionSet.CreateProjectionForPeriod(new DateOnlyPeriod(start, end), _timeZone)).Return(_multiplicatorLayers);
                Expect.Call(_definitionSet.MultiplicatorType).Return(MultiplicatorType.OBTime).Repeat.AtLeastOnce();

            }
            using (_mocker.Playback())
            {
                var result = _target.CreateProjection();
                Assert.AreEqual(3, result.Count);
            }
        }

        [Test]
        public void VerifyOBTimeGeneratedWhenPayloadIsMeetingPayload()
        {
            _multiplicatorLayersPeriods = CreatePeriods(_baseDateTime.AddHours(3), 4);
            _multiplicatorLayers = CreateMultiplicatorLayers(_definitionSet, _multiplicatorLayersPeriods, _obTimeMultiplicator);
            var projection = CreateVisualLayerCollection(_layerPeriods, _layerWithMultiplicatorPeriods, _definitionSet);
            _target = new MultiplicatorProjectionService(_schedulePart, _dateOnly);

            using (_mocker.Record())
            {
                Expect.Call(_schedulePart.TimeZone).Return(_timeZone);
                Expect.Call(_schedulePart.Person).Return(_person);
                Expect.Call(_schedulePart.ProjectionService()).Return(_projectionService);
                Expect.Call(_projectionService.CreateProjection()).Return(projection);
                Expect.Call(_definitionSet.CreateProjectionForPeriod(new DateOnlyPeriod(start, end), _timeZone)).Return(_multiplicatorLayers);
                Expect.Call(_definitionSet.MultiplicatorType).Return(MultiplicatorType.OBTime).Repeat.AtLeastOnce();
            }
            using (_mocker.Playback())
            {
                var result = _target.CreateProjection();
                Assert.AreEqual(3, result.Count);
            }
        }

        [Test]
        public void VerifyNoPersonPeriodOnPersonWorks()
        {
            _target = new MultiplicatorProjectionService(_schedulePart, _dateOnly);
            _person.RemoveAllPersonPeriods();

            using (_mocker.Record())
            {
                Expect.Call(_schedulePart.ProjectionService()).Return(_projectionService);
                Expect.Call(_projectionService.CreateProjection()).Return(new VisualLayerCollection(new List<IVisualLayer>(), new ProjectionPayloadMerger()));
                Expect.Call(_schedulePart.Person).Return(_person);
            }
            using (_mocker.Playback())
            {
                var result = _target.CreateProjection();
                Assert.AreEqual(0, result.Count);
            }
        }

        [Test]
        public void VerifyNoContractOnPersonPeriodWorks()
        {
            _target = new MultiplicatorProjectionService(_schedulePart, _dateOnly);
            _person.Period(_dateOnly).PersonContract = null;

            using (_mocker.Record())
            {
                Expect.Call(_schedulePart.ProjectionService()).Return(_projectionService);
                Expect.Call(_projectionService.CreateProjection()).Return(new VisualLayerCollection(new List<IVisualLayer>(), new ProjectionPayloadMerger()));
                Expect.Call(_schedulePart.Person).Return(_person);
            }
            using (_mocker.Playback())
            {
                var result = _target.CreateProjection();
                Assert.AreEqual(0, result.Count);
            }
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyProjectionWithUnevenPeriodsOBTime()
        {
            //Setup for multiplicator-projection:
            //Should Create a projection of following layers:
            // Projected layers      MultiplicatorLayers   Projected w. OBTime multiplic
            // X 04:00-05:00         04:00-05:00
            // X 05:00-06:00         05:00-06:00
            //                       06:00-07:00
            // X                     07:00-08:00             07:00-08:00  
            // X                     08:00-08:55             08:00
            // X                     08:55-08:58
            // X                     08:58                        -09:00
            //                                               10:00-11:00   
            //                             -11:47            11:00-12:00  
            //                       15:00-16:00
            DateTime four = _baseDateTime.AddHours(4);
            DateTime eight = _baseDateTime.AddHours(8);
            DateTime eleven = _baseDateTime.AddHours(11);
            DateTime elevenFortySeven = eleven.AddMinutes(47);
            DateTimePeriod notProjectedPeriod = new DateTimePeriod(_baseDateTime.AddHours(6), _baseDateTime.AddHours(7));
            _multiplicatorLayersPeriods = CreatePeriods(_baseDateTime.AddHours(3), 4);
            _multiplicatorLayersPeriods.Add(new DateTimePeriod(eight, eight.AddMinutes(55)));
            _multiplicatorLayersPeriods.Add(new DateTimePeriod(eight.AddMinutes(55), eight.AddMinutes(58)));
            _multiplicatorLayersPeriods.Add(new DateTimePeriod(eight.AddMinutes(58), elevenFortySeven));
            _multiplicatorLayersPeriods.Add(new DateTimePeriod(_baseDateTime.AddHours(15), _baseDateTime.AddHours(16)));

            _multiplicatorLayers = CreateMultiplicatorLayers(_definitionSet, _multiplicatorLayersPeriods, _obTimeMultiplicator);
            var projection = CreateVisualLayerCollection(_layerPeriods, _layerWithMultiplicatorPeriods, _definitionSet);
            _target = new MultiplicatorProjectionService(_schedulePart, _dateOnly);

            using (_mocker.Record())
            {
                //Expect.Call(_schedulePart.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(_dateOnly, _timeZone)).Repeat.AtLeastOnce();
                Expect.Call(_schedulePart.Person).Return(_person);
                Expect.Call(_schedulePart.TimeZone).Return(_timeZone);
                Expect.Call(_schedulePart.ProjectionService()).Return(_projectionService);
                Expect.Call(_projectionService.CreateProjection()).Return(projection);
                Expect.Call(_definitionSet.CreateProjectionForPeriod(new DateOnlyPeriod(start, end), _timeZone)).Return(_multiplicatorLayers);
                Expect.Call(_definitionSet.MultiplicatorType).Return(MultiplicatorType.OBTime).Repeat.AtLeastOnce();
            }
            using (_mocker.Playback())
            {
                IList<IMultiplicatorLayer> result = _target.CreateProjection();
                Assert.AreEqual(6, result.Count, "One Layer is created for each multiplicatorlayer where there is a underlying visuallayer");
                Assert.AreEqual(four, result.Min(l => l.Period.StartDateTime), "Start of the projection");
                Assert.AreEqual(elevenFortySeven, result.Max(l => l.Period.EndDateTime), "End of the projection");
                Assert.IsTrue(result.All(l => !l.Period.Intersect(notProjectedPeriod)), "No projection is created between 06:00-07:00");
            }

        }
        #endregion OBTime

        #region Overtime
        [Test]
        public void VerifyProjectionReturnsMultiplicatorLayersWithUnderlyingLayersWhenOvertime()
        {
            //Setup for multiplicator-projection:
            //Should Create a projection of following layers:
            // Projected layers      MultiplicatorLayers   Projected w. Overtime multiplic
            //   04:00-05:00         04:00-05:00
            //   05:00-06:00         05:00-06:00
            //                       06:00-07:00
            // X                     07:00-08:00             07:00-08:00         
            //                                               08:00-09:00
            //                                               10:00-11:00
            //                                               11:00-12:00
            _multiplicatorLayersPeriods = CreatePeriods(_baseDateTime.AddHours(3), 4);
            _multiplicatorLayers = CreateMultiplicatorLayers(_definitionSet, _multiplicatorLayersPeriods, _overtimeMultiplicator);
            var overtimeShift = CreateVisualLayerCollectionForOvertime(_layerWithMultiplicatorPeriods, _definitionSet);
            _target = new MultiplicatorProjectionService(_schedulePart, _dateOnly);
            
            using (_mocker.Record())
            {
                Expect.Call(_schedulePart.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(_dateOnly, _timeZone));
                Expect.Call(_schedulePart.ProjectionService()).Return(_projectionService);
                Expect.Call(_projectionService.CreateProjection()).Return(new VisualLayerCollection(overtimeShift.ToList(), new ProjectionPayloadMerger()));
                Expect.Call(_schedulePart.Person).Return(_person);
                Expect.Call(_schedulePart.TimeZone).Return(_timeZone);
                Expect.Call(_definitionSet.CreateProjectionForPeriod(new DateOnlyPeriod(start, end), _timeZone)).Return(_multiplicatorLayers);
                Expect.Call(_definitionSet.Equals(_definitionSet)).Return(true).Repeat.AtLeastOnce();
                Expect.Call(_definitionSet.MultiplicatorType).Return(MultiplicatorType.Overtime).Repeat.AtLeastOnce();
            }
            using (_mocker.Playback())
            {
                var result = _target.CreateProjection();
                Assert.AreEqual(1, result.Count);
                Assert.IsTrue(result.All(r => r.Payload.MultiplicatorType==MultiplicatorType.Overtime));
            }
        }

        [Test]
        public void ShouldNotReturnOvertimeForUnpaidActivity()
        {
            _multiplicatorLayersPeriods = CreatePeriods(_baseDateTime.AddHours(3), 4);
            _multiplicatorLayers = CreateMultiplicatorLayers(_definitionSet, _multiplicatorLayersPeriods, _overtimeMultiplicator);
            var overtimeShift = CreateVisualLayerCollectionForOvertime(_layerWithMultiplicatorPeriods, _definitionSet);
            _target = new MultiplicatorProjectionService(_schedulePart, _dateOnly);
            _activity.InPaidTime = false;

            using (_mocker.Record())
            {
                Expect.Call(_schedulePart.ProjectionService()).Return(_projectionService);
                Expect.Call(_projectionService.CreateProjection()).Return(new VisualLayerCollection(overtimeShift.ToList(), new ProjectionPayloadMerger()));
                Expect.Call(_schedulePart.Person).Return(_person);
                Expect.Call(_schedulePart.TimeZone).Return(_timeZone);
                Expect.Call(_definitionSet.CreateProjectionForPeriod(new DateOnlyPeriod(start, end), _timeZone)).Return(_multiplicatorLayers);
                Expect.Call(_definitionSet.Equals(_definitionSet)).Return(true).Repeat.AtLeastOnce();
                Expect.Call(_definitionSet.MultiplicatorType).Return(MultiplicatorType.Overtime).Repeat.AtLeastOnce();
            }
            using (_mocker.Playback())
            {
                var result = _target.CreateProjection();
                Assert.AreEqual(0, result.Count);
            }
        }

        [Test]
        public void ShouldSplitMultiplicatorLayerIntoTwoOverMidnight()
        {
            _multiplicatorLayersPeriods = CreatePeriods(_baseDateTime.AddHours(22.5), 1);
            _multiplicatorLayers = CreateMultiplicatorLayers(_definitionSet, _multiplicatorLayersPeriods, _overtimeMultiplicator);

            var overtimeShift = CreateVisualLayerCollectionForOvertime(CreatePeriods(_baseDateTime.AddHours(22.5), 1),
                                                                       _definitionSet);
            _target = new MultiplicatorProjectionService(_schedulePart, _dateOnly);

            using (_mocker.Record())
            {
                Expect.Call(_schedulePart.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(_dateOnly, _timeZone));
                Expect.Call(_schedulePart.ProjectionService()).Return(_projectionService);
                Expect.Call(_projectionService.CreateProjection()).Return(new VisualLayerCollection(overtimeShift.ToList(), new ProjectionPayloadMerger()));
                Expect.Call(_schedulePart.Person).Return(_person);
                Expect.Call(_schedulePart.TimeZone).Return(_timeZone);
                Expect.Call(_definitionSet.CreateProjectionForPeriod(new DateOnlyPeriod(start, end), _timeZone)).Return(_multiplicatorLayers);
                Expect.Call(_definitionSet.Equals(_definitionSet)).Return(true).Repeat.AtLeastOnce();
                Expect.Call(_definitionSet.MultiplicatorType).Return(MultiplicatorType.Overtime).Repeat.AtLeastOnce();
            }
            using (_mocker.Playback())
            {
                var result = _target.CreateProjection();
                Assert.AreEqual(2, result.Count);
                Assert.IsTrue(result.All(r => r.Payload.MultiplicatorType == MultiplicatorType.Overtime));
                Assert.IsTrue(result.All(r => r.Period.ElapsedTime() == TimeSpan.FromMinutes(30)));
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyNoMultiplicatorLayersWithUnderlyingAbsenceLayersWhenOvertime()
        {
            _multiplicatorLayersPeriods = CreatePeriods(_baseDateTime.AddHours(3), 4);
            _multiplicatorLayers = CreateMultiplicatorLayers(_definitionSet, _multiplicatorLayersPeriods, _overtimeMultiplicator);
            _person.PermissionInformation.SetDefaultTimeZone(_timeZone);
            var scenario = ScenarioFactory.CreateScenarioAggregate();
            var partFactory = new SchedulePartFactoryForDomain(_person, scenario,
                                             new DateOnlyPeriod(_dateOnly, _dateOnly)
                                                 .ToDateTimePeriod(_timeZone),SkillFactory.CreateSkill("test"));
            var basePeriod = new DateTimePeriod(
                _baseDateTime.AddHours(3),
                _baseDateTime.AddHours(7));
            var part = partFactory.CreatePart();
            part.CreateAndAddOvertime(_activity, basePeriod, _definitionSet);
            part.CreateAndAddAbsence(new AbsenceLayer(AbsenceFactory.CreateAbsence("Holiday!"),basePeriod));

            _target = new MultiplicatorProjectionService(part, _dateOnly);
            
            using (_mocker.Record())
            {
                Expect.Call(_definitionSet.CreateProjectionForPeriod(new DateOnlyPeriod(start, end), _timeZone)).Return(_multiplicatorLayers).IgnoreArguments();
                Expect.Call(_definitionSet.MultiplicatorType).Return(MultiplicatorType.Overtime).Repeat.AtLeastOnce();
            }
            using (_mocker.Playback())
            {
                var result = _target.CreateProjection();
                Assert.AreEqual(0, result.Count);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldNotReturnOvertimeForTimeOutsideAbsenceAndMultiplicatorLayer()
        {
            _multiplicatorLayersPeriods = CreatePeriods(_baseDateTime.AddHours(3), 10);
            _multiplicatorLayers = CreateMultiplicatorLayers(_definitionSet, _multiplicatorLayersPeriods, _overtimeMultiplicator);
            _person.PermissionInformation.SetDefaultTimeZone(_timeZone);
            var scenario = ScenarioFactory.CreateScenarioAggregate();
            var partFactory = new SchedulePartFactoryForDomain(_person, scenario,
                                             new DateOnlyPeriod(_dateOnly, _dateOnly)
                                                 .ToDateTimePeriod(_timeZone), SkillFactory.CreateSkill("test"));
            var basePeriod = new DateTimePeriod(
                _baseDateTime.AddHours(3),
                _baseDateTime.AddHours(7));
            var part = partFactory.CreatePart();
            part.CreateAndAddActivity(_activity, basePeriod.ChangeEndTime(TimeSpan.FromHours(9)),ShiftCategoryFactory.CreateShiftCategory("test"));
            part.CreateAndAddOvertime(_activity, basePeriod,_definitionSet);
            part.CreateAndAddAbsence(new AbsenceLayer(AbsenceFactory.CreateAbsence("Holiday!"), basePeriod.MovePeriod(TimeSpan.FromHours(5))));

            _target = new MultiplicatorProjectionService(part, _dateOnly);

            using (_mocker.Record())
            {
                Expect.Call(_definitionSet.CreateProjectionForPeriod(new DateOnlyPeriod(start, end), _timeZone)).Return(_multiplicatorLayers).IgnoreArguments();
                Expect.Call(_definitionSet.MultiplicatorType).Return(MultiplicatorType.Overtime).Repeat.AtLeastOnce();
                Expect.Call(_definitionSet.Equals(_definitionSet)).Return(true).Repeat.AtLeastOnce();
            }
            using (_mocker.Playback())
            {
                var result = _target.CreateProjection();
                Assert.AreEqual(3, result.Count);
            }
        }

        [Test]
        public void VerifyProjectionWithUnevenPeriodsOnOvertime()
        {
            //Setup for multiplicator-projection:
            //Should Create a projection of following layers:
            // Projected layers      MultiplicatorLayers   Projected w. Overtime multiplic
            //   04:00-05:00         04:00-05:00
            //   05:00-06:00         05:00-06:00
            //                       06:00-07:00
            // X                     07:00-08:00             07:00-08:00  
            // X                     08:00-08:55             08:00
            // X                     08:55-08:58
            // X                     08:58                        -09:00
            //                                               10:00-11:00   
            //                             -11:47            11:00-12:00  
            //                       15:00-16:00
            
            DateTime seven = _baseDateTime.AddHours(7);
            DateTime eight = _baseDateTime.AddHours(8);
            DateTime eleven = _baseDateTime.AddHours(11);
            DateTime elevenFortySeven = eleven.AddMinutes(47);
            _multiplicatorLayersPeriods = CreatePeriods(_baseDateTime.AddHours(3), 4);
            _multiplicatorLayersPeriods.Add(new DateTimePeriod(eight, eight.AddMinutes(55)));
            _multiplicatorLayersPeriods.Add(new DateTimePeriod(eight.AddMinutes(55), eight.AddMinutes(58)));
            _multiplicatorLayersPeriods.Add(new DateTimePeriod(eight.AddMinutes(58), elevenFortySeven));
            _multiplicatorLayersPeriods.Add(new DateTimePeriod(_baseDateTime.AddHours(15), _baseDateTime.AddHours(16)));

            _multiplicatorLayers = CreateMultiplicatorLayers(_definitionSet, _multiplicatorLayersPeriods, _overtimeMultiplicator);
            var overtimeShift = CreateVisualLayerCollectionForOvertime(_layerWithMultiplicatorPeriods, _definitionSet);
            _target = new MultiplicatorProjectionService(_schedulePart, _dateOnly);
            
            using (_mocker.Record())
            {
                Expect.Call(_schedulePart.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(_dateOnly, _timeZone));
                Expect.Call(_schedulePart.ProjectionService()).Return(_projectionService);
                Expect.Call(_projectionService.CreateProjection()).Return(new VisualLayerCollection(overtimeShift.ToList(), new ProjectionPayloadMerger()));
                
                Expect.Call(_schedulePart.Person).Return(_person);
                Expect.Call(_schedulePart.TimeZone).Return(_timeZone);
                Expect.Call(_definitionSet.CreateProjectionForPeriod(new DateOnlyPeriod(start, end), _timeZone)).Return(_multiplicatorLayers);
                Expect.Call(_definitionSet.Equals(_definitionSet)).Return(true).Repeat.AtLeastOnce();
                Expect.Call(_definitionSet.MultiplicatorType).Return(MultiplicatorType.Overtime).Repeat.AtLeastOnce();
            }
            using (_mocker.Playback())
            {
                IList<IMultiplicatorLayer> projection = _target.CreateProjection();
                Assert.AreEqual(4, projection.Count, "One Layer is created for each multiplicatorlayer where there is a underlying visuallayer with overtime definitionset");
                Assert.AreEqual(seven, projection.Min(l => l.Period.StartDateTime), "Start of the projection");
                Assert.AreEqual(elevenFortySeven, projection.Max(l => l.Period.EndDateTime), "End of the projection");
            }
        }
        #endregion Overtime

        #region multiple DefinitionSets
        [Test]
        public void VerifyCombinesOBTimeDefinitionSets()
        {
            //Setup for multiplicator-projection:
            //Should Create a projection of following layers:
            // Projected layers      MultiplicatorLayers   Projected w. OBTime multiplic
            // X 04:00-05:00         04:00-05:00 D1
            // X 05:00-06:00         05:00-06:00 D1
            //                       06:00-07:00 D2
            // X                     07:00-08:00 D2          07:00-08:00 D1         
            //                                               08:00-09:00 D1
            //                                               09:00-10:00 D2
            //                                               10:00-11:00 D2

            IMultiplicatorDefinitionSet definitionSet2 = _mocker.StrictMock<IMultiplicatorDefinitionSet>();
            
            _multiplicatorLayers = CreateMultiplicatorLayers(_definitionSet, CreatePeriods(_baseDateTime.AddHours(3), 2), _obTimeMultiplicator);
            IList<IMultiplicatorLayer> multiplicatorLayers2 = CreateMultiplicatorLayers(definitionSet2, CreatePeriods(_baseDateTime.AddHours(5), 2), _obTimeMultiplicator);

            //Create projection
            var projection = CreateVisualLayerCollection(_layerPeriods, CreatePeriods(_baseDateTime.AddHours(6), 2), _definitionSet);
            
            foreach (DateTimePeriod period in CreatePeriods(_baseDateTime.AddHours(8), 2))
            {
                projection = Add(projection, period, definitionSet2);
            }

            _person.Period(_dateOnly).PersonContract.Contract.AddMultiplicatorDefinitionSetCollection(definitionSet2);
            _target = new MultiplicatorProjectionService(_schedulePart, _dateOnly);

            using (_mocker.Record())
            {
                Expect.Call(_schedulePart.Person).Return(_person);
                Expect.Call(_schedulePart.TimeZone).Return(_timeZone).Repeat.AtLeastOnce();
                Expect.Call(_schedulePart.ProjectionService()).Return(_projectionService);
                Expect.Call(_projectionService.CreateProjection()).Return(projection);
                Expect.Call(_definitionSet.CreateProjectionForPeriod(new DateOnlyPeriod(start, end), _timeZone)).Return(_multiplicatorLayers);
                Expect.Call(definitionSet2.CreateProjectionForPeriod(new DateOnlyPeriod(start, end), _timeZone)).Return(multiplicatorLayers2);
                Expect.Call(_definitionSet.MultiplicatorType).Return(MultiplicatorType.OBTime).Repeat.AtLeastOnce();
                Expect.Call(definitionSet2.MultiplicatorType).Return(MultiplicatorType.OBTime).Repeat.AtLeastOnce();

                Expect.Call(_definitionSet.IsDeleted).Return(false).Repeat.AtLeastOnce();
                Expect.Call(definitionSet2.IsDeleted).Return(false).Repeat.AtLeastOnce();
            }
            using (_mocker.Playback())
            {
                IList<IMultiplicatorLayer> result = _target.CreateProjection();
                Assert.AreEqual(3, result.Count);
            }
        }

        [Test]
        public void VerifyCombinesOvertimeDefinitionSets()
        {
            //Setup for multiplicator-projection:
            //Should Create a projection of following layers:
            //MultiplicatorLayers
            //07:00-09:00 D2 / M2
            //08:00-11:00 D1 / M1

            var period1 = new DateTimePeriod(_baseDateTime.AddHours(7), _baseDateTime.AddHours(9));
            var period2 = new DateTimePeriod(_baseDateTime.AddHours(8), _baseDateTime.AddHours(11));
            _overtimeMultiplicator.Description = new Description("M2");
            IMultiplicatorDefinitionSet definitionSet2 = _mocker.StrictMock<IMultiplicatorDefinitionSet>();
            var newOvertimeMultiplicator = new Multiplicator(MultiplicatorType.Overtime)
                                               {Description = new Description("M1")};
            _multiplicatorLayers = CreateMultiplicatorLayers(_definitionSet, new List<DateTimePeriod> {period1}, _overtimeMultiplicator);
            IList<IMultiplicatorLayer> multiplicatorLayers2 =
                CreateMultiplicatorLayers(definitionSet2, new List<DateTimePeriod> {period2}, newOvertimeMultiplicator);

            //Create projection
            var overtimeShift1 = CreateVisualLayerCollectionForOvertime(new List<DateTimePeriod>{period1}, _definitionSet);
            var overtimeShift2 = CreateVisualLayerCollectionForOvertime(new List<DateTimePeriod> { period2 }, _definitionSet);
            _target = new MultiplicatorProjectionService(_schedulePart, _dateOnly);
            _person.Period(_dateOnly).PersonContract.Contract.AddMultiplicatorDefinitionSetCollection(definitionSet2);

            using (_mocker.Record())
            {
                Expect.Call(_schedulePart.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(_dateOnly, _timeZone)).Repeat.AtLeastOnce();
                Expect.Call(_schedulePart.ProjectionService()).Return(_projectionService);
                Expect.Call(_projectionService.CreateProjection()).Return(new VisualLayerCollection(overtimeShift1.Concat(overtimeShift2).ToList(), new ProjectionPayloadMerger()));
                Expect.Call(_schedulePart.Person).Return(_person);
                Expect.Call(_schedulePart.TimeZone).Return(_timeZone).Repeat.AtLeastOnce();
                Expect.Call(_definitionSet.CreateProjectionForPeriod(new DateOnlyPeriod(start, end), _timeZone)).Return(_multiplicatorLayers);
                Expect.Call(definitionSet2.CreateProjectionForPeriod(new DateOnlyPeriod(start, end), _timeZone)).Return(multiplicatorLayers2);
                Expect.Call(definitionSet2.Equals(_definitionSet)).Return(false).Repeat.AtLeastOnce();
                Expect.Call(_definitionSet.Equals(_definitionSet)).Return(true).Repeat.AtLeastOnce();
                Expect.Call(_definitionSet.MultiplicatorType).Return(MultiplicatorType.Overtime).Repeat.AtLeastOnce();
                Expect.Call(definitionSet2.MultiplicatorType).Return(MultiplicatorType.Overtime).Repeat.AtLeastOnce();

                Expect.Call(_definitionSet.IsDeleted).Return(false).Repeat.AtLeastOnce();
                Expect.Call(definitionSet2.IsDeleted).Return(false).Repeat.AtLeastOnce();
            }
            using (_mocker.Playback())
            {
                IList<IMultiplicatorLayer> projection = _target.CreateProjection();
                Assert.AreEqual(2, projection.Count);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "NUnit.Framework.Assert.AreEqual(System.Object,System.Object,System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "NUnit.Framework.Assert.AreEqual(System.Int32,System.Int32,System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldNotReturnOBWhereWorkingOvertime()
        {
            //Setup for multiplicator-projection:
            //Should Create a projection of following layers:
            // Projected layers      MultiplicatorLayersOBtime   MultilpicatorLayersOvertime        Projected w. OBTime multiplic
            // OB 04:00-05:00         04:00-05:00 D1
            // OB 05:00-06:00         05:00-06:00 D1
            //                        06:00-07:00 D1
            // OB                     07:00-08:00 D1                                                 07:00-08:00 D1         
            // OB                     08:00-09:00 D1                                                 08:00-09:00 D1
            // OB,OT                  09:00-10:00 D1              09:00-10:00 D2                     09:00-10:00 D2
            // OT                                                 10:00-11:00 D2                     10:00-11:00 D2

            IMultiplicatorDefinitionSet definitionSet2 = _mocker.StrictMock<IMultiplicatorDefinitionSet>();
            _multiplicatorLayers = CreateMultiplicatorLayers(_definitionSet, CreatePeriods(_baseDateTime.AddHours(3), 6), _obTimeMultiplicator);
            IList<IMultiplicatorLayer> multiplicatorLayers2 = CreateMultiplicatorLayers(definitionSet2, CreatePeriods(_baseDateTime.AddHours(8), 2), _overtimeMultiplicator);

            //Create projection
            var projection = CreateVisualLayerCollection(_layerPeriods, CreatePeriods(_baseDateTime.AddHours(6), 2), _definitionSet);

            foreach (DateTimePeriod period in CreatePeriods(_baseDateTime.AddHours(8), 2))
            {
                projection = Add(projection, period, definitionSet2);
            }

            _person.Period(_dateOnly).PersonContract.Contract.AddMultiplicatorDefinitionSetCollection(definitionSet2);
            _target = new MultiplicatorProjectionService(_schedulePart, _dateOnly);

            using (_mocker.Record())
            {
                Expect.Call(_schedulePart.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(_dateOnly, _timeZone)).Repeat.AtLeastOnce();
                Expect.Call(_schedulePart.ProjectionService()).Return(_projectionService);
                Expect.Call(_projectionService.CreateProjection()).Return(projection);
                Expect.Call(_schedulePart.Person).Return(_person);
                Expect.Call(_schedulePart.TimeZone).Return(_timeZone).Repeat.AtLeastOnce();
                Expect.Call(_definitionSet.CreateProjectionForPeriod(new DateOnlyPeriod(start, end), _timeZone)).Return(_multiplicatorLayers);
                Expect.Call(definitionSet2.CreateProjectionForPeriod(new DateOnlyPeriod(start, end), _timeZone)).Return(multiplicatorLayers2);
                Expect.Call(definitionSet2.Equals(definitionSet2)).Return(true).Repeat.AtLeastOnce();
                Expect.Call(_definitionSet.Equals(definitionSet2)).Return(false).Repeat.AtLeastOnce();
                Expect.Call(definitionSet2.Equals(_definitionSet)).Return(false).Repeat.AtLeastOnce();
                Expect.Call(definitionSet2.Equals(null)).Return(false).Repeat.AtLeastOnce();
                Expect.Call(_definitionSet.MultiplicatorType).Return(MultiplicatorType.OBTime).Repeat.AtLeastOnce();
                Expect.Call(definitionSet2.MultiplicatorType).Return(MultiplicatorType.Overtime).Repeat.AtLeastOnce();

                Expect.Call(_definitionSet.IsDeleted).Return(false).Repeat.AtLeastOnce();
                Expect.Call(definitionSet2.IsDeleted).Return(false).Repeat.AtLeastOnce();
            }
            using (_mocker.Playback())
            {
                IList<IMultiplicatorLayer> result = _target.CreateProjection();
                DateTime overtimeStart =
                    result.Where(l => l.Payload.MultiplicatorType == MultiplicatorType.Overtime).Min(
                        l => l.Period.StartDateTime);
                DateTime overtimeEnd =
                   result.Where(l => l.Payload.MultiplicatorType == MultiplicatorType.Overtime).Max(
                       l => l.Period.EndDateTime);
                Assert.AreEqual(_baseDateTime.AddHours(9), overtimeStart, "Overtime from 09:00-11:00");
                Assert.AreEqual(_baseDateTime.AddHours(11), overtimeEnd, "Overtime from 09:00-11:00");

                DateTime OBmin =
                   result.Where(l => l.Payload.MultiplicatorType == MultiplicatorType.OBTime).Min(
                       l => l.Period.StartDateTime);
                DateTime OBmax =
                   result.Where(l => l.Payload.MultiplicatorType == MultiplicatorType.OBTime).Max(
                       l => l.Period.EndDateTime);
                Assert.AreEqual(_baseDateTime.AddHours(4), OBmin, "OB minimum from 04:00");
                Assert.AreEqual(_baseDateTime.AddHours(9), OBmax, "OB max from 10:00, but working overtime 9-11");

                //No ob between 06:00-07:00
                DateTimePeriod sixToSeven = new DateTimePeriod(_baseDateTime.AddHours(6), _baseDateTime.AddHours(7));
                int numberOfLayersBetweenSixAndSeven = result.Where(l => l.Payload.MultiplicatorType == MultiplicatorType.OBTime).Where(
                    l => sixToSeven.Contains(l.Period)).
                    Count();
                Assert.AreEqual(0, numberOfLayersBetweenSixAndSeven, "No ob between 06:00-07:00 because mo underlying layers");


            }
        }

        #endregion

        #region helpers for setup
        //Create visualLayerCollection:
        private IVisualLayerCollection CreateVisualLayerCollection(IEnumerable<DateTimePeriod> layers, IEnumerable<DateTimePeriod> layersWithMultiplicator, IMultiplicatorDefinitionSet definitionSet)
        {
            IList<IVisualLayer> retList = new List<IVisualLayer>();
            VisualLayerFactory factory = new VisualLayerFactory();

            //Create the normallayers
            foreach (var period in layers)
            {
                retList.Add(factory.CreateShiftSetupLayer(_activity, period));
            }

            foreach (var period in layersWithMultiplicator)
            {
                var layer = factory.CreateShiftSetupLayer(_activity, period);
                SetDefinitionSetOnVisualLayer(layer, definitionSet);
                retList.Add(layer);
            }

            return new VisualLayerCollection(retList, new ProjectionPayloadMerger());
        }

        private IEnumerable<IVisualLayer> CreateVisualLayerCollectionForOvertime(IEnumerable<DateTimePeriod> layersWithMultiplicator, IMultiplicatorDefinitionSet definitionSet)
        {
            var factory = new VisualLayerFactory();
            foreach (var period in layersWithMultiplicator)
            {
                var layer = factory.CreateShiftSetupLayer(_activity, period);
                SetDefinitionSetOnVisualLayer(layer, definitionSet);
                yield return layer;
            }
        }

        private static IList<DateTimePeriod> CreatePeriods(DateTime baseDateTime, int numberOfPeriods)
        {
            IList<DateTimePeriod> retList = new List<DateTimePeriod>();
            for (int i = 1; i < numberOfPeriods + 1; i++)
            {
                retList.Add(new DateTimePeriod(baseDateTime.AddHours(i), baseDateTime.AddHours(i + 1)));
            }
            return retList;
        }

        private static IList<IMultiplicatorLayer> CreateMultiplicatorLayers(IMultiplicatorDefinitionSet multiplicatorDefinitionSet, IEnumerable<DateTimePeriod> periods, IMultiplicator multiplicator)
        {
            IList<IMultiplicatorLayer> retList = new List<IMultiplicatorLayer>();
            foreach (DateTimePeriod period in periods)
            {
                retList.Add(new MultiplicatorLayer(multiplicatorDefinitionSet, multiplicator, period));
            }
            return retList;
        }

        private static void SetDefinitionSetOnVisualLayer(IVisualLayer visualLayer, IMultiplicatorDefinitionSet definitionSet)
        {
            ((VisualLayer)visualLayer).DefinitionSet = definitionSet;
        }

        //Adds a layer to a existing collection
        private IVisualLayerCollection Add(IVisualLayerCollection collection,DateTimePeriod period,IMultiplicatorDefinitionSet definitionSet)
        {
            var layers = collection.ToList();
            IVisualLayer layerToAdd = _factory.CreateShiftSetupLayer(_activity, period);
            SetDefinitionSetOnVisualLayer(layerToAdd, definitionSet);
            layers.Add(layerToAdd);
            return new VisualLayerCollection(layers, new ProjectionPayloadMerger());
        }

        private DateOnly start
        {
            get { return _dateOnly.AddDays(-1); }
        }
        private DateOnly end
        {
            get { return _dateOnly.AddDays(1); }
        }
        #endregion //helpers

    }
}
