using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class PersonAssignmentAssemblerTest
    {
        private PersonAssignmentAssembler target;
        private IShiftCategoryRepository shiftCatRep;
        private IActivityRepository actRep;
        private MockRepository mocks;
        private IPerson person;
        private IScenario scenario;
        private IMultiplicatorDefinitionSetRepository definitionSetRep;
        private IContract contract;
        private DateTimePeriodAssembler dateTimePeriodAssembler;
        private ActivityAssembler activityAssembler;
        private IActivityLayerAssembler<IMainShiftLayer> mainShiftLayerAssembler;
        private IActivityLayerAssembler<IPersonalShiftActivityLayer> personalShiftLayerAssembler;
        private IOvertimeLayerAssembler overtimeShiftLayerAssembler;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            SetupRepositories();
            SetupAssemblers();
            target = new PersonAssignmentAssembler(shiftCatRep, mainShiftLayerAssembler,personalShiftLayerAssembler,overtimeShiftLayerAssembler);
            contract = ContractFactory.CreateContract("test");
            person = PersonFactory.CreatePerson();
            person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(new DateOnly(1800, 1, 1),
                                                                          PersonContractFactory.CreatePersonContract(
                                                                              contract,
                                                                              PartTimePercentageFactory.
                                                                                  CreatePartTimePercentage("test"),
                                                                              ContractScheduleFactory.
                                                                                  CreateContractSchedule("test")),
                                                                          TeamFactory.CreateSimpleTeam()));
            scenario = ScenarioFactory.CreateScenarioAggregate();
            target.Person = person;
            target.DefaultScenario = scenario;
        }

        private void SetupAssemblers()
        {
            activityAssembler = new ActivityAssembler(actRep);
            dateTimePeriodAssembler = new DateTimePeriodAssembler();
            mainShiftLayerAssembler =
                new ActivityLayerAssembler<IMainShiftLayer>(new MainShiftLayerConstructor(),
                                                                    dateTimePeriodAssembler,activityAssembler);
            personalShiftLayerAssembler = new ActivityLayerAssembler<IPersonalShiftActivityLayer>(new PersonalShiftLayerConstructor(),
                                                                                                  dateTimePeriodAssembler, activityAssembler);
            overtimeShiftLayerAssembler = new OvertimeLayerAssembler(dateTimePeriodAssembler, activityAssembler, definitionSetRep);
        }

        private void SetupRepositories()
        {
            shiftCatRep = mocks.StrictMock<IShiftCategoryRepository>();
            actRep = mocks.StrictMock<IActivityRepository>();
            definitionSetRep = mocks.StrictMock<IMultiplicatorDefinitionSetRepository>();
        }

        [Test]
        public void VerifyDtoToDo()
        {
            PersonAssignmentDto dto = new PersonAssignmentDto {Id=Guid.NewGuid(), Version = 4};
            dto.MainShift = new MainShiftDto {Id = Guid.NewGuid(), ShiftCategoryId = Guid.NewGuid()};
            ActivityDto actMain = new ActivityDto {Id = Guid.NewGuid()};
            ActivityDto actPers = new ActivityDto {Id = Guid.NewGuid()};
            ActivityDto actOvertime = new ActivityDto {Id = Guid.NewGuid()};
            OvertimeDefinitionSetDto overtimeDefinitionSetDto = new OvertimeDefinitionSetDto {Id = Guid.NewGuid()};

            ActivityLayerDto mainShiftLayer = new ActivityLayerDto
                                                  {
                                                      Period =
                                                          dateTimePeriodAssembler.DomainEntityToDto(
                                                              new DateTimePeriod(1900, 1, 1, 1900, 1, 2)),
                                                      Activity = actMain
                                                  };
            dto.MainShift.LayerCollection.Add(mainShiftLayer);

            ActivityLayerDto persShiftLayer = new ActivityLayerDto
                                                  {
                                                      Period =
                                                          dateTimePeriodAssembler.DomainEntityToDto(
                                                              new DateTimePeriod(2000, 1, 1, 2000, 1, 2)),
                                                      Activity = actPers
                                                  };
            var personalShift = new ShiftDto {Id = Guid.NewGuid()};
            personalShift.LayerCollection.Add(persShiftLayer);
            dto.PersonalShiftCollection.Add(personalShift);

            OvertimeLayerDto overtimeLayerDto =
                new OvertimeLayerDto { Period = dateTimePeriodAssembler.DomainEntityToDto(new DateTimePeriod(2001, 1, 1, 2001, 1, 2)), Activity = actOvertime, OvertimeDefinitionSetId = overtimeDefinitionSetDto.Id.Value };
            var overtimeShift = new ShiftDto { Id = Guid.NewGuid() };
            overtimeShift.LayerCollection.Add(overtimeLayerDto);
            dto.OvertimeShiftCollection.Add(overtimeShift);

            IShiftCategory shiftCategory = ShiftCategoryFactory.CreateShiftCategory("d1");
            IActivity activityMain = ActivityFactory.CreateActivity("sd");
            IActivity activityPers = ActivityFactory.CreateActivity("sd");
            IActivity activityOvertime = ActivityFactory.CreateActivity("sd");
            IMultiplicatorDefinitionSet definitionSet = mocks.StrictMock<IMultiplicatorDefinitionSet>();
            contract.AddMultiplicatorDefinitionSetCollection(definitionSet);

            using(mocks.Record())
            {
                Expect.Call(shiftCatRep.Load(dto.MainShift.ShiftCategoryId)).Return(shiftCategory);
                Expect.Call(actRep.Get(actMain.Id.Value)).Return(activityMain);
                Expect.Call(actRep.Get(actPers.Id.Value)).Return(activityPers);
                Expect.Call(actRep.Get(actOvertime.Id.Value)).Return(activityOvertime);
                Expect.Call(definitionSetRep.Load(overtimeDefinitionSetDto.Id.Value)).Return(definitionSet);
            }
            using(mocks.Playback())
            {
                IPersonAssignment entity = target.DtoToDomainEntity(dto);

                Assert.AreEqual(dto.Version, entity.Version);
                Assert.AreEqual(dto.Id, entity.Id);
                Assert.AreEqual(1, entity.MainShiftLayers.Count());
                Assert.AreEqual(1, entity.PersonalShiftCollection.Count);
                Assert.AreEqual(dto.PersonalShiftCollection.First().Id, entity.PersonalShiftCollection[0].Id);
                Assert.AreEqual(1, entity.PersonalShiftCollection[0].LayerCollection.Count);
                Assert.AreEqual(1, entity.OvertimeShiftCollection.Count);
                Assert.AreEqual(dto.OvertimeShiftCollection.First().Id, entity.OvertimeShiftCollection[0].Id);
                Assert.AreEqual(1, entity.OvertimeShiftCollection[0].LayerCollection.Count);
                Assert.AreEqual(new DateTimePeriod(1900, 1, 1, 1900, 1, 2), entity.MainShiftLayers.First().Period);
                Assert.AreEqual(new DateTimePeriod(2000, 1, 1, 2000, 1, 2), entity.PersonalShiftCollection[0].LayerCollection[0].Period);
                Assert.AreEqual(new DateTimePeriod(2001, 1, 1, 2001, 1, 2), entity.OvertimeShiftCollection[0].LayerCollection[0].Period);
                Assert.AreSame(person, entity.Person);
                Assert.AreSame(scenario, entity.Scenario);             
                Assert.AreSame(shiftCategory, entity.ShiftCategory);
                Assert.AreSame(activityMain, entity.MainShiftLayers.First().Payload);
                Assert.AreSame(activityPers, entity.PersonalShiftCollection[0].LayerCollection[0].Payload);
                Assert.AreSame(activityOvertime, entity.OvertimeShiftCollection[0].LayerCollection[0].Payload);
                Assert.AreSame(definitionSet, ((IOvertimeShiftActivityLayer)entity.OvertimeShiftCollection[0].LayerCollection[0]).DefinitionSet);
            }
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CannotExecuteFromDtoConverterWithoutPerson()
        {
            target.Person = null;
            target.DtoToDomainEntity(new PersonAssignmentDto());
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CannotExecuteFromDtoConverterWithoutDefaultScenario()
        {
            target.DefaultScenario = null;
            target.DtoToDomainEntity(new PersonAssignmentDto());
        }

        [Test]
        public void VerifyDoToDto()
        {
            IActivity act = ActivityFactory.CreateActivity("asdf");
            act.SetId(Guid.NewGuid());
            IMultiplicatorDefinitionSet definitionSet =
                MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("ovt", MultiplicatorType.Overtime);
            definitionSet.SetId(Guid.NewGuid());
            IShiftCategory sCat = ShiftCategoryFactory.CreateShiftCategory("d1");
            sCat.SetId(Guid.NewGuid());
            var mainShift = EditableShiftFactory.CreateEditorShift(act,new DateTimePeriod(1900, 1, 1, 1900, 1, 2), sCat);
            IPersonalShift pShift = PersonalShiftFactory.CreatePersonalShift(act,new DateTimePeriod(1800, 1, 1, 1800, 1, 2));
            pShift.SetId(Guid.NewGuid());
            ((IPersonalShiftActivityLayer)pShift.LayerCollection[0]).SetId(Guid.NewGuid());
            IPersonAssignment ass = PersonAssignmentFactory.CreatePersonAssignment(person, scenario);
            ass.SetId(Guid.NewGuid());
            IOvertimeShift oShift = OvertimeShiftFactory.CreateOvertimeShift(act, new DateTimePeriod(1803, 1, 1, 1803, 1, 2), definitionSet,ass);
            oShift.SetId(Guid.NewGuid());
            ((IOvertimeShiftActivityLayer)oShift.LayerCollection[0]).SetId(Guid.NewGuid());
            new EditableShiftMapper().SetMainShiftLayers(ass, mainShift);
            ass.AddPersonalShift(pShift);
            ass.AddOvertimeShift(oShift);

            PersonAssignmentDto dto = target.DomainEntityToDto(ass);
            ActivityLayerDto firstMainShiftLayer = dto.MainShift.LayerCollection.First();

            Assert.AreEqual(0, dto.Version);
            Assert.AreEqual(ass.Id, dto.Id);
            Assert.AreEqual(ass.ShiftCategory.Id, dto.MainShift.ShiftCategoryId);
            Assert.AreEqual(ass.MainShiftLayers.Count(), dto.MainShift.LayerCollection.Count);
            Assert.AreEqual(ass.MainShiftLayers.First().Payload.Id, firstMainShiftLayer.Activity.Id);
            Assert.AreEqual(ass.MainShiftLayers.First().Period,	new DateTimePeriod(firstMainShiftLayer.Period.UtcStartTime, firstMainShiftLayer.Period.UtcEndTime));
            Assert.AreEqual(ass.PersonalShiftCollection[0].Id, dto.PersonalShiftCollection.First().Id);
            Assert.AreEqual(ass.PersonalShiftCollection.Count, dto.PersonalShiftCollection.Count);
            Assert.AreEqual(ass.OvertimeShiftCollection[0].Id, dto.OvertimeShiftCollection.First().Id);
            Assert.AreEqual(ass.OvertimeShiftCollection.Count, dto.OvertimeShiftCollection.Count);
        }
    }
}