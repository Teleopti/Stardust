using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Restrictions
{
    [TestFixture]
    public class RestrictionCheckerTest
    {
        private RestrictionChecker _target;
        private MockRepository _mockRepository;
        private IScheduleDay _schedulePartMock;
        private IPerson _person;
        private DateTime _dateTime;
        private DateTimePeriod _dateTimePeriod;
        private IProjectionService _projectionService;
        private IVisualLayerCollection _visualLayerCollection;
        private IScheduleParameters _parameters;
        private IVisualLayerFactory _layerFactory;
        private IShiftCategory _shiftCategory;
        private IActivity _activity;

        [SetUp]
        public void Setup()
        {
			_mockRepository = new MockRepository();
            _layerFactory = new VisualLayerFactory();
            _dateTime = new DateTime(2006, 12, 23, 7, 30, 0);
            _dateTime = TimeZoneHelper.ConvertToUtc(_dateTime);
            _dateTimePeriod = new DateTimePeriod(_dateTime, TimeZoneHelper.ConvertToUtc(_dateTime.AddHours(12d)));
            _schedulePartMock = _mockRepository.StrictMock<IScheduleDay>();
            _person = PersonFactory.CreatePerson("testAgent");
            _person.PermissionInformation.SetDefaultTimeZone(new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")));
            _projectionService = _mockRepository.StrictMock<IProjectionService>();
            _visualLayerCollection = _mockRepository.StrictMock<IVisualLayerCollection>();
            _parameters = _mockRepository.StrictMock<IScheduleParameters>();
            _shiftCategory = new ShiftCategory("TopCat");
            _activity = new Activity("Activity");
        }

        #region Availability
        [Test]
        public void VerifyCanCalculateIllegalStartPermissionState()
        {
            IAvailabilityRestriction availabilityDayRestriction617 = new AvailabilityRestriction
                                                                            {
                StartTimeLimitation = new StartTimeLimitation(new TimeSpan(10, 0, 0),null),
                EndTimeLimitation = new EndTimeLimitation(null, new TimeSpan(17, 0, 0)),
            };

            var baseRestrictions = new List<IRestrictionBase> { availabilityDayRestriction617 };
            var dayRestrictions = new List<IPersistableScheduleData>();
            RecordMock(baseRestrictions, dayRestrictions);

            using (_mockRepository.Playback())
            {
                _target = new RestrictionChecker(_schedulePartMock);
                Assert.AreEqual(PermissionState.Broken, _target.CheckAvailability());
            }
        }
        
        [Test] 
        public void VerifyCanCalculateIllegalEndPermissionState()
        {
            IAvailabilityRestriction availabilityDayRestriction617 = new AvailabilityRestriction
            {
                StartTimeLimitation = new StartTimeLimitation(new TimeSpan(8, 0, 0), null),
                EndTimeLimitation = new EndTimeLimitation(null, new TimeSpan(21, 0, 0)),
            };

            
            var baseRestrictions = new List<IRestrictionBase> { availabilityDayRestriction617 };
            var dayRestrictions = new List<IPersistableScheduleData>();
            RecordMock(baseRestrictions, dayRestrictions);
            
            using (_mockRepository.Playback())
            {
                _target = new RestrictionChecker(_schedulePartMock);
                Assert.AreEqual(PermissionState.Broken, _target.CheckAvailability());
            }
        }

        [Test]
        public void VerifyCanCalculateIllegalLengthPermissionState()
        {
            IAvailabilityRestriction availabilityDayRestriction617 = new AvailabilityRestriction
                                                                           {
                                                                               StartTimeLimitation =
                                                                                   new StartTimeLimitation(
                                                                                   new TimeSpan(8, 0, 0), null),
                                                                               EndTimeLimitation =
                                                                                   new EndTimeLimitation(null,
                                                                                                         new TimeSpan(
                                                                                                             21, 0, 0)),
                                                                               WorkTimeLimitation =
                                                                                   new WorkTimeLimitation(new TimeSpan(0, 0, 0), new TimeSpan(21, 0, 0))// new TimePeriod(0,0,4,0)
            };

            var baseRestrictions = new List<IRestrictionBase> { availabilityDayRestriction617 };
            var dayRestrictions = new List<IPersistableScheduleData>();
            RecordMock(baseRestrictions, dayRestrictions);
            
            using (_mockRepository.Playback())
            {
                _target = new RestrictionChecker(_schedulePartMock);
                Assert.AreEqual(PermissionState.Broken, _target.CheckAvailability());
            }
        }

        [Test] 
        public void VerifyCanCalculateLegalPermissionState()
        {
            IAvailabilityRestriction availabilityDayRestriction620 = new AvailabilityRestriction
            {
                //Add timezone compensation to make the test runnable on all machines(independent of local timezone)
                StartTimeLimitation =
                    new StartTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(20, 0, 0)),
                EndTimeLimitation =
                new EndTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(20, 0, 0)),
                    NotAvailable = false
            };

            
            var baseRestrictions = new List<IRestrictionBase> { availabilityDayRestriction620 };
            var dayRestrictions = new List<IPersistableScheduleData>();
            RecordMock(baseRestrictions, dayRestrictions);
            
            using (_mockRepository.Playback())
            {
                _target = new RestrictionChecker(_schedulePartMock);
                Assert.AreEqual(PermissionState.Satisfied, _target.CheckAvailability());
            }
        }

        [Test]
        public void VerifyCanCalculateLegalLengthPermissionState()
        {
            IAvailabilityRestriction availabilityDayRestriction620 = new AvailabilityRestriction
                                                                           {
                                                                               WorkTimeLimitation =
                                                                                   new WorkTimeLimitation(
                                                                                   new TimeSpan(0, 0, 0),
                                                                                   new TimeSpan(12, 0, 0))
            };
            availabilityDayRestriction620.NotAvailable = false;

            
            var baseRestrictions = new List<IRestrictionBase> { availabilityDayRestriction620 };
            var dayRestrictions = new List<IPersistableScheduleData>();
            RecordMock(baseRestrictions, dayRestrictions);

            using (_mockRepository.Playback())
            {
                _target = new RestrictionChecker(_schedulePartMock);
                Assert.AreEqual(PermissionState.Satisfied, _target.CheckAvailability());
            }
        }

        [Test]
        public void VerifyCanCalculateUnspecifiedPermissionState()
        {
            //If the projection doesnt have any layers
            IAvailabilityRestriction availabilityDayRestriction620 = new AvailabilityRestriction
            {
                StartTimeLimitation =
                    new StartTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(20, 0, 0)),
                EndTimeLimitation =
                new EndTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(20, 0, 0)),
                NotAvailable = false
                    
            };

            var dayRestrictions = new List<IRestrictionBase> { availabilityDayRestriction620 };

            using (_mockRepository.Record())
            {
                Expect.Call(_schedulePartMock.Person).Return(_person).Repeat.Any();
                Expect.Call(_schedulePartMock.Period).Return(_dateTimePeriod).Repeat.Any();
                Expect.Call(_parameters.Person).Return(_person).Repeat.Any();
                Expect.Call(_schedulePartMock.RestrictionCollection())
                    .Return(dayRestrictions).Repeat.Any();
                
                Expect.Call(_schedulePartMock.ProjectionService()).Return(_projectionService);
                Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
                Expect.Call(_visualLayerCollection.HasLayers).Return(false).Repeat.Any();
                Expect.Call(_schedulePartMock.PersonDayOffCollection()).Return(
                   new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff>()));
            }

            using (_mockRepository.Playback())
            {
                _target = new RestrictionChecker(_schedulePartMock);
                Assert.AreEqual(PermissionState.Unspecified, _target.CheckAvailability());
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyCanCalculateIfDayOff()
        {
            //If the projection doesnt have any layers
            IAvailabilityRestriction availabilityDayRestriction620 = new AvailabilityRestriction
            {
                StartTimeLimitation =
                    new StartTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(20, 0, 0)),
                EndTimeLimitation =
                new EndTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(20, 0, 0)),
               NotAvailable = false

            };

            var dayRestrictions = new List<IRestrictionBase> { availabilityDayRestriction620 };
            ICccTimeZoneInfo timeZoneInfo = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            using (_mockRepository.Record())
            {
                Expect.Call(_schedulePartMock.Person).Return(_person).Repeat.Any();
                Expect.Call(_schedulePartMock.Period).Return(_dateTimePeriod).Repeat.Any();
                Expect.Call(_parameters.Person).Return(_person).Repeat.Any();
                Expect.Call(_schedulePartMock.RestrictionCollection())
                    .Return(dayRestrictions).Repeat.Any();
                Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
                Expect.Call(_schedulePartMock.ProjectionService()).Return(_projectionService);
                Expect.Call(_visualLayerCollection.HasLayers).Return(false).Repeat.Any();
                Expect.Call(_schedulePartMock.PersonDayOffCollection()).Return(
                   new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff> { new PersonDayOff(_person, new Scenario("Def"), new DayOff(), new DateOnly(_dateTime), timeZoneInfo) }));
            }

            using (_mockRepository.Playback())
            {
                _target = new RestrictionChecker(_schedulePartMock);
                Assert.AreEqual(PermissionState.Unspecified, _target.CheckAvailability());
            }
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyShowSatisfiedWhenDayOffAndAvailabilityFalse()
        {
            //If the projection doesnt have any layers
            IAvailabilityRestriction availabilityDayRestriction620 = new AvailabilityRestriction();
            availabilityDayRestriction620.NotAvailable = true;

            var dayRestrictions = new List<IRestrictionBase> { availabilityDayRestriction620 };
            ICccTimeZoneInfo _timeZoneInfo = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            using (_mockRepository.Record())
            {
                Expect.Call(_schedulePartMock.Person).Return(_person).Repeat.Any();
                Expect.Call(_schedulePartMock.Period).Return(_dateTimePeriod).Repeat.Any();
                Expect.Call(_parameters.Person).Return(_person).Repeat.Any();
                Expect.Call(_schedulePartMock.RestrictionCollection())
                    .Return(dayRestrictions).Repeat.Any();
                Expect.Call(_visualLayerCollection.HasLayers).Return(false).Repeat.Any();
                Expect.Call(_schedulePartMock.PersonDayOffCollection()).Return(
                   new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff> { new PersonDayOff(_person, new Scenario("Def"), new DayOff(), new DateOnly(_dateTime), _timeZoneInfo) }));
            }

            using (_mockRepository.Playback())
            {
                _target = new RestrictionChecker(_schedulePartMock);
                Assert.AreEqual(PermissionState.Satisfied, _target.CheckAvailability());
            }
        }

        [Test]
        public void VerifyCanCalculateNoAvailabilitiesRestrictions()
        {
            
            var dayRestrictions = new List<IPersistableScheduleData>();
            RecordMock(new List<IRestrictionBase>(), dayRestrictions);

            using (_mockRepository.Playback())
            {
                _target = new RestrictionChecker(_schedulePartMock);
                Assert.AreEqual(PermissionState.None, _target.CheckAvailability());
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyCanCalculateIllegalTimesAvailabilityPermissionState()
        {
            IShiftCategory shiftCategory = new ShiftCategory("TopCat");
            IMainShift mainShift = new MainShift(shiftCategory);
            IPersonAssignment assignment = new PersonAssignment(_person, new Scenario("Default"));
            assignment.SetMainShift(mainShift);


            IAvailabilityRestriction dayRestriction = new AvailabilityRestriction
            {
                //Add timezone compensation to make the test runnable on all machines(independent of local timezone)
                StartTimeLimitation =
                    new StartTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(10, 0, 0)),
                EndTimeLimitation =
                new EndTimeLimitation(
                    new TimeSpan(21, 0, 0),
                    new TimeSpan(20, 0, 0))
            };
            dayRestriction.NotAvailable = false;
            var dayRestrictions = new List<IRestrictionBase> { dayRestriction };

           
            using (_mockRepository.Record())
            {
                Expect.Call(_schedulePartMock.RestrictionCollection()).Return(dayRestrictions);
                Expect.Call(_schedulePartMock.ProjectionService()).Return(_projectionService);
                Expect.Call(_schedulePartMock.Person).Return(_person).Repeat.Once();
                Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
                Expect.Call(_visualLayerCollection.HasLayers).Return(true).Repeat.Any();
                Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
                Expect.Call(_schedulePartMock.PersonDayOffCollection()).Return(
                    new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff>())).Repeat.AtLeastOnce();
                Expect.Call(_visualLayerCollection.ContractTime()).Return(
                    _dateTimePeriod.EndDateTime.Subtract(_dateTimePeriod.StartDateTime));
            }
            using (_mockRepository.Playback())
            {
                _target = new RestrictionChecker(_schedulePartMock);
                Assert.AreEqual(PermissionState.Broken, _target.CheckAvailability());
            }
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyCanCalculateIllegalWorkLengthAvailabilityPermissionState()
        {
            IShiftCategory shiftCategory = new ShiftCategory("TopCat");
            IMainShift mainShift = new MainShift(shiftCategory);
            IPersonAssignment assignment = new PersonAssignment(_person, new Scenario("Default"));
            assignment.SetMainShift(mainShift);


            IAvailabilityRestriction dayRestriction = new AvailabilityRestriction
            {
                //Add timezone compensation to make the test runnable on all machines(independent of local timezone)
                WorkTimeLimitation =
                    new WorkTimeLimitation(
                    new TimeSpan(1, 0, 0),
                    new TimeSpan(3, 0, 0))
            };
            dayRestriction.NotAvailable = false;
            var dayRestrictions = new List<IRestrictionBase> { dayRestriction };


            using (_mockRepository.Record())
            {
                Expect.Call(_schedulePartMock.RestrictionCollection()).Return(dayRestrictions);
                Expect.Call(_schedulePartMock.ProjectionService()).Return(_projectionService);
                Expect.Call(_schedulePartMock.Person).Return(_person).Repeat.Once();
                Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
                Expect.Call(_visualLayerCollection.HasLayers).Return(true).Repeat.Any();
                Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
                Expect.Call(_schedulePartMock.PersonDayOffCollection()).Return(
                    new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff>())).Repeat.AtLeastOnce();
                Expect.Call(_visualLayerCollection.ContractTime()).Return(
                    _dateTimePeriod.EndDateTime.Subtract(_dateTimePeriod.StartDateTime));
            }
            using (_mockRepository.Playback())
            {
                _target = new RestrictionChecker(_schedulePartMock);
                Assert.AreEqual(PermissionState.Broken, _target.CheckAvailability());
            }
        }

        #endregion

        #region Rotations

        [Test]
        public void VerifyCanCalculateIllegalStartRotationPermissionState()
        {

            IRotationRestriction dayRestriction = new RotationRestriction
                        {
                            //Add timezone compensation to make the test runnable on all machines(independent of local timezone)
                            StartTimeLimitation =
                                new StartTimeLimitation(
                                new TimeSpan(9, 0, 0),
                                new TimeSpan(20, 0, 0)),
                            EndTimeLimitation =
                            new EndTimeLimitation(
                                new TimeSpan(9, 0, 0),
                                new TimeSpan(20, 0, 0))
                        };

            var baseRestrictions = new List<IRestrictionBase> { dayRestriction };
            var dayRestrictions = new List<IPersistableScheduleData>();
            RecordMock(baseRestrictions, dayRestrictions);

            using (_mockRepository.Playback())
            {
                _target = new RestrictionChecker(_schedulePartMock);
                Assert.AreEqual(PermissionState.Broken, _target.CheckRotations());
            }
        }

        [Test]
        public void VerifyCanCalculateIllegalEndRotationPermissionState()
        {
            IRotationRestriction dayRestriction = new RotationRestriction
            {
                //Add timezone compensation to make the test runnable on all machines(independent of local timezone)
                StartTimeLimitation =
                    new StartTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(17, 0, 0)),
                EndTimeLimitation =
                new EndTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(17, 0, 0))
            };

            var baseRestrictions = new List<IRestrictionBase> { dayRestriction };
            var dayRestrictions = new List<IPersistableScheduleData>();
            RecordMock(baseRestrictions, dayRestrictions);

            using (_mockRepository.Playback())
            {
                _target = new RestrictionChecker(_schedulePartMock);
                Assert.AreEqual(PermissionState.Broken, _target.CheckRotations());
            }
        }

        [Test]
        public void VerifyCanCalculateLegalTimesRotationsPermissionState()
        {

            IRotationRestriction dayRestriction = new RotationRestriction
            {
                //Add timezone compensation to make the test runnable on all machines(independent of local timezone)
                StartTimeLimitation =
                    new StartTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(20, 0, 0)),
                EndTimeLimitation =
                new EndTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(20, 0, 0))
            };

            var baseRestrictions = new List<IRestrictionBase> { dayRestriction };
            
            RecordMock(baseRestrictions,null);

            using (_mockRepository.Playback())
            {
                _target = new RestrictionChecker(_schedulePartMock);
                Assert.AreEqual(PermissionState.Satisfied, _target.CheckRotations());
            }
        }

        [Test]
        public void VerifyCanCalculateLegalShiftCategoryRotationsPermissionState()
        {
            IShiftCategory shiftCategory = new ShiftCategory("TopCat");
            IMainShift mainShift = new MainShift(shiftCategory);
            IPersonAssignment assignment = new PersonAssignment(_person, new Scenario("Default"));
            assignment.SetMainShift(mainShift);

            IRotationRestriction dayRestriction = new RotationRestriction
                                                      {
                //Add timezone compensation to make the test runnable on all machines(independent of local timezone)
                StartTimeLimitation =
                    new StartTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(20, 0, 0)),
                EndTimeLimitation =
                new EndTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(20, 0, 0)), ShiftCategory = shiftCategory
                    
            };

            var dayRestrictions = new List<IRestrictionBase> { dayRestriction };

            using (_mockRepository.Record())
            {
                Expect.Call(_schedulePartMock.PersonAssignmentCollection()).Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment>{assignment}));
                Expect.Call(_schedulePartMock.RestrictionCollection()).Return(dayRestrictions).Repeat.AtLeastOnce();
                Expect.Call(_schedulePartMock.ProjectionService()).Return(_projectionService);
                Expect.Call(_schedulePartMock.Person).Return(_person).Repeat.Once();
                Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
                Expect.Call(_visualLayerCollection.HasLayers).Return(true).Repeat.Any();
                Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
                Expect.Call(_schedulePartMock.PersonDayOffCollection()).Return(
                    new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff>())).Repeat.Twice();
                Expect.Call(_visualLayerCollection.ContractTime()).Return(
                    _dateTimePeriod.EndDateTime.Subtract(_dateTimePeriod.StartDateTime));
            }
            
            using (_mockRepository.Playback())
            {
                _target = new RestrictionChecker(_schedulePartMock);
                Assert.AreEqual(PermissionState.Satisfied, _target.CheckRotations());
            }
        }

        [Test]
        public void VerifyCanCalculateIllegalShiftCategoryRotationsPermissionState()
        {
            IShiftCategory wrongCategory = new ShiftCategory("WrongShiftCat");
            IShiftCategory shiftCategory = new ShiftCategory("TopCat");
            IMainShift mainShift = new MainShift(shiftCategory);
            IPersonAssignment assignment = new PersonAssignment(_person, new Scenario("Default"));
            assignment.SetMainShift(mainShift);

            IRotationRestriction dayRestriction = new RotationRestriction
            {
                StartTimeLimitation =
                    new StartTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(20, 0, 0)),
                EndTimeLimitation =
                new EndTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(20, 0, 0)),
                ShiftCategory = wrongCategory

            };

            var dayRestrictions = new List<IRestrictionBase> { dayRestriction };


            using (_mockRepository.Record())
            {
                Expect.Call(_schedulePartMock.PersonAssignmentCollection()).Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> { assignment }));
                Expect.Call(_schedulePartMock.RestrictionCollection()).Return(dayRestrictions).Repeat.AtLeastOnce();
                
                Expect.Call(_schedulePartMock.ProjectionService()).Return(_projectionService);
                Expect.Call(_schedulePartMock.Person).Return(_person).Repeat.Once();
                Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
                Expect.Call(_visualLayerCollection.HasLayers).Return(true).Repeat.Any();
                Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
                Expect.Call(_schedulePartMock.PersonDayOffCollection()).Return(
                    new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff>())).Repeat.Twice();
                Expect.Call(_visualLayerCollection.ContractTime()).Return(
                    _dateTimePeriod.EndDateTime.Subtract(_dateTimePeriod.StartDateTime));
               
            }

            using (_mockRepository.Playback())
            {
                _target = new RestrictionChecker(_schedulePartMock);
                Assert.AreEqual(PermissionState.Broken, _target.CheckRotations());
            }
        }

        [Test]
        public void VerifyCanCalculateIllegalWorkTimeRotationsPermissionState()
        {
            RotationRestriction dayRestriction = new RotationRestriction
            {
                //Add timezone compensation to make the test runnable on all machines(independent of local timezone)
                StartTimeLimitation =
                    new StartTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(20, 0, 0)),
                EndTimeLimitation =
                new EndTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(20, 0, 0)),
                WorkTimeLimitation = new WorkTimeLimitation(new TimeSpan(0, 0, 0), new TimeSpan(3, 0, 0))

            };

            var dayRestrictions = new List<IRestrictionBase> { dayRestriction };


            using (_mockRepository.Record())
            {
                Expect.Call(_schedulePartMock.PersonAssignmentCollection()).Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment>()));
                Expect.Call(_schedulePartMock.RestrictionCollection()).Return(dayRestrictions).Repeat.AtLeastOnce();
                Expect.Call(_schedulePartMock.ProjectionService()).Return(_projectionService);
                Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
                Expect.Call(_schedulePartMock.Person).Return(_person).Repeat.Once();
                Expect.Call(_visualLayerCollection.HasLayers).Return(true).Repeat.Any();
                Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
                Expect.Call(_schedulePartMock.PersonDayOffCollection()).Return(
                    new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff>())).Repeat.Twice();
                Expect.Call(_visualLayerCollection.ContractTime()).Return(
                    _dateTimePeriod.EndDateTime.Subtract(_dateTimePeriod.StartDateTime));
            }

            using (_mockRepository.Playback())
            {
                _target = new RestrictionChecker(_schedulePartMock);
                Assert.AreEqual(PermissionState.Broken, _target.CheckRotations());
            }
        }

        [Test]
        public void VerifyCanCalculateLegalWorkTimeRotationsPermissionState()
        {
            RotationRestriction dayRestriction = new RotationRestriction
            {
                //Add timezone compensation to make the test runnable on all machines(independent of local timezone)
                StartTimeLimitation =
                    new StartTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(20, 0, 0)),
                EndTimeLimitation =
                new EndTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(20, 0, 0)),
                WorkTimeLimitation = new WorkTimeLimitation(new TimeSpan(4, 0, 0), new TimeSpan(10, 0, 0))//new TimePeriod(4, 0, 10, 0)

            };

            var dayRestrictions = new List<IRestrictionBase> { dayRestriction };

            using (_mockRepository.Record())
            {
                Expect.Call(_schedulePartMock.PersonAssignmentCollection()).Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment>()));
                Expect.Call(_schedulePartMock.RestrictionCollection()).Return(dayRestrictions).Repeat.AtLeastOnce();
                Expect.Call(_schedulePartMock.ProjectionService()).Return(_projectionService);
                Expect.Call(_schedulePartMock.Person).Return(_person).Repeat.Once();
                Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
                Expect.Call(_visualLayerCollection.HasLayers).Return(true).Repeat.Any();
                Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
                Expect.Call(_schedulePartMock.PersonDayOffCollection()).Return(
                    new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff>())).Repeat.Twice();
                Expect.Call(_visualLayerCollection.ContractTime()).Return(
                    _dateTimePeriod.EndDateTime.Subtract(_dateTimePeriod.StartDateTime));
            }

            using (_mockRepository.Playback())
            {
                _target = new RestrictionChecker(_schedulePartMock);
                Assert.AreEqual(PermissionState.Broken, _target.CheckRotations());
            }
        }

        [Test]
        public void VerifyCanCalculateLegalDayOffRotationsPermissionState()
        {
            IDayOffTemplate dayOffTemplate = new DayOffTemplate(new Description("DayOffTemplate"));
            dayOffTemplate.Anchor = TimeSpan.FromHours(10);
            dayOffTemplate.SetTargetAndFlexibility(TimeSpan.FromHours(4), TimeSpan.FromHours(1));

            IPersonDayOff personDayOff = new PersonDayOff(_person,
                                   ScenarioFactory.CreateScenarioAggregate(),
                                   dayOffTemplate, new DateOnly(2007, 1, 1));

            IShiftCategory shiftCategory = new ShiftCategory("TopCat");
            IMainShift mainShift = new MainShift(shiftCategory);
            IPersonAssignment assignment = new PersonAssignment(_person, new Scenario("Default"));
            assignment.SetMainShift(mainShift);

            IRotationRestriction dayRestriction = new RotationRestriction
            {
                //Add timezone compensation to make the test runnable on all machines(independent of local timezone)
                StartTimeLimitation =
                    new StartTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(20, 0, 0)),
                EndTimeLimitation =
                new EndTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(20, 0, 0)),
                ShiftCategory = shiftCategory,
                DayOffTemplate = dayOffTemplate

            };

            var dayRestrictions = new List<IRestrictionBase> { dayRestriction };

            using (_mockRepository.Record())
            {
                Expect.Call(_schedulePartMock.RestrictionCollection()).Return(dayRestrictions).Repeat.AtLeastOnce();
                Expect.Call(_schedulePartMock.ProjectionService()).Return(_projectionService);
                Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
                Expect.Call(_visualLayerCollection.HasLayers).Return(true).Repeat.Any();
                Expect.Call(_schedulePartMock.PersonDayOffCollection()).Return(
                    new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff> { personDayOff })).Repeat.Twice();
                
            }

            using (_mockRepository.Playback())
            {
                _target = new RestrictionChecker(_schedulePartMock);
                Assert.AreEqual(PermissionState.Satisfied, _target.CheckRotations());
            }
        }

        [Test]
        public void VerifyCanCalculateIllegalDayOffRotationsPermissionState()
        {
            IDayOffTemplate dayOffTemplate = new DayOffTemplate(new Description("DayOffTemplate"));
            dayOffTemplate.Anchor = TimeSpan.FromHours(10);
            dayOffTemplate.SetTargetAndFlexibility(TimeSpan.FromHours(4), TimeSpan.FromHours(1));

            IPersonDayOff personDayOff = new PersonDayOff(_person,
                                   ScenarioFactory.CreateScenarioAggregate(),
                                   dayOffTemplate, new DateOnly(2007, 1, 1));


            IShiftCategory shiftCategory = new ShiftCategory("TopCat");
            IMainShift mainShift = new MainShift(shiftCategory);
            IPersonAssignment assignment = new PersonAssignment(_person, new Scenario("Default"));
            assignment.SetMainShift(mainShift);


            IRotationRestriction dayRestriction = new RotationRestriction
            {
                //Add timezone compensation to make the test runnable on all machines(independent of local timezone)
                StartTimeLimitation =
                    new StartTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(20, 0, 0)),
                EndTimeLimitation =
                new EndTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(20, 0, 0)),
                ShiftCategory = shiftCategory,
                DayOffTemplate = new DayOffTemplate(new Description("WrongDayOff"))

            };

            var dayRestrictions = new List<IRestrictionBase> { dayRestriction };

            using (_mockRepository.Record())
            {
                Expect.Call(_schedulePartMock.PersonAssignmentConflictCollection).Return(new List<IPersonAssignment> { assignment }).Repeat.Any();
                Expect.Call(_schedulePartMock.RestrictionCollection()).Return(dayRestrictions).Repeat.AtLeastOnce();
                Expect.Call(_visualLayerCollection.HasLayers).Return(true).Repeat.Any();
                Expect.Call(_schedulePartMock.PersonDayOffCollection()).Return(
                    new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff> { personDayOff }));
            }

            using (_mockRepository.Playback())
            {
                _target = new RestrictionChecker(_schedulePartMock);
                Assert.AreEqual(PermissionState.Broken, _target.CheckRotations());
            }
        }

        [Test]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public void VerifyCanCalculateIllegalDayOffRotationsPermissionState2()
        {
            IDayOffTemplate dayOffTemplate = new DayOffTemplate(new Description("DayOffTemplate"));
            dayOffTemplate.Anchor = TimeSpan.FromHours(10);
            dayOffTemplate.SetTargetAndFlexibility(TimeSpan.FromHours(4), TimeSpan.FromHours(1));

            IPersonDayOff personDayOff = new PersonDayOff(_person,
                                   ScenarioFactory.CreateScenarioAggregate(),
                                   dayOffTemplate, new DateOnly(2007, 1, 1));


            IShiftCategory shiftCategory = new ShiftCategory("TopCat");
            IMainShift mainShift = new MainShift(shiftCategory);
            IPersonAssignment assignment = new PersonAssignment(_person, new Scenario("Default"));
            assignment.SetMainShift(mainShift);


            IRotationRestriction dayRestriction = new RotationRestriction
            {
                //Add timezone compensation to make the test runnable on all machines(independent of local timezone)
                StartTimeLimitation =
                    new StartTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(20, 0, 0)),
                EndTimeLimitation =
                new EndTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(20, 0, 0)),
                ShiftCategory = shiftCategory,
                DayOffTemplate = new DayOffTemplate(new Description("WrongDayOff"))

            };

            var dayRestrictions = new List<IRestrictionBase> { dayRestriction };

            using (_mockRepository.Record())
            {
                Expect.Call(_schedulePartMock.PersonAssignmentConflictCollection).Return(new List<IPersonAssignment> { assignment }).Repeat.Any();
                Expect.Call(_schedulePartMock.RestrictionCollection()).Return(dayRestrictions).Repeat.AtLeastOnce();
                Expect.Call(_visualLayerCollection.HasLayers).Return(true).Repeat.Any();
                Expect.Call(_schedulePartMock.PersonDayOffCollection()).Return(
                    new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff> { personDayOff }));
            }

            using (_mockRepository.Playback())
            {
                _target = new RestrictionChecker(_schedulePartMock);
                Assert.AreEqual(PermissionState.Broken, _target.CheckRotations());
            }
        }

        [Test]
        public void VerifyShiftRotationWithDayOffShouldReturnBroken()
        {
            IScheduleDay scheduleDay = _mockRepository.StrictMock<IScheduleDay>();
            RotationRestriction rotationRestriction = new RotationRestriction();
            rotationRestriction.ShiftCategory = ShiftCategoryFactory.CreateShiftCategory("Test");
            IPersonDayOff personDayOff = PersonDayOffFactory.CreatePersonDayOff();
            _target = new RestrictionChecker(scheduleDay);

            using(_mockRepository.Record())
            {
                Expect.Call(scheduleDay.RestrictionCollection())
                    .Return(new List<IRestrictionBase>{ rotationRestriction }).Repeat.AtLeastOnce();
                Expect.Call(scheduleDay.PersonDayOffCollection())
                    .Return(new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff>{ personDayOff })).Repeat.AtLeastOnce();
            }
            using(_mockRepository.Playback())
            {
                Assert.AreEqual(PermissionState.Broken, _target.CheckRotations());
            }
        }

        [Test]
        public void VerifyCanCalculateNoDayOffOnShiftRotationsPermissionState()
        {
            IShiftCategory shiftCategory = new ShiftCategory("TopCat");
            IMainShift mainShift = new MainShift(shiftCategory);
            IPersonAssignment assignment = new PersonAssignment(_person, new Scenario("Default"));
            assignment.SetMainShift(mainShift);


            IRotationRestriction dayRestriction = new RotationRestriction
            {
                //Add timezone compensation to make the test runnable on all machines(independent of local timezone)
                StartTimeLimitation =
                    new StartTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(20, 0, 0)),
                EndTimeLimitation =
                new EndTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(20, 0, 0)),
                ShiftCategory = shiftCategory,
                DayOffTemplate = new DayOffTemplate(new Description("WrongDayOff"))

            };

            var dayRestrictions = new List<IRestrictionBase> { dayRestriction };


            using (_mockRepository.Record())
            {
                Expect.Call(_schedulePartMock.PersonAssignmentConflictCollection).Return(new List<IPersonAssignment> { assignment }).Repeat.Any();
                Expect.Call(_schedulePartMock.RestrictionCollection()).Return(dayRestrictions).Repeat.AtLeastOnce();
                //Expect.Call(_schedulePartMock.ProjectionService()).Return(_projectionService);
                //Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
                Expect.Call(_visualLayerCollection.HasLayers).Return(true).Repeat.Any();
                Expect.Call(_schedulePartMock.PersonDayOffCollection()).Return(
                    new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff>()));
                //Expect.Call(_schedulePartMock.PersistableScheduleDataCollection()).Return(
                //    new List<IPersistableScheduleData>());

            }

            using (_mockRepository.Playback())
            {
                _target = new RestrictionChecker(_schedulePartMock);
                Assert.AreEqual(PermissionState.Broken, _target.CheckRotations());
            }
        }

        [Test]
        public void VerifyCanCalculateNoRotationsPermissionState()
        {
            IShiftCategory shiftCategory = new ShiftCategory("TopCat");
            IMainShift mainShift = new MainShift(shiftCategory);
            IPersonAssignment assignment = new PersonAssignment(_person, new Scenario("Default"));
            assignment.SetMainShift(mainShift);

            var dayRestrictions = new List<IRestrictionBase>();

            using (_mockRepository.Record())
            {
                Expect.Call(_schedulePartMock.PersonAssignmentConflictCollection).Return(new List<IPersonAssignment> { assignment }).Repeat.Any();
                Expect.Call(_schedulePartMock.RestrictionCollection()).Return(dayRestrictions);
                
                Expect.Call(_visualLayerCollection.HasLayers).Return(true).Repeat.Any();
            }

            using (_mockRepository.Playback())
            {
                _target = new RestrictionChecker(_schedulePartMock);
                Assert.AreEqual(PermissionState.None, _target.CheckRotations());
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyCanCalculateIllegalShiftCategoryWithNoMainShiftRotationPermissionState()
        {
            IActivity activity = new Activity("Activity");
            IList<IVisualLayer> layerCollection = new List<IVisualLayer>();
            layerCollection.Add(_layerFactory.CreateShiftSetupLayer(activity, _dateTimePeriod));

            IShiftCategory shiftCategory = new ShiftCategory("ShiftCat");
            IPersonAssignment assignment = new PersonAssignment(_person, new Scenario("Default"));

            IRotationRestriction dayRestriction = new RotationRestriction
            {
                StartTimeLimitation =
                    new StartTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(20, 0, 0)),
                EndTimeLimitation =
                new EndTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(20, 0, 0)),
                ShiftCategory = shiftCategory

            };

            var dayRestrictions = new List<IRestrictionBase> { dayRestriction };


            using (_mockRepository.Record())
            {
                Expect.Call(_schedulePartMock.PersonAssignmentCollection()).Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> { assignment })).Repeat.Any();
                Expect.Call(_schedulePartMock.RestrictionCollection()).Return(dayRestrictions).Repeat.AtLeastOnce();
               
                Expect.Call(_schedulePartMock.ProjectionService()).Return(_projectionService);
                Expect.Call(_schedulePartMock.Person).Return(_person).Repeat.Once();
                Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
                Expect.Call(_visualLayerCollection.HasLayers).Return(true).Repeat.Any();
                Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
                Expect.Call(_schedulePartMock.PersonDayOffCollection()).Return(
                    new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff>())).Repeat.Twice();
                Expect.Call(_visualLayerCollection.ContractTime()).Return(
                    _dateTimePeriod.EndDateTime.Subtract(_dateTimePeriod.StartDateTime));
                
            }

            using (_mockRepository.Playback())
            {
                _target = new RestrictionChecker(_schedulePartMock);
                Assert.AreEqual(PermissionState.Broken, _target.CheckRotations());
            }
        }

        #endregion

        #region Preference

        [Test] 
        public void VerifyCanCalculateIllegalStartPreferencePermissionState()
        {
            IActivity activity = new Activity("Activity");
            IList<IVisualLayer> layerCollection = new List<IVisualLayer>();
            layerCollection.Add(_layerFactory.CreateShiftSetupLayer(activity, _dateTimePeriod));

            IShiftCategory shiftCategory = new ShiftCategory("TopCat");
            IMainShift mainShift = new MainShift(shiftCategory);
            IPersonAssignment assignment = new PersonAssignment(_person, new Scenario("Default"));
            assignment.SetMainShift(mainShift);

            PreferenceRestriction dayRestriction = new PreferenceRestriction
            {
                //Add timezone compensation to make the test runnable on all machines(independent of local timezone)
                StartTimeLimitation =
                    new StartTimeLimitation(
                    new TimeSpan(9, 0, 0),
                    new TimeSpan(20, 0, 0)),
                EndTimeLimitation =
                new EndTimeLimitation(
                    new TimeSpan(9, 0, 0),
                    new TimeSpan(20, 0, 0))
            };

            IPreferenceDay personRestriction = new PreferenceDay(_person, new DateOnly(_dateTime), dayRestriction);
            IList<IPersistableScheduleData> list = new List<IPersistableScheduleData> { personRestriction };
            ReadOnlyCollection<IPersistableScheduleData> dayRestrictions = new ReadOnlyCollection<IPersistableScheduleData>(list);

			var filteredVisualLayers = _mockRepository.StrictMock<IFilteredVisualLayerCollection>();

            using (_mockRepository.Record())
            {
                Expect.Call(_schedulePartMock.PersonAssignmentCollection()).Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> { assignment }));
                Expect.Call(_schedulePartMock.PersistableScheduleDataCollection()).Return(dayRestrictions).Repeat.AtLeastOnce();
                Expect.Call(_schedulePartMock.ProjectionService()).Return(_projectionService);
                Expect.Call(_schedulePartMock.Person).Return(_person).Repeat.Once();
				Expect.Call(_schedulePartMock.SignificantPart()).Return(SchedulePartView.None);
				Expect.Call(_schedulePartMock.IsScheduled()).Return(false);

                Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
                
				Expect.Call(_visualLayerCollection.HasLayers).Return(true).Repeat.Any();
                Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
                Expect.Call(_visualLayerCollection.FilterLayers(new Activity("T"))).Return(filteredVisualLayers).IgnoreArguments();
                Expect.Call(_schedulePartMock.PersonDayOffCollection()).Return(
                    new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff>()));
                Expect.Call(_visualLayerCollection.ContractTime()).Return(
                    _dateTimePeriod.EndDateTime.Subtract(_dateTimePeriod.StartDateTime));
            	Expect.Call(filteredVisualLayers.GetEnumerator()).Return(layerCollection.GetEnumerator());
            }

            using (_mockRepository.Playback())
            {
                _target = new RestrictionChecker(_schedulePartMock);
                Assert.AreEqual(PermissionState.Broken, _target.CheckPreference());
            }
        }

        [Test]
        public void VerifyCanCalculateIllegalEndPreferencePermissionState()
        {
            IActivity activity = new Activity("Activity");
            IList<IVisualLayer> layerCollection = new List<IVisualLayer>();
            layerCollection.Add(_layerFactory.CreateShiftSetupLayer(activity, _dateTimePeriod));

            IShiftCategory shiftCategory = new ShiftCategory("TopCat");
            IMainShift mainShift = new MainShift(shiftCategory);
            IPersonAssignment assignment = new PersonAssignment(_person, new Scenario("Default"));
            assignment.SetMainShift(mainShift);

            PreferenceRestriction dayRestriction = new PreferenceRestriction
            {
                //Add timezone compensation to make the test runnable on all machines(independent of local timezone)
                StartTimeLimitation =
                    new StartTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(17, 0, 0)),
                EndTimeLimitation =
                new EndTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(17, 0, 0))
            };

            IPreferenceDay personRestriction = new PreferenceDay(_person, new DateOnly(_dateTime), dayRestriction);
            IList<IPersistableScheduleData> list = new List<IPersistableScheduleData> { personRestriction };
            ReadOnlyCollection<IPersistableScheduleData> dayRestrictions = new ReadOnlyCollection<IPersistableScheduleData>(list);
			
			var filteredVisualLayers = _mockRepository.StrictMock<IFilteredVisualLayerCollection>();

            using (_mockRepository.Record())
            {
                Expect.Call(_schedulePartMock.PersonAssignmentCollection()).Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> { assignment }));
                Expect.Call(_schedulePartMock.PersistableScheduleDataCollection()).Return(dayRestrictions).Repeat.AtLeastOnce();
                Expect.Call(_schedulePartMock.Person).Return(_person).Repeat.Once();
                Expect.Call(_schedulePartMock.ProjectionService()).Return(_projectionService);
				Expect.Call(_schedulePartMock.SignificantPart()).Return(SchedulePartView.None);
				Expect.Call((_schedulePartMock.IsScheduled())).Return(false);

                Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
                Expect.Call(_visualLayerCollection.HasLayers).Return(true).Repeat.Any();
                Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
                Expect.Call(_visualLayerCollection.FilterLayers(new Activity("T"))).Return(filteredVisualLayers).IgnoreArguments();
                Expect.Call(_schedulePartMock.PersonDayOffCollection()).Return(
                    new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff>()));
                Expect.Call(_visualLayerCollection.ContractTime()).Return(
                    _dateTimePeriod.EndDateTime.Subtract(_dateTimePeriod.StartDateTime));
            	Expect.Call(filteredVisualLayers.GetEnumerator()).Return(layerCollection.GetEnumerator());
            }

            using (_mockRepository.Playback())
            {
                _target = new RestrictionChecker(_schedulePartMock);
                Assert.AreEqual(PermissionState.Broken, _target.CheckPreference());
            }
        }

        [Test]
        public void VerifyCanCalculateLegalTimesPreferencePermissionState()
        {
            IActivity activity = new Activity("Activity");
            IList<IVisualLayer> layerCollection = new List<IVisualLayer>();
            layerCollection.Add(_layerFactory.CreateShiftSetupLayer(activity, _dateTimePeriod));

            IShiftCategory shiftCategory = new ShiftCategory("TopCat");
            IMainShift mainShift = new MainShift(shiftCategory);
            IPersonAssignment assignment = new PersonAssignment(_person, new Scenario("Default"));
            assignment.SetMainShift(mainShift);

            PreferenceRestriction dayRestriction = new PreferenceRestriction
            {
                //Add timezone compensation to make the test runnable on all machines(independent of local timezone)
                StartTimeLimitation =
                    new StartTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(20, 0, 0)),
                EndTimeLimitation =
                new EndTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(20, 0, 0))
            };

            IPreferenceDay personRestriction = new PreferenceDay(_person, new DateOnly(_dateTime), dayRestriction);
            IList<IPersistableScheduleData> list = new List<IPersistableScheduleData> { personRestriction };
            ReadOnlyCollection<IPersistableScheduleData> dayRestrictions = new ReadOnlyCollection<IPersistableScheduleData>(list);
			var filteredVisualLayers = _mockRepository.StrictMock<IFilteredVisualLayerCollection>();

            using (_mockRepository.Record())
            {
                Expect.Call(_schedulePartMock.PersonAssignmentCollection()).Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> { assignment }));
                Expect.Call(_schedulePartMock.PersistableScheduleDataCollection()).Return(dayRestrictions).Repeat.AtLeastOnce();
                Expect.Call(_schedulePartMock.ProjectionService()).Return(_projectionService);
                Expect.Call(_schedulePartMock.Person).Return(_person).Repeat.Once();
				Expect.Call(_schedulePartMock.SignificantPart()).Return(SchedulePartView.None);
				Expect.Call((_schedulePartMock.IsScheduled())).Return(false);

                Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
                Expect.Call(_visualLayerCollection.HasLayers).Return(true).Repeat.Any();
                Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
                Expect.Call(_visualLayerCollection.FilterLayers(new Activity("T"))).Return(filteredVisualLayers).IgnoreArguments();
                Expect.Call(_schedulePartMock.PersonDayOffCollection()).Return(
                    new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff>()));
                Expect.Call(_visualLayerCollection.ContractTime()).Return(
					_dateTimePeriod.EndDateTime.Subtract(_dateTimePeriod.StartDateTime));
				Expect.Call(filteredVisualLayers.GetEnumerator()).Return(layerCollection.GetEnumerator());
            }

            using (_mockRepository.Playback())
            {
                _target = new RestrictionChecker(_schedulePartMock);
                Assert.AreEqual(PermissionState.Satisfied, _target.CheckPreference());
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyCanCalculateLegalTimesOverMidnightPreferencePermissionState()
        {
            IActivity activity = new Activity("Activity");
            IList<IVisualLayer> layerCollection = new List<IVisualLayer>();
            DateTime dateTimeStart = new DateTime(2010, 1,10,22,0,0, DateTimeKind.Utc);
            DateTime dateTimeEnd = dateTimeStart.AddHours(7);
            DateTimePeriod period = new DateTimePeriod(dateTimeStart, dateTimeEnd);
            layerCollection.Add(_layerFactory.CreateShiftSetupLayer(activity, period));
            IShiftCategory shiftCategory = new ShiftCategory("TopCat");
            IMainShift mainShift = new MainShift(shiftCategory);
            IPersonAssignment assignment = new PersonAssignment(_person, new Scenario("Default"));
            assignment.SetMainShift(mainShift);

            PreferenceRestriction dayRestriction = new PreferenceRestriction
            {
                //Add timezone compensation to make the test runnable on all machines(independent of local timezone)
                StartTimeLimitation =
                    new StartTimeLimitation(
                    new TimeSpan(23, 0, 0),
                    new TimeSpan(23, 0, 0)),
                EndTimeLimitation =
                new EndTimeLimitation(
                    new TimeSpan(1, 6, 0, 0),
                    new TimeSpan(1, 6, 0, 0))
            };

            IPreferenceDay personRestriction = new PreferenceDay(_person, new DateOnly(_dateTime), dayRestriction);
            IList<IPersistableScheduleData> list = new List<IPersistableScheduleData> { personRestriction };
            ReadOnlyCollection<IPersistableScheduleData> dayRestrictions = new ReadOnlyCollection<IPersistableScheduleData>(list);
			var filteredVisualLayers = _mockRepository.StrictMock<IFilteredVisualLayerCollection>();

            using (_mockRepository.Record())
            {
                Expect.Call(_schedulePartMock.PersonAssignmentCollection()).Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> { assignment }));
                Expect.Call(_schedulePartMock.PersistableScheduleDataCollection()).Return(dayRestrictions).Repeat.AtLeastOnce();
                Expect.Call(_schedulePartMock.ProjectionService()).Return(_projectionService);
                Expect.Call(_schedulePartMock.Person).Return(_person).Repeat.Once();
				Expect.Call(_schedulePartMock.SignificantPart()).Return(SchedulePartView.None);
				Expect.Call((_schedulePartMock.IsScheduled())).Return(false);

                Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
                Expect.Call(_visualLayerCollection.HasLayers).Return(true).Repeat.Any();
                Expect.Call(_visualLayerCollection.Period()).Return(period);
                Expect.Call(_visualLayerCollection.FilterLayers(new Activity("T"))).Return(filteredVisualLayers).IgnoreArguments();
                Expect.Call(_schedulePartMock.PersonDayOffCollection()).Return(
                    new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff>()));
                Expect.Call(_visualLayerCollection.ContractTime()).Return(
                    period.EndDateTime.Subtract(period.StartDateTime));
				Expect.Call(filteredVisualLayers.GetEnumerator()).Return(layerCollection.GetEnumerator());
            }

            using (_mockRepository.Playback())
            {
                _target = new RestrictionChecker(_schedulePartMock);
                Assert.AreEqual(PermissionState.Satisfied, _target.CheckPreference());
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyCanCalculateIllegalTimesOverMidnightPreferencePermissionState()
        {
            IActivity activity = new Activity("Activity");
            IList<IVisualLayer> layerCollection = new List<IVisualLayer>();
            DateTime dateTimeStart = new DateTime(2010, 1, 10, 22, 0, 0, DateTimeKind.Utc);
            DateTime dateTimeEnd = dateTimeStart.AddHours(7);
            DateTimePeriod period = new DateTimePeriod(dateTimeStart, dateTimeEnd);
            layerCollection.Add(_layerFactory.CreateShiftSetupLayer(activity, period));
            IShiftCategory shiftCategory = new ShiftCategory("TopCat");
            IMainShift mainShift = new MainShift(shiftCategory);
            IPersonAssignment assignment = new PersonAssignment(_person, new Scenario("Default"));
            assignment.SetMainShift(mainShift);

            PreferenceRestriction dayRestriction = new PreferenceRestriction
            {
                //Add timezone compensation to make the test runnable on all machines(independent of local timezone)
                StartTimeLimitation =
                    new StartTimeLimitation(
                    new TimeSpan(23, 0, 0),
                    new TimeSpan(23, 0, 0)),
                EndTimeLimitation =
                new EndTimeLimitation(
                    new TimeSpan(1, 7, 0, 0),
                    new TimeSpan(1, 7, 0, 0))
            };

            IPreferenceDay personRestriction = new PreferenceDay(_person, new DateOnly(_dateTime), dayRestriction);
            IList<IPersistableScheduleData> list = new List<IPersistableScheduleData> { personRestriction };
            ReadOnlyCollection<IPersistableScheduleData> dayRestrictions = new ReadOnlyCollection<IPersistableScheduleData>(list);
			var filteredVisualLayers = _mockRepository.StrictMock<IFilteredVisualLayerCollection>();

            using (_mockRepository.Record())
            {
                Expect.Call(_schedulePartMock.PersonAssignmentCollection()).Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> { assignment }));
                Expect.Call(_schedulePartMock.PersistableScheduleDataCollection()).Return(dayRestrictions).Repeat.AtLeastOnce();
                Expect.Call(_schedulePartMock.ProjectionService()).Return(_projectionService);
                Expect.Call(_schedulePartMock.Person).Return(_person).Repeat.Once();
				Expect.Call(_schedulePartMock.SignificantPart()).Return(SchedulePartView.None);
				Expect.Call((_schedulePartMock.IsScheduled())).Return(false);

                Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
                Expect.Call(_visualLayerCollection.HasLayers).Return(true).Repeat.Any();
                Expect.Call(_visualLayerCollection.Period()).Return(period);
                Expect.Call(_visualLayerCollection.FilterLayers(new Activity("T"))).Return(filteredVisualLayers).IgnoreArguments();
                Expect.Call(_schedulePartMock.PersonDayOffCollection()).Return(
                    new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff>()));
                Expect.Call(_visualLayerCollection.ContractTime()).Return(
                    period.EndDateTime.Subtract(period.StartDateTime));
            	Expect.Call(filteredVisualLayers.GetEnumerator()).Return(layerCollection.GetEnumerator());
            }

            using (_mockRepository.Playback())
            {
                _target = new RestrictionChecker(_schedulePartMock);
                Assert.AreEqual(PermissionState.Broken, _target.CheckPreference());
            }
        }

        [Test]
        public void VerifyCanCalculateIllegalWorkTimePreferencePermissionState()
        {
            IActivity activity = new Activity("Activity");
            IList<IVisualLayer> layerCollection = new List<IVisualLayer>();
            layerCollection.Add(_layerFactory.CreateShiftSetupLayer(activity, _dateTimePeriod));

            IShiftCategory shiftCategory = new ShiftCategory("TopCat");
            IMainShift mainShift = new MainShift(shiftCategory);
            IPersonAssignment assignment = new PersonAssignment(_person, new Scenario("Default"));
            assignment.SetMainShift(mainShift);

            IPreferenceRestriction dayRestriction = new PreferenceRestriction
            {
                //Add timezone compensation to make the test runnable on all machines(independent of local timezone)
                StartTimeLimitation =
                    new StartTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(20, 0, 0)),
                EndTimeLimitation =
                new EndTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(20, 0, 0)),
                WorkTimeLimitation = new WorkTimeLimitation(new TimeSpan(12,0,0), new TimeSpan(14,0,0)) //TimePeriod(12,0,14,0)
            };

            IPreferenceDay personRestriction = new PreferenceDay(_person, new DateOnly(_dateTime), dayRestriction);
            IList<IPersistableScheduleData> list = new List<IPersistableScheduleData> { personRestriction };
            ReadOnlyCollection<IPersistableScheduleData> dayRestrictions = new ReadOnlyCollection<IPersistableScheduleData>(list);
			var filteredVisualLayers = _mockRepository.StrictMock<IFilteredVisualLayerCollection>();

            using (_mockRepository.Record())
            {
                Expect.Call(_schedulePartMock.PersonAssignmentCollection()).Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> { assignment }));
                Expect.Call(_schedulePartMock.PersistableScheduleDataCollection()).Return(dayRestrictions).Repeat.AtLeastOnce();
                Expect.Call(_schedulePartMock.ProjectionService()).Return(_projectionService);
				Expect.Call(_schedulePartMock.SignificantPart()).Return(SchedulePartView.None);
				Expect.Call((_schedulePartMock.IsScheduled())).Return(false);

                Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
                Expect.Call(_schedulePartMock.Person).Return(_person).Repeat.Once();
                Expect.Call(_visualLayerCollection.HasLayers).Return(true).Repeat.Any();
                Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
                Expect.Call(_visualLayerCollection.FilterLayers(new Activity("T"))).Return(filteredVisualLayers).IgnoreArguments();
                Expect.Call(_schedulePartMock.PersonDayOffCollection()).Return(
                    new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff>()));
                Expect.Call(_visualLayerCollection.ContractTime()).Return(
                    _dateTimePeriod.EndDateTime.Subtract(_dateTimePeriod.StartDateTime));
            	Expect.Call(filteredVisualLayers.GetEnumerator()).Return(layerCollection.GetEnumerator());
            }

            using (_mockRepository.Playback())
            {
                _target = new RestrictionChecker(_schedulePartMock);
                Assert.AreEqual(PermissionState.Broken, _target.CheckPreference());
            }
        }

        [Test]
        public void VerifyCanCalculateLegalShiftCategoryPreferencePermissionState()
        {
            IActivity activity = new Activity("Activity");
            IList<IVisualLayer> layerCollection = new List<IVisualLayer>();
            layerCollection.Add(_layerFactory.CreateShiftSetupLayer(activity, _dateTimePeriod));

            IShiftCategory shiftCategory = new ShiftCategory("TopCat");
            IMainShift mainShift = new MainShift(shiftCategory);
            IPersonAssignment assignment = new PersonAssignment(_person, new Scenario("Default"));
            assignment.SetMainShift(mainShift);

            PreferenceRestriction dayRestriction = new PreferenceRestriction
            {
                //Add timezone compensation to make the test runnable on all machines(independent of local timezone)
                StartTimeLimitation =
                    new StartTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(20, 0, 0)),
                EndTimeLimitation =
                new EndTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(20, 0, 0)),
                ShiftCategory = shiftCategory

            };

            IPreferenceDay personRestriction = new PreferenceDay(_person, new DateOnly(_dateTime), dayRestriction);
            IList<IPersistableScheduleData> list = new List<IPersistableScheduleData> { personRestriction };
            ReadOnlyCollection<IPersistableScheduleData> dayRestrictions = new ReadOnlyCollection<IPersistableScheduleData>(list);
			var filteredVisualLayers = _mockRepository.StrictMock<IFilteredVisualLayerCollection>();

            using (_mockRepository.Record())
            {
                Expect.Call(_schedulePartMock.PersonAssignmentCollection()).Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> { assignment }));
                Expect.Call(_schedulePartMock.PersistableScheduleDataCollection()).Return(dayRestrictions).Repeat.AtLeastOnce();
                Expect.Call(_schedulePartMock.ProjectionService()).Return(_projectionService);
                Expect.Call(_schedulePartMock.Person).Return(_person).Repeat.Once();
				Expect.Call(_schedulePartMock.SignificantPart()).Return(SchedulePartView.None);
				Expect.Call(_schedulePartMock.IsScheduled()).Return(false);

                Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
                Expect.Call(_visualLayerCollection.HasLayers).Return(true).Repeat.Any();
                Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
                Expect.Call(_visualLayerCollection.FilterLayers(new Activity("T"))).Return(filteredVisualLayers).IgnoreArguments();
                Expect.Call(_schedulePartMock.PersonDayOffCollection()).Return(
                    new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff>()));
                Expect.Call(_visualLayerCollection.ContractTime()).Return(
                    _dateTimePeriod.EndDateTime.Subtract(_dateTimePeriod.StartDateTime));
				Expect.Call(filteredVisualLayers.GetEnumerator()).Return(layerCollection.GetEnumerator());
            }

            using (_mockRepository.Playback())
            {
                _target = new RestrictionChecker(_schedulePartMock);
                Assert.AreEqual(PermissionState.Satisfied, _target.CheckPreference());
            }
        }

        [Test]
        public void ShouldCalculateLegalShiftCategoryPreferencePermissionStateMustHave()
        {
            using (_mockRepository.Record())
            {
				var layerCollection = CreateLayerCollection();
				var assignment = CreatePersonAssignment();
				var dayRestriction = CreatePreferenceRestriction();

				IPreferenceDay personRestriction = new PreferenceDay(_person, new DateOnly(_dateTime), dayRestriction);
				IList<IPersistableScheduleData> list = new List<IPersistableScheduleData> { personRestriction };
				var dayRestrictions = new ReadOnlyCollection<IPersistableScheduleData>(list);
				var filteredVisualLayers = _mockRepository.StrictMock<IFilteredVisualLayerCollection>();

				Expect.Call(_schedulePartMock.PersonAssignmentCollection()).Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> { assignment }));
				Expect.Call(_schedulePartMock.PersistableScheduleDataCollection()).Return(dayRestrictions).Repeat.AtLeastOnce();
				Expect.Call(_schedulePartMock.ProjectionService()).Return(_projectionService);
				Expect.Call(_schedulePartMock.Person).Return(_person).Repeat.Once();
				Expect.Call(_schedulePartMock.SignificantPart()).Return(SchedulePartView.None);
				Expect.Call((_schedulePartMock.IsScheduled())).Return(false);

				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);

				Expect.Call(_visualLayerCollection.HasLayers).Return(true).Repeat.Any();
				Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
				Expect.Call(_visualLayerCollection.FilterLayers(new Activity("T"))).Return(filteredVisualLayers).IgnoreArguments();

				Expect.Call(_schedulePartMock.PersonDayOffCollection()).Return(
					new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff>())).Repeat.AtLeastOnce();
				Expect.Call(_visualLayerCollection.ContractTime()).Return(
					_dateTimePeriod.EndDateTime.Subtract(_dateTimePeriod.StartDateTime));
				Expect.Call(filteredVisualLayers.GetEnumerator()).Return(layerCollection.GetEnumerator()).Repeat.Any();
            }

            using (_mockRepository.Playback())
            {
                _target = new RestrictionChecker(_schedulePartMock);
                Assert.AreEqual(PermissionState.Satisfied, _target.CheckPreferenceMustHave());
            }
        }

        private PreferenceRestriction CreatePreferenceRestriction()
        {
            var dayRestriction = new PreferenceRestriction
            {
                //Add timezone compensation to make the test runnable on all machines(independent of local timezone)
                StartTimeLimitation =
                    new StartTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(20, 0, 0)),
                EndTimeLimitation =
                    new EndTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(20, 0, 0)),
                ShiftCategory = _shiftCategory,
                MustHave = true

            };

            return dayRestriction;
        }

        private IPersonAssignment CreatePersonAssignment()
        {
            IMainShift mainShift = new MainShift(_shiftCategory);
            IPersonAssignment assignment = new PersonAssignment(_person, new Scenario("Default"));
            assignment.SetMainShift(mainShift);

            return assignment;
        }


        private IList<IVisualLayer> CreateLayerCollection()
        {
            IList<IVisualLayer> layerCollection = new List<IVisualLayer>();
            layerCollection.Add(_layerFactory.CreateShiftSetupLayer(_activity, _dateTimePeriod));

            return layerCollection;
        }

        [Test]
        public void VerifyCanCalculateIllegalShiftCategoryPreferencePermissionState()
        {
            IActivity activity = new Activity("Activity");
            IList<IVisualLayer> layerCollection = new List<IVisualLayer>();
            layerCollection.Add(_layerFactory.CreateShiftSetupLayer(activity, _dateTimePeriod));

            IShiftCategory wrongCategory = new ShiftCategory("WrongShiftCat");
            IShiftCategory shiftCategory = new ShiftCategory("TopCat");
            IMainShift mainShift = new MainShift(shiftCategory);
            IPersonAssignment assignment = new PersonAssignment(_person, new Scenario("Default"));
            assignment.SetMainShift(mainShift);

            PreferenceRestriction dayRestriction = new PreferenceRestriction
            {
                //Add timezone compensation to make the test runnable on all machines(independent of local timezone)
                StartTimeLimitation =
                    new StartTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(20, 0, 0)),
                EndTimeLimitation =
                new EndTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(20, 0, 0)),
                ShiftCategory = wrongCategory
            };

            IPreferenceDay personRestriction = new PreferenceDay(_person, new DateOnly(_dateTime), dayRestriction);
            IList<IPersistableScheduleData> list = new List<IPersistableScheduleData> { personRestriction };
            ReadOnlyCollection<IPersistableScheduleData> dayRestrictions = new ReadOnlyCollection<IPersistableScheduleData>(list);
			var filteredVisualLayers = _mockRepository.StrictMock<IFilteredVisualLayerCollection>();

            using (_mockRepository.Record())
            {
                Expect.Call(_schedulePartMock.PersonAssignmentCollection()).Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> { assignment })).Repeat.Any();
                Expect.Call(_schedulePartMock.PersistableScheduleDataCollection()).Return(dayRestrictions).Repeat.AtLeastOnce();
                Expect.Call(_schedulePartMock.ProjectionService()).Return(_projectionService);
				Expect.Call(_schedulePartMock.SignificantPart()).Return(SchedulePartView.None);
				Expect.Call((_schedulePartMock.IsScheduled())).Return(false);
				Expect.Call(_schedulePartMock.Person).Return(_person).Repeat.Once();

                Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);

                Expect.Call(_visualLayerCollection.HasLayers).Return(true).Repeat.Any();
                Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
                Expect.Call(_visualLayerCollection.FilterLayers(new Activity("T"))).Return(filteredVisualLayers).IgnoreArguments();
                Expect.Call(_schedulePartMock.PersonDayOffCollection()).Return(
                    new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff>()));
                Expect.Call(_visualLayerCollection.ContractTime()).Return(
                    _dateTimePeriod.EndDateTime.Subtract(_dateTimePeriod.StartDateTime));
            	Expect.Call(filteredVisualLayers.GetEnumerator()).Return(layerCollection.GetEnumerator());
            }

            using (_mockRepository.Playback())
            {
                _target = new RestrictionChecker(_schedulePartMock);
                Assert.AreEqual(PermissionState.Broken, _target.CheckPreference());
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyCanCalculateIllegalShiftCategoryWithNoMainShiftPreferencePermissionState()
        {
            IActivity activity = new Activity("Activity");
            IList<IVisualLayer> layerCollection = new List<IVisualLayer>();
            layerCollection.Add(_layerFactory.CreateShiftSetupLayer(activity, _dateTimePeriod));

            IShiftCategory shiftCategory = new ShiftCategory("ShiftCat");
            IPersonAssignment assignment = new PersonAssignment(_person, new Scenario("Default"));

            PreferenceRestriction dayRestriction = new PreferenceRestriction
            {
                //Add timezone compensation to make the test runnable on all machines(independent of local timezone)
                StartTimeLimitation =
                    new StartTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(20, 0, 0)),
                EndTimeLimitation =
                new EndTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(20, 0, 0)),
                ShiftCategory = shiftCategory

            };

            IPreferenceDay personRestriction = new PreferenceDay(_person, new DateOnly(_dateTime), dayRestriction);
            IList<IPersistableScheduleData> list = new List<IPersistableScheduleData> { personRestriction };
            ReadOnlyCollection<IPersistableScheduleData> dayRestrictions = new ReadOnlyCollection<IPersistableScheduleData>(list);
			var filteredVisualLayers = _mockRepository.StrictMock<IFilteredVisualLayerCollection>();

            using (_mockRepository.Record())
            {
                Expect.Call(_schedulePartMock.PersonAssignmentCollection()).Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> { assignment })).Repeat.Any();
                Expect.Call(_schedulePartMock.PersistableScheduleDataCollection()).Return(dayRestrictions).Repeat.AtLeastOnce();
                Expect.Call(_schedulePartMock.ProjectionService()).Return(_projectionService);
                Expect.Call(_schedulePartMock.Person).Return(_person).Repeat.Once();
				Expect.Call(_schedulePartMock.SignificantPart()).Return(SchedulePartView.None);
				Expect.Call((_schedulePartMock.IsScheduled())).Return(false);

                Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
                Expect.Call(_visualLayerCollection.HasLayers).Return(true).Repeat.Any();
                Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
                Expect.Call(_visualLayerCollection.FilterLayers(new Activity("T"))).Return(filteredVisualLayers).IgnoreArguments();
                Expect.Call(_schedulePartMock.PersonDayOffCollection()).Return(
                    new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff>()));
                Expect.Call(_visualLayerCollection.ContractTime()).Return(
                    _dateTimePeriod.EndDateTime.Subtract(_dateTimePeriod.StartDateTime));
            	Expect.Call(filteredVisualLayers.GetEnumerator()).Return(layerCollection.GetEnumerator());
            }

            using (_mockRepository.Playback())
            {
                _target = new RestrictionChecker(_schedulePartMock);
                Assert.AreEqual(PermissionState.Broken, _target.CheckPreference());
            }
        }

        [Test]
        public void VerifyCanCalculateLegalActivityPreferencePermissionState()
        {
            IActivity activity = new Activity("Activity");
            IList<IVisualLayer> layerCollection = new List<IVisualLayer>();
            layerCollection.Add(_layerFactory.CreateShiftSetupLayer(activity, _dateTimePeriod));
            
            IShiftCategory shiftCategory = new ShiftCategory("TopCat");
            IMainShift mainShift = new MainShift(shiftCategory);
            IPersonAssignment assignment = new PersonAssignment(_person, new Scenario("Default"));
            assignment.SetMainShift(mainShift);


            PreferenceRestriction dayRestriction = new PreferenceRestriction
            {
                //Add timezone compensation to make the test runnable on all machines(independent of local timezone)
                StartTimeLimitation =
                    new StartTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(20, 0, 0)),
                EndTimeLimitation =
                new EndTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(20, 0, 0)), 
                ShiftCategory = shiftCategory
            };
            dayRestriction.AddActivityRestriction(new ActivityRestriction(activity));
            IPreferenceDay personRestriction = new PreferenceDay(_person, new DateOnly(_dateTime), dayRestriction);
            IList<IPersistableScheduleData> list = new List<IPersistableScheduleData> { personRestriction };
            ReadOnlyCollection<IPersistableScheduleData> dayRestrictions = new ReadOnlyCollection<IPersistableScheduleData>(list);
			var filteredVisualLayers = _mockRepository.StrictMock<IFilteredVisualLayerCollection>();

            using (_mockRepository.Record())
            {
                Expect.Call(_schedulePartMock.PersonAssignmentCollection()).Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> { assignment }));
                Expect.Call(_schedulePartMock.PersistableScheduleDataCollection()).Return(dayRestrictions).Repeat.AtLeastOnce();
                Expect.Call(_schedulePartMock.ProjectionService()).Return(_projectionService);
				Expect.Call(_schedulePartMock.SignificantPart()).Return(SchedulePartView.None);
				Expect.Call((_schedulePartMock.IsScheduled())).Return(false);

                Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
                Expect.Call(_schedulePartMock.Person).Return(_person).Repeat.Once();
                Expect.Call(_visualLayerCollection.HasLayers).Return(true).Repeat.Any();
                Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
                Expect.Call(_visualLayerCollection.FilterLayers(new Activity("T"))).Return(filteredVisualLayers).IgnoreArguments();
                Expect.Call(_schedulePartMock.PersonDayOffCollection()).Return(
                    new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff>()));
                Expect.Call(_visualLayerCollection.ContractTime()).Return(
                    _dateTimePeriod.EndDateTime.Subtract(_dateTimePeriod.StartDateTime));
				Expect.Call(filteredVisualLayers.GetEnumerator()).Return(layerCollection.GetEnumerator());
            }

            using (_mockRepository.Playback())
            {
                _target = new RestrictionChecker(_schedulePartMock);
                Assert.AreEqual(PermissionState.Satisfied, _target.CheckPreference());
            }
        }

        [Test]
        public void VerifyCanCalculateIllegalActivityPreferencePermissionState()
        {
            IActivity activity = new Activity("Activity");
            IList<IVisualLayer> layerCollection = new List<IVisualLayer>();
            layerCollection.Add(_layerFactory.CreateShiftSetupLayer(activity, _dateTimePeriod));

            IShiftCategory shiftCategory = new ShiftCategory("TopCat");
            IMainShift mainShift = new MainShift(shiftCategory);
            IPersonAssignment assignment = new PersonAssignment(_person, new Scenario("Default"));
            assignment.SetMainShift(mainShift);


            IPreferenceRestriction dayRestriction = new PreferenceRestriction
            {
                //Add timezone compensation to make the test runnable on all machines(independent of local timezone)
                StartTimeLimitation =
                    new StartTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(20, 0, 0)),
                EndTimeLimitation =
                new EndTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(20, 0, 0)),
                ShiftCategory = shiftCategory

            };
            dayRestriction.AddActivityRestriction(new ActivityRestriction(new Activity("WrongActivity")));

            IPreferenceDay personRestriction = new PreferenceDay(_person, new DateOnly(_dateTime), dayRestriction);
            IList<IPersistableScheduleData> list = new List<IPersistableScheduleData> { personRestriction };
            ReadOnlyCollection<IPersistableScheduleData> dayRestrictions = new ReadOnlyCollection<IPersistableScheduleData>(list);
			var filteredVisualLayers = _mockRepository.StrictMock<IFilteredVisualLayerCollection>();

            using (_mockRepository.Record())
            {
                Expect.Call(_schedulePartMock.PersonAssignmentCollection()).Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> { assignment }));
                Expect.Call(_schedulePartMock.PersistableScheduleDataCollection()).Return(dayRestrictions).Repeat.AtLeastOnce();
                Expect.Call(_schedulePartMock.ProjectionService()).Return(_projectionService);
                Expect.Call(_schedulePartMock.Person).Return(_person).Repeat.Once();
				Expect.Call(_schedulePartMock.SignificantPart()).Return(SchedulePartView.None);
				Expect.Call((_schedulePartMock.IsScheduled())).Return(false);

                Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
                Expect.Call(_visualLayerCollection.HasLayers).Return(true).Repeat.Any();
                Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
                Expect.Call(_visualLayerCollection.FilterLayers(new Activity("T"))).Return(filteredVisualLayers).IgnoreArguments();
                Expect.Call(_schedulePartMock.PersonDayOffCollection()).Return(
                    new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff>())).Repeat.AtLeastOnce();
                Expect.Call(_visualLayerCollection.ContractTime()).Return(
                    _dateTimePeriod.EndDateTime.Subtract(_dateTimePeriod.StartDateTime));
            	Expect.Call(filteredVisualLayers.GetEnumerator()).Return(layerCollection.GetEnumerator());
            }

            using (_mockRepository.Playback())
            {
                _target = new RestrictionChecker(_schedulePartMock);
                Assert.AreEqual(PermissionState.Broken, _target.CheckPreference());
            }
        }

        [Test]
        public void VerifyCanCalculateLegalDayOffPreferencePermissionState()
        {
            IDayOffTemplate dayOffTemplate = new DayOffTemplate(new Description("DayOffTemplate"));
            dayOffTemplate.Anchor = TimeSpan.FromHours(10);
            dayOffTemplate.SetTargetAndFlexibility(TimeSpan.FromHours(4), TimeSpan.FromHours(1));

            IActivity activity = new Activity("Activity");
            IList<IVisualLayer> layerCollection = new List<IVisualLayer>();
            layerCollection.Add(_layerFactory.CreateShiftSetupLayer(activity, _dateTimePeriod));

            IShiftCategory shiftCategory = new ShiftCategory("TopCat");
            IMainShift mainShift = new MainShift(shiftCategory);
            IPersonAssignment assignment = new PersonAssignment(_person, new Scenario("Default"));
            assignment.SetMainShift(mainShift);


            PreferenceRestriction dayRestriction = new PreferenceRestriction
            {
                //Add timezone compensation to make the test runnable on all machines(independent of local timezone)
                StartTimeLimitation =
                    new StartTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(20, 0, 0)),
                EndTimeLimitation =
                new EndTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(20, 0, 0)),
                ShiftCategory = shiftCategory, DayOffTemplate = dayOffTemplate

            };
            dayRestriction.AddActivityRestriction(new ActivityRestriction(activity));

            IPreferenceDay personRestriction = new PreferenceDay(_person, new DateOnly(_dateTime), dayRestriction);
            IList<IPersistableScheduleData> list = new List<IPersistableScheduleData> { personRestriction };
            ReadOnlyCollection<IPersistableScheduleData> dayRestrictions = new ReadOnlyCollection<IPersistableScheduleData>(list);
			var filteredVisualLayers = _mockRepository.StrictMock<IFilteredVisualLayerCollection>();

            using (_mockRepository.Record())
            {
                Expect.Call(_schedulePartMock.PersonAssignmentCollection()).Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> { assignment }));
                Expect.Call(_schedulePartMock.PersistableScheduleDataCollection()).Return(dayRestrictions).Repeat.AtLeastOnce();
                Expect.Call(_schedulePartMock.ProjectionService()).Return(_projectionService);
                Expect.Call(_schedulePartMock.Person).Return(_person).Repeat.Once();
				Expect.Call(_schedulePartMock.SignificantPart()).Return(SchedulePartView.None);
				Expect.Call((_schedulePartMock.IsScheduled())).Return(false);

                Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
                Expect.Call(_visualLayerCollection.HasLayers).Return(true).Repeat.Any();
                Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
                Expect.Call(_visualLayerCollection.FilterLayers(new Activity("T"))).Return(filteredVisualLayers).IgnoreArguments();
                Expect.Call(_visualLayerCollection.ContractTime()).Return(
					_dateTimePeriod.EndDateTime.Subtract(_dateTimePeriod.StartDateTime));
				Expect.Call(filteredVisualLayers.GetEnumerator()).Return(layerCollection.GetEnumerator());
            }

            using (_mockRepository.Playback())
            {
                _target = new RestrictionChecker(_schedulePartMock);
                Assert.AreEqual(PermissionState.Satisfied, _target.CheckPreference());
            }
        }


		[Test]
		public void ShouldGetUnspecifiedIfPreferenceIsDayOffAndNotScheduled()
		{

			_target = new RestrictionChecker(_schedulePartMock);
			IDayOffTemplate dayOffTemplate = new DayOffTemplate(new Description("DayOffTemplate"));
			PreferenceRestriction dayRestriction = new PreferenceRestriction
			{
				DayOffTemplate = dayOffTemplate
			};
			IPreferenceDay personRestriction = new PreferenceDay(_person, new DateOnly(_dateTime), dayRestriction);
			ReadOnlyCollection<IPersistableScheduleData> scheduleData = new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData> { personRestriction });


			using (_mockRepository.Record())
			{
				Expect.Call(_schedulePartMock.PersistableScheduleDataCollection()).Return(scheduleData).Repeat.AtLeastOnce();
				Expect.Call(_schedulePartMock.SignificantPart()).Return(SchedulePartView.None);
				Expect.Call((_schedulePartMock.IsScheduled())).Return(false);
			}

			PermissionState result;

			using (_mockRepository.Playback())
			{
				result = _target.CheckPreferenceDayOff();
			}

			Assert.AreEqual(PermissionState.Unspecified, result);
		}


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyCanReturnBrokenWhenPreferenceIsShiftCategoryAndScheduledIsDayOff()
        {
            IDayOffTemplate dayOffTemplate = new DayOffTemplate(new Description("DayOffTemplate"));
            dayOffTemplate.Anchor = TimeSpan.FromHours(10);
            dayOffTemplate.SetTargetAndFlexibility(TimeSpan.FromHours(4), TimeSpan.FromHours(1));

            IPersonDayOff personDayOff = new PersonDayOff(_person,
                                   ScenarioFactory.CreateScenarioAggregate(),
                                   dayOffTemplate, new DateOnly(2007, 1, 1));


            IActivity activity = new Activity("Activity");
            IList<IVisualLayer> layerCollection = new List<IVisualLayer>();
            layerCollection.Add(_layerFactory.CreateShiftSetupLayer(activity, _dateTimePeriod));

            IShiftCategory shiftCategory = new ShiftCategory("TopCat");
            IMainShift mainShift = new MainShift(shiftCategory);
            IPersonAssignment assignment = new PersonAssignment(_person, new Scenario("Default"));
            assignment.SetMainShift(mainShift);


            PreferenceRestriction dayRestriction = new PreferenceRestriction
            {
                //Add timezone compensation to make the test runnable on all machines(independent of local timezone)
                StartTimeLimitation =
                    new StartTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(20, 0, 0)),
                EndTimeLimitation =
                new EndTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(20, 0, 0)),
                ShiftCategory = shiftCategory,
                DayOffTemplate = null
            };
            dayRestriction.AddActivityRestriction(new ActivityRestriction(activity));

            IPreferenceDay personRestriction = new PreferenceDay(_person, new DateOnly(_dateTime), dayRestriction);
            IList<IPersistableScheduleData> list = new List<IPersistableScheduleData> { personRestriction };
            ReadOnlyCollection<IPersistableScheduleData> dayRestrictions = new ReadOnlyCollection<IPersistableScheduleData>(list);

            using (_mockRepository.Record())
            {
                Expect.Call(_schedulePartMock.PersistableScheduleDataCollection()).Return(dayRestrictions).Repeat.AtLeastOnce();
				Expect.Call(_schedulePartMock.SignificantPart()).Return(SchedulePartView.None);
				Expect.Call((_schedulePartMock.IsScheduled())).Return(false);

                Expect.Call(_visualLayerCollection.HasLayers).Return(true).Repeat.Any();
                Expect.Call(_schedulePartMock.PersonDayOffCollection()).Return(
                    new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff> { personDayOff }));
            }

            using (_mockRepository.Playback())
            {
                _target = new RestrictionChecker(_schedulePartMock);
                Assert.AreEqual(PermissionState.Broken, _target.CheckPreference());
            }
        }

        [Test]
        public void VerifyCanCalculateIllegalDayOffPreferencePermissionState()
        {
            IDayOffTemplate dayOffTemplate = new DayOffTemplate(new Description("DayOffTemplate"));
            dayOffTemplate.Anchor = TimeSpan.FromHours(10);
            dayOffTemplate.SetTargetAndFlexibility(TimeSpan.FromHours(4), TimeSpan.FromHours(1));

            IPersonDayOff personDayOff = new PersonDayOff(_person,
                                   ScenarioFactory.CreateScenarioAggregate(),
                                   dayOffTemplate, new DateOnly(2007, 1, 1));


            IActivity activity = new Activity("Activity");
            IList<IVisualLayer> layerCollection = new List<IVisualLayer>();
            layerCollection.Add(_layerFactory.CreateShiftSetupLayer(activity, _dateTimePeriod));

            IShiftCategory shiftCategory = new ShiftCategory("TopCat");
            IMainShift mainShift = new MainShift(shiftCategory);
            IPersonAssignment assignment = new PersonAssignment(_person, new Scenario("Default"));
            assignment.SetMainShift(mainShift);


            PreferenceRestriction dayRestriction = new PreferenceRestriction
            {
                //Add timezone compensation to make the test runnable on all machines(independent of local timezone)
                StartTimeLimitation =
                    new StartTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(20, 0, 0)),
                EndTimeLimitation =
                new EndTimeLimitation(
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(20, 0, 0)),
                ShiftCategory = shiftCategory,
                DayOffTemplate = new DayOffTemplate(new Description("WrongDayOff"))

            };
            dayRestriction.AddActivityRestriction(new ActivityRestriction(activity));

            IPreferenceDay personRestriction = new PreferenceDay(_person, new DateOnly(_dateTime), dayRestriction);
            IList<IPersistableScheduleData> list = new List<IPersistableScheduleData> { personRestriction };
            ReadOnlyCollection<IPersistableScheduleData> dayRestrictions = new ReadOnlyCollection<IPersistableScheduleData>(list);

            using (_mockRepository.Record())
            {
                Expect.Call(_schedulePartMock.PersonAssignmentCollection()).Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> { assignment })).Repeat.Any();
                Expect.Call(_schedulePartMock.PersistableScheduleDataCollection()).Return(dayRestrictions).Repeat.AtLeastOnce();
				Expect.Call(_schedulePartMock.SignificantPart()).Return(SchedulePartView.None);
				Expect.Call((_schedulePartMock.IsScheduled())).Return(true);
				Expect.Call(_schedulePartMock.PersonDayOffCollection()).Return(
					new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff> { personDayOff }));

            	Expect.Call(_visualLayerCollection.HasLayers).Return(true).Repeat.Any();
			}

            using (_mockRepository.Playback())
            {
                _target = new RestrictionChecker(_schedulePartMock);
                Assert.AreEqual(PermissionState.Broken, _target.CheckPreference());
            }
        }

        [Test]
		[Ignore("What do WeakReference want to test here?")]
		public void VerifyCanCalculateNoDayOffOnShiftPreferencePermissionState()
		{
			IActivity activity = new Activity("Activity");
			IList<IVisualLayer> layerCollection = new List<IVisualLayer>();
			layerCollection.Add(_layerFactory.CreateShiftSetupLayer(activity, _dateTimePeriod));

			IShiftCategory shiftCategory = new ShiftCategory("TopCat");
			IMainShift mainShift = new MainShift(shiftCategory);
			IPersonAssignment assignment = new PersonAssignment(_person, new Scenario("Default"));
			assignment.SetMainShift(mainShift);


			PreferenceRestriction dayRestriction = new PreferenceRestriction
			{
				//Add timezone compensation to make the test runnable on all machines(independent of local timezone)
				StartTimeLimitation =
					new StartTimeLimitation(
					new TimeSpan(6, 0, 0),
					new TimeSpan(20, 0, 0)),
				EndTimeLimitation =
				new EndTimeLimitation(
					new TimeSpan(6, 0, 0),
					new TimeSpan(20, 0, 0)),
				ShiftCategory = shiftCategory,
				DayOffTemplate = new DayOffTemplate(new Description("WrongDayOff"))

			};
			dayRestriction.AddActivityRestriction(new ActivityRestriction(activity));
			IPreferenceDay personRestriction = new PreferenceDay(_person, new DateOnly(_dateTime), dayRestriction);
			IList<IPersistableScheduleData> list = new List<IPersistableScheduleData> { personRestriction };
			ReadOnlyCollection<IPersistableScheduleData> dayRestrictions = new ReadOnlyCollection<IPersistableScheduleData>(list);

			using (_mockRepository.Record())
			{
				Expect.Call(_schedulePartMock.PersonAssignmentCollection()).Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> { assignment })).Repeat.Any();
				Expect.Call(_schedulePartMock.PersistableScheduleDataCollection()).Return(dayRestrictions).Repeat.AtLeastOnce();
				Expect.Call(_visualLayerCollection.HasLayers).Return(true).Repeat.Any();

				Expect.Call(_schedulePartMock.PersonDayOffCollection()).Return(
					new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff>()));
			}

			using (_mockRepository.Playback())
			{
				_target = new RestrictionChecker(_schedulePartMock);
				Assert.AreEqual(PermissionState.Broken, _target.CheckPreference());
			}
		}


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyCanCalculateNoPreferencePermissionState()
        {
            IActivity activity = new Activity("Activity");
            IList<IVisualLayer> layerCollection = new List<IVisualLayer>();
            layerCollection.Add(_layerFactory.CreateShiftSetupLayer(activity, _dateTimePeriod));

            IShiftCategory shiftCategory = new ShiftCategory("TopCat");
            IMainShift mainShift = new MainShift(shiftCategory);
            IPersonAssignment assignment = new PersonAssignment(_person, new Scenario("Default"));
            assignment.SetMainShift(mainShift);

            ReadOnlyCollection<IPersistableScheduleData> dayRestrictions = new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData>());

            using (_mockRepository.Record())
            {
                Expect.Call(_schedulePartMock.PersonAssignmentCollection()).Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> { assignment })).Repeat.Any();
                Expect.Call(_schedulePartMock.PersistableScheduleDataCollection()).Return(dayRestrictions);
                Expect.Call(_visualLayerCollection.HasLayers).Return(true).Repeat.Any();
            }

            using (_mockRepository.Playback())
            {
                _target = new RestrictionChecker(_schedulePartMock);
                Assert.AreEqual(PermissionState.None, _target.CheckPreference());
            }
        }

        #endregion

        #region StudentAvailability

        [Test]
        public void VerifySatisfiedStudentAvailabilityWhenNotAvailable()
        {
            ICccTimeZoneInfo cccTimeZoneInfo = _person.PermissionInformation.DefaultTimeZone();
            IStudentAvailabilityRestriction restriction = new StudentAvailabilityRestriction();
            DateTime start = cccTimeZoneInfo.ConvertTimeFromUtc(_dateTimePeriod.StartDateTime, cccTimeZoneInfo);
            DateTime end = cccTimeZoneInfo.ConvertTimeFromUtc(_dateTimePeriod.EndDateTime, cccTimeZoneInfo);
            restriction.StartTimeLimitation = new StartTimeLimitation(start.TimeOfDay, null);
            restriction.EndTimeLimitation = new EndTimeLimitation(null, end.AddMinutes(-1).TimeOfDay);
            IStudentAvailabilityDay studentAvailabilityDay = new StudentAvailabilityDay(_person, new DateOnly(_dateTime), new List<IStudentAvailabilityRestriction> { restriction });
            studentAvailabilityDay.NotAvailable = true;
            ReadOnlyCollection<IPersistableScheduleData> dayRestrictions = new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData> { studentAvailabilityDay });
            IPersonDayOff dayOff = _mockRepository.StrictMock<IPersonDayOff>();
            using (_mockRepository.Record())
            {

                Expect.Call(_schedulePartMock.PersonDayOffCollection()).Return(new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff> { dayOff })).Repeat.Any();
                 Expect.Call(_schedulePartMock.PersistableScheduleDataCollection()).Return(dayRestrictions);

                Expect.Call(_schedulePartMock.Person).Return(_person).Repeat.Any();
                Expect.Call(_schedulePartMock.Period).Return(_dateTimePeriod).Repeat.Any();

            }
            using (_mockRepository.Playback())
            {
                _target = new RestrictionChecker(_schedulePartMock);
                Assert.AreEqual(PermissionState.Satisfied, _target.CheckStudentAvailability());
            }
        }

        [Test]
        public void VerifyBrokenStudentAvailabilityWhenNotAvailable()
        {
            IActivity activity = new Activity("Activity");
            IList<IVisualLayer> layerCollection = new List<IVisualLayer>();
            layerCollection.Add(_layerFactory.CreateShiftSetupLayer(activity, _dateTimePeriod));

            IShiftCategory shiftCategory = new ShiftCategory("TopCat");
            IMainShift mainShift = new MainShift(shiftCategory);
            IPersonAssignment assignment = new PersonAssignment(_person, new Scenario("Default"));
            assignment.SetMainShift(mainShift);
            ICccTimeZoneInfo cccTimeZoneInfo = _person.PermissionInformation.DefaultTimeZone();
            IStudentAvailabilityRestriction restriction = new StudentAvailabilityRestriction();
            DateTime start = cccTimeZoneInfo.ConvertTimeFromUtc(_dateTimePeriod.StartDateTime, cccTimeZoneInfo);
            DateTime end = cccTimeZoneInfo.ConvertTimeFromUtc(_dateTimePeriod.EndDateTime, cccTimeZoneInfo);
            restriction.StartTimeLimitation = new StartTimeLimitation(start.TimeOfDay, null);
            restriction.EndTimeLimitation = new EndTimeLimitation(null, end.TimeOfDay);
            IStudentAvailabilityDay studentAvailabilityDay = new StudentAvailabilityDay(_person, new DateOnly(_dateTime), new List<IStudentAvailabilityRestriction> { restriction });
            studentAvailabilityDay.NotAvailable = true;
            ReadOnlyCollection<IPersistableScheduleData> dayRestrictions = new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData> { studentAvailabilityDay });

            using (_mockRepository.Record())
            {

                Expect.Call(_schedulePartMock.PersonDayOffCollection()).Return(new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff>())).Repeat.Any();
                Expect.Call(_schedulePartMock.PersonAssignmentCollection()).Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> { assignment })).Repeat.Any();
                Expect.Call(_schedulePartMock.PersistableScheduleDataCollection()).Return(dayRestrictions);

                Expect.Call(_schedulePartMock.Person).Return(_person).Repeat.Any();
                Expect.Call(_schedulePartMock.Period).Return(_dateTimePeriod).Repeat.Any();

                Expect.Call(_schedulePartMock.ProjectionService()).Return(_projectionService).Repeat.Any();
                Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection).Repeat.Any();
                Expect.Call(_visualLayerCollection.HasLayers).Return(true).Repeat.Any();
                
            }
            using (_mockRepository.Playback())
            {
                _target = new RestrictionChecker(_schedulePartMock);
                Assert.AreEqual(PermissionState.Broken, _target.CheckStudentAvailability());
            }
        }

        [Test]
        public void VerifyBrokenStudentAvailability()
        {
            IActivity activity = new Activity("Activity");
            IList<IVisualLayer> layerCollection = new List<IVisualLayer>();
            layerCollection.Add(_layerFactory.CreateShiftSetupLayer(activity, _dateTimePeriod));

            IShiftCategory shiftCategory = new ShiftCategory("TopCat");
            IMainShift mainShift = new MainShift(shiftCategory);
            IPersonAssignment assignment = new PersonAssignment(_person, new Scenario("Default"));
            assignment.SetMainShift(mainShift);
            ICccTimeZoneInfo cccTimeZoneInfo = _person.PermissionInformation.DefaultTimeZone();
            IStudentAvailabilityRestriction restriction = new StudentAvailabilityRestriction();
            DateTime start = cccTimeZoneInfo.ConvertTimeFromUtc(_dateTimePeriod.StartDateTime, cccTimeZoneInfo);
            DateTime end = cccTimeZoneInfo.ConvertTimeFromUtc(_dateTimePeriod.EndDateTime, cccTimeZoneInfo);
            restriction.StartTimeLimitation = new StartTimeLimitation(start.TimeOfDay,null);
            restriction.EndTimeLimitation = new EndTimeLimitation(null,end.AddMinutes(-1).TimeOfDay);
            IStudentAvailabilityDay studentAvailabilityDay = new StudentAvailabilityDay(_person,new DateOnly(_dateTime) ,new List<IStudentAvailabilityRestriction>{restriction});

            ReadOnlyCollection<IPersistableScheduleData> dayRestrictions = new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData> { studentAvailabilityDay });

            using (_mockRepository.Record())
            {
                
                Expect.Call(_schedulePartMock.PersonDayOffCollection()).Return(new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff>())).Repeat.Any();
                Expect.Call(_schedulePartMock.PersonAssignmentCollection()).Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> { assignment })).Repeat.Any();
                Expect.Call(_schedulePartMock.PersistableScheduleDataCollection()).Return(dayRestrictions);

                Expect.Call(_schedulePartMock.Person).Return(_person).Repeat.Any();
                Expect.Call(_schedulePartMock.Period).Return(_dateTimePeriod).Repeat.Any();

                Expect.Call(_schedulePartMock.ProjectionService()).Return(_projectionService).Repeat.Any();
                Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection).Repeat.Any();
                Expect.Call(_visualLayerCollection.HasLayers).Return(true).Repeat.Any();
                Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod).Repeat.Any();
                Expect.Call(_visualLayerCollection.ContractTime()).Return(
                    _dateTimePeriod.EndDateTime.Subtract(_dateTimePeriod.StartDateTime)).Repeat.Any();
            }
            using (_mockRepository.Playback())
            {
                _target = new RestrictionChecker(_schedulePartMock);
                Assert.AreEqual(PermissionState.Broken, _target.CheckStudentAvailability());
            }
        }

        [Test]
        public void VerifyBrokenSatisfiedAvailability()
        {
            IActivity activity = new Activity("Activity");
            IList<IVisualLayer> layerCollection = new List<IVisualLayer>();
            layerCollection.Add(_layerFactory.CreateShiftSetupLayer(activity, _dateTimePeriod));

            IShiftCategory shiftCategory = new ShiftCategory("TopCat");
            IMainShift mainShift = new MainShift(shiftCategory);
            IPersonAssignment assignment = new PersonAssignment(_person, new Scenario("Default"));
            assignment.SetMainShift(mainShift);
            ICccTimeZoneInfo cccTimeZoneInfo = _person.PermissionInformation.DefaultTimeZone();
            IStudentAvailabilityRestriction restriction = new StudentAvailabilityRestriction();
            DateTime start = cccTimeZoneInfo.ConvertTimeFromUtc(_dateTimePeriod.StartDateTime, cccTimeZoneInfo);
            DateTime end = cccTimeZoneInfo.ConvertTimeFromUtc(_dateTimePeriod.EndDateTime, cccTimeZoneInfo);
            restriction.StartTimeLimitation = new StartTimeLimitation(start.TimeOfDay, null);
            restriction.EndTimeLimitation = new EndTimeLimitation(null, end.TimeOfDay);
            IStudentAvailabilityDay studentAvailabilityDay = new StudentAvailabilityDay(_person, new DateOnly(_dateTime), new List<IStudentAvailabilityRestriction> { restriction });

            ReadOnlyCollection<IPersistableScheduleData> dayRestrictions = new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData> { studentAvailabilityDay });

            using (_mockRepository.Record())
            {

                Expect.Call(_schedulePartMock.PersonDayOffCollection()).Return(new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff>())).Repeat.Any();
                Expect.Call(_schedulePartMock.PersonAssignmentCollection()).Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> { assignment })).Repeat.Any();
                Expect.Call(_schedulePartMock.PersistableScheduleDataCollection()).Return(dayRestrictions);

                Expect.Call(_schedulePartMock.Person).Return(_person).Repeat.Any();
                Expect.Call(_schedulePartMock.Period).Return(_dateTimePeriod).Repeat.Any();

                Expect.Call(_schedulePartMock.ProjectionService()).Return(_projectionService).Repeat.Any();
                Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection).Repeat.Any();
                Expect.Call(_visualLayerCollection.HasLayers).Return(true).Repeat.Any();
                Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod).Repeat.Any();
                Expect.Call(_visualLayerCollection.ContractTime()).Return(
                    _dateTimePeriod.EndDateTime.Subtract(_dateTimePeriod.StartDateTime)).Repeat.Any();
            }
            using (_mockRepository.Playback())
            {
                _target = new RestrictionChecker(_schedulePartMock);
                Assert.AreEqual(PermissionState.Satisfied, _target.CheckStudentAvailability());
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyUnspecifiedAvailability()
        {
            IList<IVisualLayer> layerCollection = new List<IVisualLayer>();
            IVisualLayerCollection visualLayerCollection = new VisualLayerCollection(_person, layerCollection, new ProjectionPayloadMerger());
            ICccTimeZoneInfo cccTimeZoneInfo = _person.PermissionInformation.DefaultTimeZone();
            IStudentAvailabilityRestriction restriction = new StudentAvailabilityRestriction();
            DateTime start = cccTimeZoneInfo.ConvertTimeFromUtc(_dateTimePeriod.StartDateTime, cccTimeZoneInfo);
            DateTime end = cccTimeZoneInfo.ConvertTimeFromUtc(_dateTimePeriod.EndDateTime, cccTimeZoneInfo);
            restriction.StartTimeLimitation = new StartTimeLimitation(start.TimeOfDay, null);
            restriction.EndTimeLimitation = new EndTimeLimitation(null, end.TimeOfDay);
            IStudentAvailabilityDay studentAvailabilityDay = new StudentAvailabilityDay(_person, new DateOnly(_dateTime), new List<IStudentAvailabilityRestriction> { restriction });

            ReadOnlyCollection<IPersistableScheduleData> dayRestrictions = new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData> { studentAvailabilityDay });

            using (_mockRepository.Record())
            {
                Expect.Call(_schedulePartMock.PersonDayOffCollection()).Return(new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff>())).Repeat.Any();
                Expect.Call(_schedulePartMock.PersonAssignmentCollection()).Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> {  })).Repeat.Any();
                Expect.Call(_schedulePartMock.PersistableScheduleDataCollection()).Return(dayRestrictions);

                Expect.Call(_schedulePartMock.Person).Return(_person).Repeat.Any();
                Expect.Call(_schedulePartMock.Period).Return(_dateTimePeriod).Repeat.Any();

                Expect.Call(_schedulePartMock.ProjectionService()).Return(_projectionService).Repeat.Any();
                Expect.Call(_projectionService.CreateProjection()).Return(visualLayerCollection).Repeat.Any();
                Expect.Call(_visualLayerCollection.HasLayers).Return(true).Repeat.Any();
                Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod).Repeat.Any();
                Expect.Call(_visualLayerCollection.ContractTime()).Return(
                    _dateTimePeriod.EndDateTime.Subtract(_dateTimePeriod.StartDateTime)).Repeat.Any();
            }
            using (_mockRepository.Playback())
            {
                _target = new RestrictionChecker(_schedulePartMock);
                Assert.AreEqual(PermissionState.Unspecified, _target.CheckStudentAvailability());
            }
        }

        #endregion

        private void RecordMock(IEnumerable<IRestrictionBase> baseRestrictions, IEnumerable<IPersistableScheduleData> dayRestrictions)
        {
            using (_mockRepository.Record())
            {
                Expect.Call(_schedulePartMock.Person).Return(_person).Repeat.Any();
                Expect.Call(_schedulePartMock.Period).Return(_dateTimePeriod).Repeat.Any();
                Expect.Call(_schedulePartMock.PersonDayOffCollection()).Return(new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff>())).Repeat.Any();
                Expect.Call(_schedulePartMock.PersonAssignmentCollection()).Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment>())).Repeat.Any();
                Expect.Call(_parameters.Person).Return(_person).Repeat.Any();
                if (dayRestrictions != null)
                    Expect.Call(_schedulePartMock.PersistableScheduleDataCollection()).Return(dayRestrictions).Repeat.Any();
                Expect.Call(_schedulePartMock.RestrictionCollection()).Return(baseRestrictions).Repeat.Any();

                Expect.Call(_schedulePartMock.ProjectionService()).Return(_projectionService).Repeat.Any();
                Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection).Repeat.Any();
                Expect.Call(_visualLayerCollection.HasLayers).Return(true).Repeat.Any();
                Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod).Repeat.Any();
                Expect.Call(_visualLayerCollection.ContractTime()).Return(
                    _dateTimePeriod.EndDateTime.Subtract(_dateTimePeriod.StartDateTime)).Repeat.Any();
            }
        }
    }
}
