using System;
using System.Drawing;
using System.Globalization;
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
    public class AbsenceRequestAssemblerTest
    {
        private AbsenceRequestAssembler _target;
        private IAssembler<IAbsence, AbsenceDto> _absenceAssembler;
        private IAbsence _absence;
        private MockRepository _mockRepository;
        private CultureInfo _cultureForDetails;
        private DateTimePeriodAssembler _dateTimePeriodAssembler;

        [SetUp]
        public void Setup()
        {
            _cultureForDetails = new CultureInfo("en-US");
            _mockRepository = new MockRepository();
            _dateTimePeriodAssembler = new DateTimePeriodAssembler();
            _absenceAssembler = _mockRepository.StrictMock<IAssembler<IAbsence,AbsenceDto>>();
            _absence = AbsenceFactory.CreateAbsence("Sjuk");
            _absence.SetId(Guid.NewGuid());
            _target = new AbsenceRequestAssembler(new TestCultureProvider(_cultureForDetails),_absenceAssembler,_dateTimePeriodAssembler);
        }

        [Test]
        public void VerifyDtoToDo()
        {
            DateTimePeriod period = new DateTimePeriod(2009, 7, 5, 2009, 7, 31);
            AbsenceDto absenceDto = new AbsenceDto { DisplayColor = new ColorDto(Color.DarkViolet) };
            AbsenceRequestDto absenceRequestDto = new AbsenceRequestDto
                                                   {
                                                       Id = Guid.NewGuid(),
                                                       Period =_dateTimePeriodAssembler.DomainEntityToDto(period),
                                                       Absence = absenceDto
                                                   };
            Expect.Call(_absenceAssembler.DtoToDomainEntity(absenceDto)).Return(_absence);

            _mockRepository.ReplayAll();
            IRequest textRequest = _target.DtoToDomainEntity(absenceRequestDto);
            Assert.AreEqual(period, textRequest.Period);
            _mockRepository.VerifyAll();
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void VerifyInjectionForDtoToDo()
        {
            _target = new AbsenceRequestAssembler(new TestCultureProvider(_cultureForDetails),null,_dateTimePeriodAssembler);
            _mockRepository.ReplayAll();
            _target.DtoToDomainEntity(new AbsenceRequestDto());
            _mockRepository.VerifyAll();
        }

        [Test]
        public void VerifyDoToDto()
        {
            AbsenceDto absenceDto = new AbsenceDto {Id = _absence.Id, Name = _absence.Name};
            Expect.Call(_absenceAssembler.DomainEntityToDto(_absence)).Return(absenceDto);
            _mockRepository.ReplayAll();
            DateTimePeriod period = new DateTimePeriod(2009, 7, 5, 2009, 7, 31);
            IAbsenceRequest absenceRequest = new AbsenceRequest(_absence, period);

            AbsenceRequestDto absenceRequestDto = _target.DomainEntityToDto(absenceRequest);
            Assert.AreEqual(period.StartDateTime, absenceRequestDto.Period.UtcStartTime);
            Assert.AreEqual(period.EndDateTime, absenceRequestDto.Period.UtcEndTime);
            Assert.AreEqual(_absence.Description.Name,absenceRequestDto.Absence.Name);
            Assert.AreEqual(absenceRequest.GetDetails(_cultureForDetails), absenceRequestDto.Details);
            _mockRepository.VerifyAll();
        }
    }
}