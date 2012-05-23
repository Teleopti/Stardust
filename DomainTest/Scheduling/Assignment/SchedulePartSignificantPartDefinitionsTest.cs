using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.AgentInfo;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
    [TestFixture]
    public class SchedulePartSignificantPartDefinitionsTest
    {
        private IScheduleDay _part;
        private DateTime _baseDateTime;
        private MockRepository _mocker;
        private IScenario _scenario;
        private IPerson _person;
        private IScheduleDictionary _scheduleDictionary;
        private IScheduleDay _mockedPart;
    	private IHasDayOffDefinition _hasDayOffDefinition;

        [SetUp]
        public void Setup()
        {
            _baseDateTime = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            _person = new Person();
            _scenario = new Scenario("for test");
            _mocker = new MockRepository();
            _scheduleDictionary = new ScheduleDictionary(_scenario, new ScheduleDateTimePeriod(new DateTimePeriod(2000,1,1,2002,1,1)));
            _part = ExtractedSchedule.CreateScheduleDay(_scheduleDictionary, _person, new DateOnly(2001,1,1));
            _mocker.BackToRecordAll();
            _mockedPart = _mocker.StrictMock<IScheduleDay>();
        	_hasDayOffDefinition = _mocker.StrictMock<IHasDayOffDefinition>();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VerifyPartIsNotNull()
        {
            _part = null;
			ISignificantPartProvider provider = new SchedulePartSignificantPartDefinitions(_part, _hasDayOffDefinition);
            Assert.IsNull(provider,"We will not touch this, but fxcop will");
        }

        [Test]
        public void VerifyHasDayOffDefinition()
        {
            //Definition: PersonDayOffCollection > 0
            ICccTimeZoneInfo timeZoneInfo = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            IList<IPersonDayOff> pDayOffs = new List<IPersonDayOff>
                                                {
                                                    new PersonDayOff(_person, _scenario, new DayOff(),
                                                                     new DateOnly(2001, 1, 1), timeZoneInfo)
                                                };

            using (_mocker.Record())
            {
                Expect.Call(_mockedPart.PersonDayOffCollection()).Return(new ReadOnlyCollection<IPersonDayOff>(pDayOffs));
                Expect.Call(_mockedPart.PersonDayOffCollection()).Return(new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff>()));
                
            }

            using (_mocker.Playback())
            {
                Assert.IsTrue(new SchedulePartSignificantPartDefinitions(_mockedPart, _hasDayOffDefinition).HasDayOff());
				Assert.IsFalse(new SchedulePartSignificantPartDefinitions(_mockedPart, _hasDayOffDefinition).HasDayOff(), "empty");
            }

        }

        [Test]
        public void VerifyHasFullAbsenceDefinition()
        {
            //Definition: main layer(s) is covered by one or more absence layers
            IShiftCategory category = new ShiftCategory("shiftCategory");
            var mainLayer1 = new MainShiftActivityLayer(new Activity("main1"), new DateTimePeriod(_baseDateTime.AddHours(4), _baseDateTime.AddHours(6)));
            var mainLayer2 = new MainShiftActivityLayer(new Activity("main2"), new DateTimePeriod(_baseDateTime.AddHours(6), _baseDateTime.AddHours(8)));
            var absenceLayer1 = new AbsenceLayer(new Absence(), new DateTimePeriod(_baseDateTime.AddHours(4), _baseDateTime.AddHours(5)));
            var absenceLayer3 = new AbsenceLayer(new Absence(), new DateTimePeriod(_baseDateTime, _baseDateTime.AddHours(24)));

            //only mainshifts
            _part.CreateAndAddActivity(mainLayer1, category);
            _part.CreateAndAddActivity(mainLayer2, category);
			Assert.IsFalse(new SchedulePartSignificantPartDefinitions(_part, _hasDayOffDefinition).HasFullAbsence());

            //one absence not covering whole mainshifts
            _part.CreateAndAddAbsence(absenceLayer1);
			Assert.IsFalse(new SchedulePartSignificantPartDefinitions(_part, _hasDayOffDefinition).HasFullAbsence());

            //rk: anders said no to this one
            ////two absences covering whole mainshifts together
            //_part.CreateAndAddAbsence(absenceLayer2);
            //Assert.IsTrue(new SchedulePartSignificantPartDefinitions(_part).HasFullAbsence());

            //one absence covering whole mainshifts
            _part.Clear<IPersonAbsence>();
            _part.CreateAndAddAbsence(absenceLayer3);
			Assert.IsTrue(new SchedulePartSignificantPartDefinitions(_part, _hasDayOffDefinition).HasFullAbsence());

            //only absence
            IContract contract = new Contract("contract");
            IContractSchedule contractSchedule = new ContractSchedule("contractSchedule");
            IPersonContract personContract = new PersonContract(contract,new PartTimePercentage("partTimePercentage"), contractSchedule);
            IPersonPeriod personPeriod = new PersonPeriod(new DateOnly(new DateTime(2001, 1, 1)), personContract, new Team());
            _part.Person.AddPersonPeriod(personPeriod);
            _part.Clear<IPersonAssignment>();
			Assert.IsTrue(new SchedulePartSignificantPartDefinitions(_part, _hasDayOffDefinition).HasFullAbsence());
        }

        [Test]
        public void ShouldOnlyCreateOneProjectionForHasFullAbsenceDefinitionBetweenBeginAndEnd()
        {
            IProjectionService projectionService = _mocker.StrictMock<IProjectionService>();
            IVisualLayerCollection visualLayerCollection = _mocker.StrictMock<IVisualLayerCollection>();
            IVisualLayer visualLayer = _mocker.StrictMock<IVisualLayer>();
            IAbsence absence = _mocker.DynamicMock<IAbsence>();
            using (_mocker.Record())
            {
                Expect.Call(_mockedPart.ProjectionService()).Return(projectionService);
                Expect.Call(projectionService.CreateProjection()).Return(visualLayerCollection);
                Expect.Call(visualLayerCollection.GetEnumerator()).Return(new List<IVisualLayer> {visualLayer}.GetEnumerator()).Repeat.AtLeastOnce();
                Expect.Call(visualLayer.Payload).Return(absence).Repeat.AtLeastOnce();
                Expect.Call(visualLayerCollection.HasLayers).Return(true).Repeat.AtLeastOnce();
            }
            using (_mocker.Playback())
            {
				var definitions = new SchedulePartSignificantPartDefinitions(_mockedPart, _hasDayOffDefinition);
                using(definitions.BeginRead())
                {
                    Assert.IsTrue(definitions.HasFullAbsence());
                    Assert.IsTrue(definitions.HasFullAbsence());
                }
            }
        }

        [Test]
        public void VerifyHasAbsenceDefinition()
        {
            //Definition: Has One layer in projection, and that layer is an absencelayer
            IShiftCategory category = new ShiftCategory("shiftCategory");
            var layer = new MainShiftActivityLayer(new Activity("underlying"), new DateTimePeriod(_baseDateTime.AddHours(4), _baseDateTime.AddHours(6)));
            var layer2 = new MainShiftActivityLayer(new Activity("underlying2"), new DateTimePeriod(_baseDateTime.AddHours(5), _baseDateTime.AddHours(8)));
            var absenceLayer = new AbsenceLayer(new Absence(), new DateTimePeriod(_baseDateTime.AddHours(4), _baseDateTime.AddHours(6)));
            var absenceLayer2 = new AbsenceLayer(new Absence(), new DateTimePeriod(_baseDateTime.AddHours(5), _baseDateTime.AddHours(8)));
			var target = new SchedulePartSignificantPartDefinitions(_part, _hasDayOffDefinition);

            _part.CreateAndAddActivity(layer, category);
            Assert.IsFalse(target.HasAbsence(), "no absence");

            _part.CreateAndAddAbsence(absenceLayer);
            Assert.IsTrue(target.HasAbsence(), "one absence, nothing else");

            _part.CreateAndAddActivity(layer2, category);
            _part.CreateAndAddAbsence(absenceLayer2);
            Assert.IsTrue(target.HasAbsence(), "two different absencelayers");

            _part.Clear<IPersonAbsence>();
            var pAbs = new PersonAbsence(_part.Person, _part.Scenario,
                                         new AbsenceLayer(new Absence(),
                                                          new DateTimePeriod(_baseDateTime.AddHours(26),
                                                                             _baseDateTime.AddHours(30))));
            _part.Add(pAbs);
            Assert.IsFalse(target.HasAbsence(), "Absence next day should result no hit if no projection");
        }

        [Test]
        public void VerifyHasAbsenceNightshift()
        {
            IShiftCategory category = new ShiftCategory("shiftCategory");
            var layer = new MainShiftActivityLayer(new Activity("underlying"), new DateTimePeriod(_baseDateTime.AddHours(21), _baseDateTime.AddHours(31)));
            var pAbs = new PersonAbsence(_part.Person, _part.Scenario,
                             new AbsenceLayer(new Absence(),
                                              new DateTimePeriod(_baseDateTime.AddHours(26),
                                                                 _baseDateTime.AddHours(30))));
			var target = new SchedulePartSignificantPartDefinitions(_part, _hasDayOffDefinition);
            _part.CreateAndAddActivity(layer, category);
            _part.Add(pAbs);

            Assert.IsTrue(target.HasAbsence());
        }

        [Test]
        public void VerifyHasMainShiftDefinition()
        {
            //Definition: Assignment with "HighestZOrder" has Mainshift
            IPersonAssignment personAssignmentWithMainShift = new PersonAssignment(_person, _scenario);
            IPersonAssignment personAssignmentWithoutMainShift = new PersonAssignment(_person, _scenario);
            personAssignmentWithMainShift.SetMainShift(new MainShift(new ShiftCategory("mainShiftcategory")));

            using (_mocker.Record())
            {
                Expect.Call(_mockedPart.AssignmentHighZOrder()).Return(personAssignmentWithMainShift);
                Expect.Call(_mockedPart.AssignmentHighZOrder()).Return(personAssignmentWithoutMainShift);
                Expect.Call(_mockedPart.AssignmentHighZOrder()).Return(null);
            }

            using (_mocker.Playback())
            {
				Assert.IsTrue(new SchedulePartSignificantPartDefinitions(_mockedPart, _hasDayOffDefinition).HasMainShift());
				Assert.IsFalse(new SchedulePartSignificantPartDefinitions(_mockedPart, _hasDayOffDefinition).HasMainShift());
				Assert.IsFalse(new SchedulePartSignificantPartDefinitions(_mockedPart, _hasDayOffDefinition).HasMainShift());
            }
        }


        [Test]
        public void VerifyHasAssignmentDefinition()
        {
            //Definition: PersonAssignmentCollection() > 0
            IList<IPersonAssignment> personAssignments = new List<IPersonAssignment> { new PersonAssignment(_person, _scenario) };

            using (_mocker.Record())
            {
                Expect.Call(_mockedPart.PersonAssignmentCollection()).Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment>()));
                Expect.Call(_mockedPart.PersonAssignmentCollection()).Return(new ReadOnlyCollection<IPersonAssignment>(personAssignments));
            }

            using (_mocker.Playback())
            {
				Assert.IsFalse(new SchedulePartSignificantPartDefinitions(_mockedPart, _hasDayOffDefinition).HasAssignment());
				Assert.IsTrue(new SchedulePartSignificantPartDefinitions(_mockedPart, _hasDayOffDefinition).HasAssignment());
            }
        }

        [Test]
        public void VerifyHasPersonalShiftDefinition()
        {
            //Definition: Assignment with "HighestZOrder" has PersonalShift
            IPersonAssignment personAssignmentWithPersonalShift = new PersonAssignment(_person, _scenario);
            IPersonAssignment personAssignmentWithoutPersonalShift = new PersonAssignment(_person, _scenario);
            personAssignmentWithPersonalShift.InsertPersonalShift(new PersonalShift(), personAssignmentWithPersonalShift.PersonalShiftCollection.Count);

            using (_mocker.Record())
            {
                Expect.Call(_mockedPart.AssignmentHighZOrder()).Return(personAssignmentWithPersonalShift);
                Expect.Call(_mockedPart.AssignmentHighZOrder()).Return(personAssignmentWithoutPersonalShift);
                Expect.Call(_mockedPart.AssignmentHighZOrder()).Return(null);
            }

            using (_mocker.Playback())
            {
				Assert.IsTrue(new SchedulePartSignificantPartDefinitions(_mockedPart, _hasDayOffDefinition).HasPersonalShift());
				Assert.IsFalse(new SchedulePartSignificantPartDefinitions(_mockedPart, _hasDayOffDefinition).HasPersonalShift());
				Assert.IsFalse(new SchedulePartSignificantPartDefinitions(_mockedPart, _hasDayOffDefinition).HasPersonalShift());
            }
        }

        [Test]
        public void VerifyHasOvertimeDefinition()
        {
            //Definition: Assignment with HighesZorder has OvertimeShift
            IPersonAssignment personAssignmentWithOvertimeShift = new PersonAssignment(_person, _scenario);
            IPersonAssignment personAssignmentWithoutOvertimeShift = new PersonAssignment(_person, _scenario);
            personAssignmentWithOvertimeShift.AddOvertimeShift(new OvertimeShift());

            using (_mocker.Record())
            {
                Expect.Call(_mockedPart.AssignmentHighZOrder()).Return(personAssignmentWithOvertimeShift);
                Expect.Call(_mockedPart.AssignmentHighZOrder()).Return(personAssignmentWithoutOvertimeShift);
                Expect.Call(_mockedPart.AssignmentHighZOrder()).Return(null);
            }

            using (_mocker.Playback())
            {
				Assert.IsTrue(new SchedulePartSignificantPartDefinitions(_mockedPart, _hasDayOffDefinition).HasOvertimeShift());
				Assert.IsFalse(new SchedulePartSignificantPartDefinitions(_mockedPart, _hasDayOffDefinition).HasOvertimeShift());
				Assert.IsFalse(new SchedulePartSignificantPartDefinitions(_mockedPart, _hasDayOffDefinition).HasOvertimeShift());
            }
        }

        [Test]
        public void VerifyHasPreferenceDefinition()
        {
            IList<IRestrictionBase> restrictions = new List<IRestrictionBase>();

            using(_mocker.Record())
            {
                Expect.Call(_mockedPart.RestrictionCollection()).Return(new ReadOnlyCollection<IRestrictionBase>(restrictions)).Repeat.Twice();
            }

            using (_mocker.Playback())
            {
				Assert.IsFalse(new SchedulePartSignificantPartDefinitions(_mockedPart, _hasDayOffDefinition).HasPreferenceRestriction());
                restrictions.Add(new PreferenceRestriction());
				Assert.IsTrue(new SchedulePartSignificantPartDefinitions(_mockedPart, _hasDayOffDefinition).HasPreferenceRestriction());
            }

        }

        [Test]
        public void VerifyHasStudentAvailabilityDefinition()
        {
            IList<IRestrictionBase> restrictions = new List<IRestrictionBase>();

            using (_mocker.Record())
            {
                Expect.Call(_mockedPart.RestrictionCollection()).Return(new ReadOnlyCollection<IRestrictionBase>(restrictions)).Repeat.Twice();
            }

            using (_mocker.Playback())
            {
				Assert.IsFalse(new SchedulePartSignificantPartDefinitions(_mockedPart, _hasDayOffDefinition).HasStudentAvailabilityRestriction());
                restrictions.Add(new StudentAvailabilityRestriction());
				Assert.IsTrue(new SchedulePartSignificantPartDefinitions(_mockedPart, _hasDayOffDefinition).HasStudentAvailabilityRestriction());
            }
        }

		[Test]
		public void ShouldNotCalculateAsContractDayOffWhenFullDayAbsenceAndMainShift()
		{
			IPersonAssignment personAssignmentWithMainShift = new PersonAssignment(_person, _scenario);
			IPersonAssignment personAssignmentWithoutMainShift = new PersonAssignment(_person, _scenario);
			personAssignmentWithMainShift.SetMainShift(new MainShift(new ShiftCategory("mainShiftcategory")));
			IProjectionService projectionService = _mocker.StrictMock<IProjectionService>();
			IVisualLayerCollection visualLayerCollection = _mocker.StrictMock<IVisualLayerCollection>();
			IVisualLayer visualLayer = _mocker.StrictMock<IVisualLayer>();
			IAbsence absence = _mocker.DynamicMock<IAbsence>();

			using (_mocker.Record())
			{
				Expect.Call(_mockedPart.AssignmentHighZOrder()).Return(personAssignmentWithMainShift);
				Expect.Call(_mockedPart.AssignmentHighZOrder()).Return(personAssignmentWithoutMainShift);
				Expect.Call(_mockedPart.ProjectionService()).Return(projectionService).Repeat.AtLeastOnce();
				Expect.Call(projectionService.CreateProjection()).Return(visualLayerCollection).Repeat.AtLeastOnce();
				Expect.Call(visualLayerCollection.GetEnumerator()).Return(new List<IVisualLayer> { visualLayer }.GetEnumerator()).Repeat.AtLeastOnce();
				Expect.Call(visualLayer.Payload).Return(absence).Repeat.AtLeastOnce();
				Expect.Call(visualLayerCollection.HasLayers).Return(true).Repeat.AtLeastOnce();
				Expect.Call(_hasDayOffDefinition.IsDayOff()).Return(true);
				Expect.Call(_mockedPart.PersonDayOffCollection()).Return(
					new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff>()));
			}

			using (_mocker.Playback())
			{
				Assert.IsFalse(new SchedulePartSignificantPartDefinitions(_mockedPart, _hasDayOffDefinition).HasContractDayOff());
				Assert.IsTrue(new SchedulePartSignificantPartDefinitions(_mockedPart, _hasDayOffDefinition).HasContractDayOff());
			}
		}
    }

}
