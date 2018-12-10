using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.TestCommon.FakeData;

using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class PersonAssignmentAssemblerTest
    {
	    [Test]
	    public void VerifyDtoToDo()
	    {
		    var shiftCatRep = new FakeShiftCategoryRepository();
		    var actRep = new FakeActivityRepository();
		    var definitionSetRep = new FakeMultiplicatorDefinitionSetRepository();

		    var activityAssembler = new ActivityAssembler(actRep);
		    var dateTimePeriodAssembler = new DateTimePeriodAssembler();
		    var mainShiftLayerAssembler =
			    new ActivityLayerAssembler<MainShiftLayer>(new MainShiftLayerConstructor(),
				    dateTimePeriodAssembler, activityAssembler);
		    var personalShiftLayerAssembler =
			    new ActivityLayerAssembler<PersonalShiftLayer>(new PersonalShiftLayerConstructor(),
				    dateTimePeriodAssembler, activityAssembler);
		    var overtimeShiftLayerAssembler = new OvertimeLayerAssembler(dateTimePeriodAssembler, activityAssembler,
			    definitionSetRep);

		    var contract = ContractFactory.CreateContract("test");
		    var person = PersonFactory.CreatePerson();
		    var scenario = ScenarioFactory.CreateScenarioAggregate();


			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("d1").WithId();
			var activityMain = ActivityFactory.CreateActivity("sd").WithId();
			var activityPers = ActivityFactory.CreateActivity("sd").WithId();
			var activityOvertime = ActivityFactory.CreateActivity("sd").WithId();
			var definitionSet = new MultiplicatorDefinitionSet("Overtime", MultiplicatorType.Overtime).WithId();
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);

			shiftCatRep.Add(shiftCategory);
			actRep.Add(activityMain);
			actRep.Add(activityPers);
			actRep.Add(activityOvertime);
			definitionSetRep.Add(definitionSet);
			
		    var dto = new PersonAssignmentDto {Id = Guid.NewGuid(), Version = 4};
		    dto.MainShift = new MainShiftDto {Id = Guid.NewGuid(), ShiftCategoryId = shiftCategory.Id.Value};
		    var actMain = new ActivityDto {Id = activityMain.Id.Value};
		    var actPers = new ActivityDto { Id = activityPers.Id.Value };
		    var actOvertime = new ActivityDto {Id = activityOvertime.Id.Value};
		    var overtimeDefinitionSetDto = new OvertimeDefinitionSetDto {Id = definitionSet.Id.Value};

		    var mainShiftLayer = new ActivityLayerDto
		    {
			    Period =
				    dateTimePeriodAssembler.DomainEntityToDto(
					    new DateTimePeriod(1900, 1, 1, 1900, 1, 2)),
			    Activity = actMain
		    };
		    dto.MainShift.LayerCollection.Add(mainShiftLayer);

		    var persShiftLayer = new ActivityLayerDto
		    {
			    Period =
				    dateTimePeriodAssembler.DomainEntityToDto(
					    new DateTimePeriod(2000, 1, 1, 2000, 1, 2)),
			    Activity = actPers
		    };
		    var personalShift = new ShiftDto {Id = Guid.NewGuid()};
		    personalShift.LayerCollection.Add(persShiftLayer);
		    dto.PersonalShiftCollection.Add(personalShift);

		    var overtimeLayerDto =
			    new OvertimeLayerDto
			    {
				    Period = dateTimePeriodAssembler.DomainEntityToDto(new DateTimePeriod(2001, 1, 1, 2001, 1, 2)),
				    Activity = actOvertime,
				    OvertimeDefinitionSetId = overtimeDefinitionSetDto.Id.Value
			    };
		    var overtimeShift = new ShiftDto {Id = Guid.NewGuid()};
		    overtimeShift.LayerCollection.Add(overtimeLayerDto);
		    dto.OvertimeShiftCollection.Add(overtimeShift);

		    var target = new PersonAssignmentAssembler(shiftCatRep, mainShiftLayerAssembler, personalShiftLayerAssembler,
			    overtimeShiftLayerAssembler);
			target.Person = person;
			target.DefaultScenario = scenario;

			var entity = target.DtoToDomainEntity(dto);

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

	    [Test]
        public void CannotExecuteFromDtoConverterWithoutPerson()
        {
			var shiftCatRep = new FakeShiftCategoryRepository();
			var actRep = new FakeActivityRepository();
			var definitionSetRep = new FakeMultiplicatorDefinitionSetRepository();

			var activityAssembler = new ActivityAssembler(actRep);
			var dateTimePeriodAssembler = new DateTimePeriodAssembler();
			var mainShiftLayerAssembler =
				new ActivityLayerAssembler<MainShiftLayer>(new MainShiftLayerConstructor(),
																	dateTimePeriodAssembler, activityAssembler);
			var personalShiftLayerAssembler = new ActivityLayerAssembler<PersonalShiftLayer>(new PersonalShiftLayerConstructor(),
																								  dateTimePeriodAssembler, activityAssembler);
			var overtimeShiftLayerAssembler = new OvertimeLayerAssembler(dateTimePeriodAssembler, activityAssembler, definitionSetRep);

			var target = new PersonAssignmentAssembler(shiftCatRep, mainShiftLayerAssembler, personalShiftLayerAssembler, overtimeShiftLayerAssembler);
			
			var scenario = ScenarioFactory.CreateScenarioAggregate();

			target.DefaultScenario = scenario;
			target.Person = null;

            Assert.Throws<InvalidOperationException>(() => target.DtoToDomainEntity(new PersonAssignmentDto()));
        }

        [Test]
        public void CannotExecuteFromDtoConverterWithoutDefaultScenario()
        {
			var shiftCatRep = new FakeShiftCategoryRepository();
			var actRep = new FakeActivityRepository();
			var definitionSetRep = new FakeMultiplicatorDefinitionSetRepository();

			var activityAssembler = new ActivityAssembler(actRep);
			var dateTimePeriodAssembler = new DateTimePeriodAssembler();
			var mainShiftLayerAssembler =
				new ActivityLayerAssembler<MainShiftLayer>(new MainShiftLayerConstructor(),
																	dateTimePeriodAssembler, activityAssembler);
			var personalShiftLayerAssembler = new ActivityLayerAssembler<PersonalShiftLayer>(new PersonalShiftLayerConstructor(),
																								  dateTimePeriodAssembler, activityAssembler);
			var overtimeShiftLayerAssembler = new OvertimeLayerAssembler(dateTimePeriodAssembler, activityAssembler, definitionSetRep);

			var target = new PersonAssignmentAssembler(shiftCatRep, mainShiftLayerAssembler, personalShiftLayerAssembler, overtimeShiftLayerAssembler);
			var person = PersonFactory.CreatePerson();
			
			target.Person = person;
			target.DefaultScenario = null;

			Assert.Throws<InvalidOperationException>(() => target.DtoToDomainEntity(new PersonAssignmentDto()));
        }

        [Test]
        public void VerifyDoToDto()
        {
			var shiftCatRep = new FakeShiftCategoryRepository();
			var actRep = new FakeActivityRepository();
			var definitionSetRep = new FakeMultiplicatorDefinitionSetRepository();

			var activityAssembler = new ActivityAssembler(actRep);
			var dateTimePeriodAssembler = new DateTimePeriodAssembler();
			var mainShiftLayerAssembler =
				new ActivityLayerAssembler<MainShiftLayer>(new MainShiftLayerConstructor(),
																	dateTimePeriodAssembler, activityAssembler);
			var personalShiftLayerAssembler = new ActivityLayerAssembler<PersonalShiftLayer>(new PersonalShiftLayerConstructor(),
																								  dateTimePeriodAssembler, activityAssembler);
			var overtimeShiftLayerAssembler = new OvertimeLayerAssembler(dateTimePeriodAssembler, activityAssembler, definitionSetRep);

			var target = new PersonAssignmentAssembler(shiftCatRep, mainShiftLayerAssembler, personalShiftLayerAssembler, overtimeShiftLayerAssembler);
			var person = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			target.Person = person;
			target.DefaultScenario = scenario;

			var act = ActivityFactory.CreateActivity("asdf").WithId();
            var definitionSet =
                MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("ovt", MultiplicatorType.Overtime).WithId();
            var sCat = ShiftCategoryFactory.CreateShiftCategory("d1").WithId();
            
            var ass = PersonAssignmentFactory.CreatePersonAssignment(person, scenario).WithId();
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

		[Test]
		public void ShouldNotReturnEmptyDtoWhenNoLayersInAssignment()
		{
			var shiftCatRep = new FakeShiftCategoryRepository();
			var actRep = new FakeActivityRepository();
			var definitionSetRep = new FakeMultiplicatorDefinitionSetRepository();

			var activityAssembler = new ActivityAssembler(actRep);
			var dateTimePeriodAssembler = new DateTimePeriodAssembler();
			var mainShiftLayerAssembler =
				new ActivityLayerAssembler<MainShiftLayer>(new MainShiftLayerConstructor(),
																	dateTimePeriodAssembler, activityAssembler);
			var personalShiftLayerAssembler = new ActivityLayerAssembler<PersonalShiftLayer>(new PersonalShiftLayerConstructor(),
																								  dateTimePeriodAssembler, activityAssembler);
			var overtimeShiftLayerAssembler = new OvertimeLayerAssembler(dateTimePeriodAssembler, activityAssembler, definitionSetRep);

			var target = new PersonAssignmentAssembler(shiftCatRep, mainShiftLayerAssembler, personalShiftLayerAssembler, overtimeShiftLayerAssembler);
			var person = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();

			target.Person = person;
			target.DefaultScenario = scenario;

			var ass = PersonAssignmentFactory.CreatePersonAssignment(person, scenario).WithId();
			
			target.DomainEntityToDto(ass).Should().Be.Null();
		}
    }
}