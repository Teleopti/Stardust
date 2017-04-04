using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.MultiTenancy;
using Teleopti.Ccc.Sdk.LogicTest.QueryHandler;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class ShiftTradeSwapDetailAssemblerTest
    {
        [Test]
        public void VerifyDtoToDo()
        {
			var person1 = PersonFactory.CreatePerson().WithId();
			var person2 = PersonFactory.CreatePerson().WithId();

			var date1 = new DateOnly(2009, 9, 22);
			var date2 = new DateOnly(2009, 9, 21);

			var checksumFrom = -3;
			var checksumTo = -7;

			var dateTimePeriodAssembler = new DateTimePeriodAssembler();
			var absenceAssembler = new AbsenceAssembler(new FakeAbsenceRepository());
			var target = new ShiftTradeSwapDetailAssembler();
			var shiftCategoryRepository = new FakeShiftCategoryRepository();
			var activityAssembler = new ActivityAssembler(new FakeActivityRepository());
	        var personRepository = new FakePersonRepositoryLegacy();
			personRepository.Add(person1);
			personRepository.Add(person2);
	        var personAssembler = new PersonAssembler(personRepository,
				new WorkflowControlSetAssembler(new ShiftCategoryAssembler(shiftCategoryRepository),
					new DayOffAssembler(new FakeDayOffTemplateRepository()), activityAssembler,
					absenceAssembler), new PersonAccountUpdaterDummy(),
				new TenantPeopleLoader(new FakeTenantLogonDataManager()));
			target.PersonAssembler = personAssembler;
			target.SchedulePartAssembler = new SchedulePartAssembler(
				new PersonAssignmentAssembler(shiftCategoryRepository,
					new ActivityLayerAssembler<MainShiftLayer>(new MainShiftLayerConstructor(), dateTimePeriodAssembler,
						activityAssembler),
					new ActivityLayerAssembler<PersonalShiftLayer>(new PersonalShiftLayerConstructor(),
						new DateTimePeriodAssembler(), activityAssembler),
					new OvertimeLayerAssembler(dateTimePeriodAssembler, activityAssembler,
						new FakeMultiplicatorDefinitionSetRepository())),
				new PersonAbsenceAssembler(absenceAssembler, dateTimePeriodAssembler),
				new PersonDayOffAssembler(personAssembler, dateTimePeriodAssembler),
				new PersonMeetingAssembler(personAssembler, dateTimePeriodAssembler),
				new ProjectedLayerAssembler(dateTimePeriodAssembler), dateTimePeriodAssembler,
				new SdkProjectionServiceFactory(), new ScheduleTagAssembler(new FakeScheduleTagRepository()));

			var shiftTradeSwapDetailDto = new ShiftTradeSwapDetailDto
                                                                  {
																	  DateFrom = new DateOnlyDto { DateTime = date1.Date },
																	  DateTo = new DateOnlyDto { DateTime = date2.Date },
                                                                      Id = Guid.NewGuid(),
                                                                      PersonFrom = new PersonDto{Id=person1.Id,Name = person1.Name.ToString()},
                                                                      PersonTo = new PersonDto { Id = person2.Id, Name = person2.Name.ToString() },
                                                                      ChecksumFrom = checksumFrom,
                                                                      ChecksumTo = checksumTo
                                                                  };
			
			var shiftTradeSwapDetail = target.DtoToDomainEntity(shiftTradeSwapDetailDto);
            Assert.AreEqual(shiftTradeSwapDetail.Id.Value,shiftTradeSwapDetailDto.Id.Value);
            Assert.AreEqual(shiftTradeSwapDetail.DateFrom.Date,shiftTradeSwapDetailDto.DateFrom.DateTime);
            Assert.AreEqual(shiftTradeSwapDetail.DateTo.Date, shiftTradeSwapDetailDto.DateTo.DateTime);
            Assert.AreEqual(shiftTradeSwapDetail.PersonFrom.Id.Value,shiftTradeSwapDetailDto.PersonFrom.Id.Value);
            Assert.AreEqual(shiftTradeSwapDetail.PersonTo.Id.Value, shiftTradeSwapDetailDto.PersonTo.Id.Value);
            Assert.AreEqual(shiftTradeSwapDetail.ChecksumFrom,shiftTradeSwapDetailDto.ChecksumFrom);
            Assert.AreEqual(shiftTradeSwapDetail.ChecksumTo, shiftTradeSwapDetailDto.ChecksumTo);
        }

        [Test]
        public void VerifyDoToDto()
        {
			var person1 = PersonFactory.CreatePerson().WithId();
			var person2 = PersonFactory.CreatePerson().WithId();

			var date1 = new DateOnly(2009, 9, 22);
			var date2 = new DateOnly(2009, 9, 21);

			var checksumFrom = -3;
			var checksumTo = -7;

			var dateTimePeriodAssembler = new DateTimePeriodAssembler();
			var absenceAssembler = new AbsenceAssembler(new FakeAbsenceRepository());
			var target = new ShiftTradeSwapDetailAssembler();
			var shiftCategoryRepository = new FakeShiftCategoryRepository();
			var activityAssembler = new ActivityAssembler(new FakeActivityRepository());
			var personRepository = new FakePersonRepositoryLegacy();
			personRepository.Add(person1);
			personRepository.Add(person2);
			var personAssembler = new PersonAssembler(personRepository,
				new WorkflowControlSetAssembler(new ShiftCategoryAssembler(shiftCategoryRepository),
					new DayOffAssembler(new FakeDayOffTemplateRepository()), activityAssembler,
					absenceAssembler), new PersonAccountUpdaterDummy(),
				new TenantPeopleLoader(new FakeTenantLogonDataManager()));
			target.PersonAssembler = personAssembler;
			target.SchedulePartAssembler = new SchedulePartAssembler(
				new PersonAssignmentAssembler(shiftCategoryRepository,
					new ActivityLayerAssembler<MainShiftLayer>(new MainShiftLayerConstructor(), dateTimePeriodAssembler,
						activityAssembler),
					new ActivityLayerAssembler<PersonalShiftLayer>(new PersonalShiftLayerConstructor(),
						new DateTimePeriodAssembler(), activityAssembler),
					new OvertimeLayerAssembler(dateTimePeriodAssembler, activityAssembler,
						new FakeMultiplicatorDefinitionSetRepository())),
				new PersonAbsenceAssembler(absenceAssembler, dateTimePeriodAssembler),
				new PersonDayOffAssembler(personAssembler, dateTimePeriodAssembler),
				new PersonMeetingAssembler(personAssembler, dateTimePeriodAssembler),
				new ProjectedLayerAssembler(dateTimePeriodAssembler), dateTimePeriodAssembler,
				new SdkProjectionServiceFactory(), new ScheduleTagAssembler(new FakeScheduleTagRepository()));
			
			var scenario = new FakeCurrentScenario();
			var storage = new FakeScheduleStorage();

	        var dictionary = storage.FindSchedulesForPersonsOnlyInGivenPeriod(new [] { person1,person2}, new ScheduleDictionaryLoadOptions(false, false),
		        new DateOnlyPeriod(2000, 1, 1, 2000, 1, 1), scenario.Current());
	        var schedulePart1 = dictionary[person1].ScheduledDay(new DateOnly(2000, 1, 1));
            var schedulePart2 = dictionary[person1].ScheduledDay(new DateOnly(2000, 1, 1));

			var shiftTradeSwapDetail = new ShiftTradeSwapDetail(person1,person2,date1,date2).WithId();
            shiftTradeSwapDetail.SchedulePartFrom = schedulePart1;
            shiftTradeSwapDetail.SchedulePartTo = schedulePart2;
            shiftTradeSwapDetail.ChecksumFrom = checksumFrom;
            shiftTradeSwapDetail.ChecksumTo = checksumTo;

            var shiftTradeSwapDetailDto = target.DomainEntityToDto(shiftTradeSwapDetail);
            Assert.AreEqual(shiftTradeSwapDetailDto.Id.Value, shiftTradeSwapDetail.Id.Value);
            Assert.AreEqual(shiftTradeSwapDetailDto.DateFrom.DateTime, shiftTradeSwapDetail.DateFrom.Date);
            Assert.AreEqual(shiftTradeSwapDetailDto.DateTo.DateTime, shiftTradeSwapDetail.DateTo.Date);
            Assert.AreEqual(shiftTradeSwapDetailDto.PersonFrom.Id.Value, shiftTradeSwapDetail.PersonFrom.Id.Value);
            Assert.AreEqual(shiftTradeSwapDetailDto.PersonTo.Id.Value, shiftTradeSwapDetail.PersonTo.Id.Value);
            Assert.AreEqual(shiftTradeSwapDetailDto.ChecksumFrom, shiftTradeSwapDetail.ChecksumFrom);
            Assert.AreEqual(shiftTradeSwapDetailDto.ChecksumTo, shiftTradeSwapDetail.ChecksumTo);
        }
    }
}