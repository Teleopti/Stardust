using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.MultiTenancy;
using Teleopti.Ccc.Sdk.LogicTest.QueryHandler;
using Teleopti.Ccc.Sdk.WcfHost.Ioc;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
	[DomainTest]
    public class ShiftTradeSwapDetailAssemblerTest : IIsolateSystem, IExtendSystem
	{
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonRepository PersonRepository;
		public IScheduleStorage ScheduleStorage;
		public IAssembler<IShiftTradeSwapDetail, ShiftTradeSwapDetailDto> Target;
		
        [Test]
        public void VerifyDtoToDo()
        {
			var person1 = PersonFactory.CreatePerson().WithId();
			PersonRepository.Has(person1);
			var person2 = PersonFactory.CreatePerson().WithId();
			PersonRepository.Has(person2);

			var date1 = new DateOnly(2009, 9, 22);
			var date2 = new DateOnly(2009, 9, 21);

			var checksumFrom = -3;
			var checksumTo = -7;
			
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
			
			var shiftTradeSwapDetail = Target.DtoToDomainEntity(shiftTradeSwapDetailDto);
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
			PersonRepository.Has(person1);
			var person2 = PersonFactory.CreatePerson().WithId();
			PersonRepository.Has(person2);

			var date1 = new DateOnly(2009, 9, 22);
			var date2 = new DateOnly(2009, 9, 21);

			var checksumFrom = -3;
			var checksumTo = -7;

			var scenario = ScenarioRepository.Has("Default");
	        var dictionary = ScheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(new [] { person1,person2}, new ScheduleDictionaryLoadOptions(false, false),
		        new DateOnlyPeriod(2000, 1, 1, 2000, 1, 1), scenario);
	        var schedulePart1 = dictionary[person1].ScheduledDay(new DateOnly(2000, 1, 1));
            var schedulePart2 = dictionary[person1].ScheduledDay(new DateOnly(2000, 1, 1));

			var shiftTradeSwapDetail = new ShiftTradeSwapDetail(person1,person2,date1,date2).WithId();
            shiftTradeSwapDetail.SchedulePartFrom = schedulePart1;
            shiftTradeSwapDetail.SchedulePartTo = schedulePart2;
            shiftTradeSwapDetail.ChecksumFrom = checksumFrom;
            shiftTradeSwapDetail.ChecksumTo = checksumTo;

            var shiftTradeSwapDetailDto = Target.DomainEntityToDto(shiftTradeSwapDetail);
            Assert.AreEqual(shiftTradeSwapDetailDto.Id.Value, shiftTradeSwapDetail.Id.Value);
            Assert.AreEqual(shiftTradeSwapDetailDto.DateFrom.DateTime, shiftTradeSwapDetail.DateFrom.Date);
            Assert.AreEqual(shiftTradeSwapDetailDto.DateTo.DateTime, shiftTradeSwapDetail.DateTo.Date);
            Assert.AreEqual(shiftTradeSwapDetailDto.PersonFrom.Id.Value, shiftTradeSwapDetail.PersonFrom.Id.Value);
            Assert.AreEqual(shiftTradeSwapDetailDto.PersonTo.Id.Value, shiftTradeSwapDetail.PersonTo.Id.Value);
            Assert.AreEqual(shiftTradeSwapDetailDto.ChecksumFrom, shiftTradeSwapDetail.ChecksumFrom);
            Assert.AreEqual(shiftTradeSwapDetailDto.ChecksumTo, shiftTradeSwapDetail.ChecksumTo);
        }
		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddModule(new AssemblerModule());
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<TenantPeopleLoader>().For<ITenantPeopleLoader>();
			isolate.UseTestDouble<FakeTenantLogonDataManager>().For<ITenantLogonDataManager>();
		}
	}
}