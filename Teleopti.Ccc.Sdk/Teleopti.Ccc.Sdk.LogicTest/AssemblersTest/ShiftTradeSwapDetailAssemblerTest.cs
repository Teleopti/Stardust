using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class ShiftTradeSwapDetailAssemblerTest
    {
        private ShiftTradeSwapDetailAssembler _target;
        private IPerson _person1;
        private IPerson _person2;
        private DateOnly _date1;
        private DateOnly _date2;
        private MockRepository _mockRepository;
        private IScheduleDataAssembler<IScheduleDay, SchedulePartDto> _scheduleDataAssembler;
        private IAssembler<IPerson, PersonDto> _personAssembler;
        private int _checksumFrom;
        private int _checksumTo;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository();

            _person1 = PersonFactory.CreatePerson();
            _person1.SetId(Guid.NewGuid());
            _person2 = PersonFactory.CreatePerson();
            _person2.SetId(Guid.NewGuid());

            _date1 = new DateOnly(2009, 9, 22);
            _date2 = new DateOnly(2009, 9, 21);

            _checksumFrom = -3;
            _checksumTo = -7;

            _personAssembler = _mockRepository.StrictMock<IAssembler<IPerson,PersonDto>>();
            _scheduleDataAssembler =
                _mockRepository.StrictMock<IScheduleDataAssembler<IScheduleDay, SchedulePartDto>>();

            _target = new ShiftTradeSwapDetailAssembler();
            _target.PersonAssembler = _personAssembler;
            _target.SchedulePartAssembler = _scheduleDataAssembler;
        }

        [Test]
        public void VerifyDtoToDo()
        {
            ShiftTradeSwapDetailDto shiftTradeSwapDetailDto = new ShiftTradeSwapDetailDto
                                                                  {
																	  DateFrom = new DateOnlyDto { DateTime = _date1.Date },
																	  DateTo = new DateOnlyDto { DateTime = _date2.Date },
                                                                      Id = Guid.NewGuid(),
                                                                      PersonFrom = new PersonDto{Id=_person1.Id,Name = _person1.Name.ToString()},
                                                                      PersonTo = new PersonDto { Id = _person2.Id, Name = _person2.Name.ToString() },
                                                                      ChecksumFrom = _checksumFrom,
                                                                      ChecksumTo = _checksumTo
                                                                  };
            Expect.Call(_personAssembler.DtoToDomainEntity(shiftTradeSwapDetailDto.PersonFrom)).Return(_person1);
            Expect.Call(_personAssembler.DtoToDomainEntity(shiftTradeSwapDetailDto.PersonTo)).Return(_person2);

            _mockRepository.ReplayAll();
            IShiftTradeSwapDetail shiftTradeSwapDetail = _target.DtoToDomainEntity(shiftTradeSwapDetailDto);
            Assert.AreEqual(shiftTradeSwapDetail.Id.Value,shiftTradeSwapDetailDto.Id.Value);
            Assert.AreEqual(shiftTradeSwapDetail.DateFrom.Date,shiftTradeSwapDetailDto.DateFrom.DateTime);
            Assert.AreEqual(shiftTradeSwapDetail.DateTo.Date, shiftTradeSwapDetailDto.DateTo.DateTime);
            Assert.AreEqual(shiftTradeSwapDetail.PersonFrom.Id.Value,shiftTradeSwapDetailDto.PersonFrom.Id.Value);
            Assert.AreEqual(shiftTradeSwapDetail.PersonTo.Id.Value, shiftTradeSwapDetailDto.PersonTo.Id.Value);
            Assert.AreEqual(shiftTradeSwapDetail.ChecksumFrom,shiftTradeSwapDetailDto.ChecksumFrom);
            Assert.AreEqual(shiftTradeSwapDetail.ChecksumTo, shiftTradeSwapDetailDto.ChecksumTo);
            _mockRepository.VerifyAll();
        }

        [Test]
        public void VerifyDoToDto()
        {
            SchedulePartDto schedulePartDto1 = new SchedulePartDto();
            SchedulePartDto schedulePartDto2 = new SchedulePartDto();
            IScheduleDay schedulePart1 = _mockRepository.StrictMock<IScheduleDay>();
            IScheduleDay schedulePart2 = _mockRepository.StrictMock<IScheduleDay>();
            
            Expect.Call(_scheduleDataAssembler.DomainEntityToDto(schedulePart1)).Return(schedulePartDto1);
            Expect.Call(_scheduleDataAssembler.DomainEntityToDto(schedulePart2)).Return(schedulePartDto2);
            Expect.Call(_personAssembler.DomainEntityToDto(_person1)).Return(new PersonDto
                                                                                 {
                                                                                     Id = _person1.Id,
                                                                                     Name = _person1.Name.ToString()
                                                                                 });
            Expect.Call(_personAssembler.DomainEntityToDto(_person2)).Return(new PersonDto
            {
                Id = _person2.Id,
                Name = _person2.Name.ToString()
            });
            IShiftTradeSwapDetail shiftTradeSwapDetail = new ShiftTradeSwapDetail(_person1,_person2,_date1,_date2);
            shiftTradeSwapDetail.SetId(Guid.NewGuid());
            shiftTradeSwapDetail.SchedulePartFrom = schedulePart1;
            shiftTradeSwapDetail.SchedulePartTo = schedulePart2;
            shiftTradeSwapDetail.ChecksumFrom = _checksumFrom;
            shiftTradeSwapDetail.ChecksumTo = _checksumTo;

            _mockRepository.ReplayAll();
            ShiftTradeSwapDetailDto shiftTradeSwapDetailDto = _target.DomainEntityToDto(shiftTradeSwapDetail);
            Assert.AreEqual(shiftTradeSwapDetailDto.Id.Value, shiftTradeSwapDetail.Id.Value);
            Assert.AreEqual(shiftTradeSwapDetailDto.DateFrom.DateTime, shiftTradeSwapDetail.DateFrom.Date);
            Assert.AreEqual(shiftTradeSwapDetailDto.DateTo.DateTime, shiftTradeSwapDetail.DateTo.Date);
            Assert.AreEqual(shiftTradeSwapDetailDto.PersonFrom.Id.Value, shiftTradeSwapDetail.PersonFrom.Id.Value);
            Assert.AreEqual(shiftTradeSwapDetailDto.PersonTo.Id.Value, shiftTradeSwapDetail.PersonTo.Id.Value);
            Assert.AreEqual(shiftTradeSwapDetailDto.ChecksumFrom, shiftTradeSwapDetail.ChecksumFrom);
            Assert.AreEqual(shiftTradeSwapDetailDto.ChecksumTo, shiftTradeSwapDetail.ChecksumTo);
            _mockRepository.VerifyAll();
        }
    }
}