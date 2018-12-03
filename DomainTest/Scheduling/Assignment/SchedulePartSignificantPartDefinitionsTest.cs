using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;

using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	[DomainTest]
	public class SchedulePartSignificantPartDefinitionsTest
    {
        private IScheduleDay _part;
        private DateTime _baseDateTime;
        private MockRepository _mocker;
        private IScenario _scenario;
        private IPerson _person;
        private IScheduleDictionary _scheduleDictionary;
        private IScheduleDay _mockedPart;
    	private IHasContractDayOffDefinition _hasContractDayOffDefinition;

        [SetUp]
        public void Setup()
        {
            _baseDateTime = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            _person = new Person().InTimeZone(TimeZoneInfo.Utc);
            _scenario = new Scenario("for test");
            _mocker = new MockRepository();
            _scheduleDictionary = new ScheduleDictionary(_scenario, new ScheduleDateTimePeriod(new DateTimePeriod(2000,1,1,2002,1,1)), new PersistableScheduleDataPermissionChecker(new FullPermission()), new FullPermission());
            _part = ExtractedSchedule.CreateScheduleDay(_scheduleDictionary, _person, new DateOnly(2001,1,1), new FullPermission());
            _mocker.BackToRecordAll();
            _mockedPart = _mocker.StrictMock<IScheduleDay>();
        	_hasContractDayOffDefinition = _mocker.StrictMock<IHasContractDayOffDefinition>();
        }

        [Test]
        public void VerifyPartIsNotNull()
        {
            _part = null;
			Assert.Throws<ArgumentNullException>(() =>
			{
				ISignificantPartProvider provider = new SchedulePartSignificantPartDefinitions(_part, _hasContractDayOffDefinition);
				Assert.IsNull(provider, "We will not touch this, but fxcop will");

			});
        }

        [Test]
        public void VerifyHasDayOffDefinition()
        {
            using (_mocker.Record())
            {
                Expect.Call(_mockedPart.HasDayOff()).Return(true);
                Expect.Call(_mockedPart.HasDayOff()).Return(false);
                
            }

            using (_mocker.Playback())
            {
                Assert.IsTrue(new SchedulePartSignificantPartDefinitions(_mockedPart, _hasContractDayOffDefinition).HasDayOff());
				Assert.IsFalse(new SchedulePartSignificantPartDefinitions(_mockedPart, _hasContractDayOffDefinition).HasDayOff(), "empty");
            }

        }

        [Test]
        public void VerifyHasFullAbsenceDefinition()
        {
            //Definition: main layer(s) is covered by one or more absence layers
            IShiftCategory category = new ShiftCategory("shiftCategory");
            var absenceLayer1 = new AbsenceLayer(new Absence(), new DateTimePeriod(_baseDateTime.AddHours(4), _baseDateTime.AddHours(5)));
            var absenceLayer3 = new AbsenceLayer(new Absence(), new DateTimePeriod(_baseDateTime, _baseDateTime.AddHours(24)));

            //only mainshifts
            _part.CreateAndAddActivity(new Activity("main1"), new DateTimePeriod(_baseDateTime.AddHours(4), _baseDateTime.AddHours(6)), category);
						_part.CreateAndAddActivity(new Activity("main2"), new DateTimePeriod(_baseDateTime.AddHours(6), _baseDateTime.AddHours(8)), category);
			Assert.IsFalse(new SchedulePartSignificantPartDefinitions(_part, _hasContractDayOffDefinition).HasFullAbsence());

            //one absence not covering whole mainshifts
            _part.CreateAndAddAbsence(absenceLayer1);
			Assert.IsFalse(new SchedulePartSignificantPartDefinitions(_part, _hasContractDayOffDefinition).HasFullAbsence());

            //rk: anders said no to this one
            ////two absences covering whole mainshifts together
            //_part.CreateAndAddAbsence(absenceLayer2);
            //Assert.IsTrue(new SchedulePartSignificantPartDefinitions(_part).HasFullAbsence());

            //one absence covering whole mainshifts
            _part.Clear<IPersonAbsence>();
            _part.CreateAndAddAbsence(absenceLayer3);
			Assert.IsTrue(new SchedulePartSignificantPartDefinitions(_part, _hasContractDayOffDefinition).HasFullAbsence());

            //only absence
            IContract contract = new Contract("contract");
            IContractSchedule contractSchedule = new ContractSchedule("contractSchedule");
            IPersonContract personContract = new PersonContract(contract,new PartTimePercentage("partTimePercentage"), contractSchedule);
            IPersonPeriod personPeriod = new PersonPeriod(new DateOnly(new DateTime(2001, 1, 1)), personContract, new Team());
            _part.Person.AddPersonPeriod(personPeriod);
            _part.Clear<IPersonAssignment>();
			Assert.IsTrue(new SchedulePartSignificantPartDefinitions(_part, _hasContractDayOffDefinition).HasFullAbsence());
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
				var definitions = new SchedulePartSignificantPartDefinitions(_mockedPart, _hasContractDayOffDefinition);
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
            var absenceLayer = new AbsenceLayer(new Absence(), new DateTimePeriod(_baseDateTime.AddHours(4), _baseDateTime.AddHours(6)));
            var absenceLayer2 = new AbsenceLayer(new Absence(), new DateTimePeriod(_baseDateTime.AddHours(5), _baseDateTime.AddHours(8)));
			var target = new SchedulePartSignificantPartDefinitions(_part, _hasContractDayOffDefinition);

			_part.CreateAndAddActivity(new Activity("underlying"), new DateTimePeriod(_baseDateTime.AddHours(4), _baseDateTime.AddHours(6)), category);
            Assert.IsFalse(target.HasAbsence(), "no absence");

            _part.CreateAndAddAbsence(absenceLayer);
            Assert.IsTrue(target.HasAbsence(), "one absence, nothing else");

						_part.CreateAndAddActivity(new Activity("underlying2"), new DateTimePeriod(_baseDateTime.AddHours(5), _baseDateTime.AddHours(8)), category);
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
            var pAbs = new PersonAbsence(_part.Person, _part.Scenario,
                             new AbsenceLayer(new Absence(),
                                              new DateTimePeriod(_baseDateTime.AddHours(26),
                                                                 _baseDateTime.AddHours(30))));
			var target = new SchedulePartSignificantPartDefinitions(_part, _hasContractDayOffDefinition);
			_part.CreateAndAddActivity(new Activity("underlying"), new DateTimePeriod(_baseDateTime.AddHours(21), _baseDateTime.AddHours(31)), category);
            _part.Add(pAbs);

            Assert.IsTrue(target.HasAbsence());
        }

        [Test]
        public void VerifyHasMainShiftDefinition()
        {
            //Definition: Assignment with "HighestZOrder" has Mainshift
			IPersonAssignment personAssignmentWithMainShift = new PersonAssignment(_person, _scenario, new DateOnly(2000, 1, 1));
			IPersonAssignment personAssignmentWithoutMainShift = new PersonAssignment(_person, _scenario, new DateOnly(2000, 1, 1));
	        var mainShift = EditableShiftFactory.CreateEditorShift(new Activity("hej"), new DateTimePeriod(2000, 1, 1, 8, 2000, 1, 1 ,9),
	                                                      new ShiftCategory("hej"));
			new EditableShiftMapper().SetMainShiftLayers(personAssignmentWithMainShift, mainShift);

            using (_mocker.Record())
            {
                Expect.Call(_mockedPart.PersonAssignment()).Return(personAssignmentWithMainShift);
                Expect.Call(_mockedPart.PersonAssignment()).Return(personAssignmentWithoutMainShift);
                Expect.Call(_mockedPart.PersonAssignment()).Return(null);
            }

            using (_mocker.Playback())
            {
				Assert.IsTrue(new SchedulePartSignificantPartDefinitions(_mockedPart, _hasContractDayOffDefinition).HasMainShift());
				Assert.IsFalse(new SchedulePartSignificantPartDefinitions(_mockedPart, _hasContractDayOffDefinition).HasMainShift());
				Assert.IsFalse(new SchedulePartSignificantPartDefinitions(_mockedPart, _hasContractDayOffDefinition).HasMainShift());
            }
        }


        [Test]
        public void VerifyHasAssignmentDefinition()
        {
            using (_mocker.Record())
            {
                Expect.Call(_mockedPart.PersonAssignment()).Return(null);
								Expect.Call(_mockedPart.PersonAssignment()).Return(new PersonAssignment(_person, _scenario, new DateOnly(2000, 1, 1)));
            }

            using (_mocker.Playback())
            {
				Assert.IsFalse(new SchedulePartSignificantPartDefinitions(_mockedPart, _hasContractDayOffDefinition).HasAssignment());
				Assert.IsTrue(new SchedulePartSignificantPartDefinitions(_mockedPart, _hasContractDayOffDefinition).HasAssignment());
            }
        }

        [Test]
        public void VerifyHasPersonalShiftDefinition()
        {
            //Definition: Assignment with "HighestZOrder" has PersonalShift
					IPersonAssignment personAssignmentWithPersonalShift = new PersonAssignment(_person, _scenario, new DateOnly(2000, 1, 1));
					IPersonAssignment personAssignmentWithoutPersonalShift = new PersonAssignment(_person, _scenario, new DateOnly(2000, 1, 1));
			personAssignmentWithPersonalShift.AddPersonalActivity(new Activity("hej"), new DateTimePeriod(2000, 1, 1, 8, 2000, 1, 1, 9));

            using (_mocker.Record())
            {
                Expect.Call(_mockedPart.PersonAssignment()).Return(personAssignmentWithPersonalShift);
                Expect.Call(_mockedPart.PersonAssignment()).Return(personAssignmentWithoutPersonalShift);
                Expect.Call(_mockedPart.PersonAssignment()).Return(null);
            }

            using (_mocker.Playback())
            {
				Assert.IsTrue(new SchedulePartSignificantPartDefinitions(_mockedPart, _hasContractDayOffDefinition).HasPersonalShift());
				Assert.IsFalse(new SchedulePartSignificantPartDefinitions(_mockedPart, _hasContractDayOffDefinition).HasPersonalShift());
				Assert.IsFalse(new SchedulePartSignificantPartDefinitions(_mockedPart, _hasContractDayOffDefinition).HasPersonalShift());
            }
        }

        [Test]
        public void VerifyHasOvertimeDefinition()
        {
            //Definition: Assignment with HighesZorder has OvertimeShift
					IPersonAssignment personAssignmentWithOvertimeShift = new PersonAssignment(_person, _scenario, new DateOnly(2000, 1, 1));
					IPersonAssignment personAssignmentWithoutOvertimeShift = new PersonAssignment(_person, _scenario, new DateOnly(2000, 1, 1));
            personAssignmentWithOvertimeShift.AddOvertimeActivity(new Activity("sdf"), new DateTimePeriod(2000,1,1,2000,1,2), MockRepository.GenerateMock<IMultiplicatorDefinitionSet>());

            using (_mocker.Record())
            {
                Expect.Call(_mockedPart.PersonAssignment()).Return(personAssignmentWithOvertimeShift);
                Expect.Call(_mockedPart.PersonAssignment()).Return(personAssignmentWithoutOvertimeShift);
                Expect.Call(_mockedPart.PersonAssignment()).Return(null);
            }

            using (_mocker.Playback())
            {
				Assert.IsTrue(new SchedulePartSignificantPartDefinitions(_mockedPart, _hasContractDayOffDefinition).HasOvertimeShift());
				Assert.IsFalse(new SchedulePartSignificantPartDefinitions(_mockedPart, _hasContractDayOffDefinition).HasOvertimeShift());
				Assert.IsFalse(new SchedulePartSignificantPartDefinitions(_mockedPart, _hasContractDayOffDefinition).HasOvertimeShift());
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
				Assert.IsFalse(new SchedulePartSignificantPartDefinitions(_mockedPart, _hasContractDayOffDefinition).HasPreferenceRestriction());
                restrictions.Add(new PreferenceRestriction());
				Assert.IsTrue(new SchedulePartSignificantPartDefinitions(_mockedPart, _hasContractDayOffDefinition).HasPreferenceRestriction());
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
				Assert.IsFalse(new SchedulePartSignificantPartDefinitions(_mockedPart, _hasContractDayOffDefinition).HasStudentAvailabilityRestriction());
                restrictions.Add(new StudentAvailabilityRestriction());
				Assert.IsTrue(new SchedulePartSignificantPartDefinitions(_mockedPart, _hasContractDayOffDefinition).HasStudentAvailabilityRestriction());
            }
        }

		[Test]
		public void ShouldNotCalculateAsContractDayOffWhenFullDayAbsenceAndMainShift()
		{
			IPersonAssignment personAssignmentWithMainShift = new PersonAssignment(_person, _scenario, new DateOnly(2000, 1, 1));
			IPersonAssignment personAssignmentWithoutMainShift = new PersonAssignment(_person, _scenario, new DateOnly(2000, 1, 1));
			var mainShift = EditableShiftFactory.CreateEditorShift(new Activity("hej"), new DateTimePeriod(2000, 1, 1, 8, 2000, 1, 1, 9),
			                                                 new ShiftCategory("mainShiftcategory"));
			new EditableShiftMapper().SetMainShiftLayers(personAssignmentWithMainShift, mainShift);
			IProjectionService projectionService = _mocker.StrictMock<IProjectionService>();
			IVisualLayerCollection visualLayerCollection = _mocker.StrictMock<IVisualLayerCollection>();
			IVisualLayer visualLayer = _mocker.StrictMock<IVisualLayer>();
			IAbsence absence = _mocker.DynamicMock<IAbsence>();

			using (_mocker.Record())
			{
				Expect.Call(_mockedPart.PersonAssignment()).Return(personAssignmentWithMainShift);
				Expect.Call(_mockedPart.PersonAssignment()).Return(personAssignmentWithoutMainShift);
				Expect.Call(_mockedPart.ProjectionService()).Return(projectionService).Repeat.AtLeastOnce();
				Expect.Call(projectionService.CreateProjection()).Return(visualLayerCollection).Repeat.AtLeastOnce();
				Expect.Call(visualLayerCollection.GetEnumerator()).Return(new List<IVisualLayer> { visualLayer }.GetEnumerator()).Repeat.AtLeastOnce();
				Expect.Call(visualLayer.Payload).Return(absence).Repeat.AtLeastOnce();
				Expect.Call(visualLayerCollection.HasLayers).Return(true).Repeat.AtLeastOnce();
				Expect.Call(_hasContractDayOffDefinition.IsDayOff(_mockedPart)).Return(true);
				Expect.Call(_mockedPart.HasDayOff()).Return(false);
			}

			using (_mocker.Playback())
			{
				Assert.IsFalse(new SchedulePartSignificantPartDefinitions(_mockedPart, _hasContractDayOffDefinition).HasContractDayOff());
				Assert.IsTrue(new SchedulePartSignificantPartDefinitions(_mockedPart, _hasContractDayOffDefinition).HasContractDayOff());
			}
		}
    }

}
