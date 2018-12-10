using System;
using System.Drawing;
using System.Globalization;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;



namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class AbsenceRequestAssemblerTest
    {
        [Test]
        public void VerifyDtoToDo()
        {
			var cultureForDetails = new CultureInfo("en-US");
			var dateTimePeriodAssembler = new DateTimePeriodAssembler();
			var absence = AbsenceFactory.CreateAbsence("Sjuk").WithId();
			var absenceRepository = new FakeAbsenceRepository();
			absenceRepository.Add(absence);
	        var absenceAssembler = new AbsenceAssembler(absenceRepository);
	        var target = new AbsenceRequestAssembler(new TestCultureProvider(cultureForDetails), absenceAssembler,
		        dateTimePeriodAssembler);

			var period = new DateTimePeriod(2009, 7, 5, 2009, 7, 31);
            var absenceDto = new AbsenceDto { DisplayColor = new ColorDto(Color.DarkViolet) };
            var absenceRequestDto = new AbsenceRequestDto
                                                   {
                                                       Id = Guid.NewGuid(),
                                                       Period =dateTimePeriodAssembler.DomainEntityToDto(period),
                                                       Absence = absenceDto
                                                   };
            
            IRequest textRequest = target.DtoToDomainEntity(absenceRequestDto);
            Assert.AreEqual(period, textRequest.Period);
        }

        [Test]
        public void VerifyInjectionForDtoToDo()
        {
			var cultureForDetails = new CultureInfo("en-US");
			var dateTimePeriodAssembler = new DateTimePeriodAssembler();
			
			var target = new AbsenceRequestAssembler(new TestCultureProvider(cultureForDetails),null,dateTimePeriodAssembler);

	        Assert.Throws<InvalidOperationException>(() => target.DtoToDomainEntity(new AbsenceRequestDto()));
        }

        [Test]
        public void VerifyDoToDto()
        {
			var cultureForDetails = new CultureInfo("en-US");
			var dateTimePeriodAssembler = new DateTimePeriodAssembler();
			var absence = AbsenceFactory.CreateAbsence("Sjuk").WithId();
	        var absenceRepository = new FakeAbsenceRepository();
			absenceRepository.Add(absence);

	        var absenceAssembler = new AbsenceAssembler(absenceRepository);
			var target = new AbsenceRequestAssembler(new TestCultureProvider(cultureForDetails), absenceAssembler, dateTimePeriodAssembler);
			
            var period = new DateTimePeriod(2009, 7, 5, 2009, 7, 31);
            var absenceRequest = (IAbsenceRequest)new PersonRequest(PersonFactory.CreatePerson(), new AbsenceRequest(absence, period)).Request;

            var absenceRequestDto = target.DomainEntityToDto(absenceRequest);
            Assert.AreEqual(period.StartDateTime, absenceRequestDto.Period.UtcStartTime);
            Assert.AreEqual(period.EndDateTime, absenceRequestDto.Period.UtcEndTime);
            Assert.AreEqual(absence.Description.Name,absenceRequestDto.Absence.Name);
            Assert.AreEqual(absenceRequest.GetDetails(cultureForDetails), absenceRequestDto.Details);
        }
    }
}