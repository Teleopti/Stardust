using System;
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
        private IActivityLayerAssembler<IPersonalShiftLayer> personalShiftLayerAssembler;
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
            personalShiftLayerAssembler = new ActivityLayerAssembler<IPersonalShiftLayer>(new PersonalShiftLayerConstructor(),
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
                Assert.AreEqual(new DateTimePeriod(1900, 1, 1, 1900, 1, 2), entity.MainActivities().Single().Period);
                Assert.AreEqual(new DateTimePeriod(2000, 1, 1, 2000, 1, 2), entity.PersonalActivities().Single().Period);
                Assert.AreEqual(new DateTimePeriod(2001, 1, 1, 2001, 1, 2), entity.OvertimeActivities().Single().Period);
                Assert.AreSame(person, entity.Person);
                Assert.AreSame(scenario, entity.Scenario);             
                Assert.AreSame(shiftCategory, entity.ShiftCategory);
                Assert.AreSame(activityMain, entity.MainActivities().First().Payload);
                Assert.AreSame(activityPers, entity.PersonalActivities().First().Payload);
                Assert.AreSame(activityOvertime, entity.OvertimeActivities().First().Payload);
                Assert.AreSame(definitionSet, entity.OvertimeActivities().First().DefinitionSet);
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
           
            IPersonAssignment ass = PersonAssignmentFactory.CreatePersonAssignment(person, scenario);
            ass.SetId(Guid.NewGuid());
					ass.AddOvertimeActivity(act, new DateTimePeriod(1803, 1, 1, 1803, 1, 2), definitionSet);
            ass.OvertimeActivities().First().SetId(Guid.NewGuid());
						ass.AddActivity(act, new DateTimePeriod(1900, 1, 1, 1900, 1, 2));
					ass.SetShiftCategory(sCat);
						ass.AddPersonalActivity(act, new DateTimePeriod(1800, 1, 1, 1800, 1, 2));
					ass.MainActivities().Single().SetId(Guid.NewGuid());
					ass.PersonalActivities().Single().SetId(Guid.NewGuid());

            PersonAssignmentDto dto = target.DomainEntityToDto(ass);
            ActivityLayerDto firstMainShiftLayer = dto.MainShift.LayerCollection.First();

            Assert.AreEqual(0, dto.Version);
            Assert.AreEqual(ass.Id, dto.Id);
            Assert.AreEqual(ass.ShiftCategory.Id, dto.MainShift.ShiftCategoryId);
            Assert.AreEqual(ass.MainActivities().Count(), dto.MainShift.LayerCollection.Count);
            Assert.AreEqual(ass.MainActivities().First().Payload.Id, firstMainShiftLayer.Activity.Id);
            Assert.AreEqual(ass.MainActivities().First().Period,	new DateTimePeriod(firstMainShiftLayer.Period.UtcStartTime, firstMainShiftLayer.Period.UtcEndTime));
            Assert.AreEqual(ass.MainActivities().First().Id,	dto.MainShift.LayerCollection.First().Id);
            Assert.AreEqual(ass.PersonalActivities().Single().Id, dto.PersonalShiftCollection.First().LayerCollection.First().Id);
            Assert.AreEqual(ass.OvertimeActivities().Single().Id, dto.OvertimeShiftCollection.First().LayerCollection.First().Id);
        }
    }
}