using System;
using System.Globalization;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class TextRequestAssemblerTest
    {
        [Test]
        public void VerifyDtoToDo()
        {
			var cultureForDetails = new CultureInfo("en-US");
			var dateTimePeriodAssembler = new DateTimePeriodAssembler();

			var target = new TextRequestAssembler(new TestCultureProvider(cultureForDetails), dateTimePeriodAssembler);
			DateTimePeriod period = new DateTimePeriod(2009, 7, 5, 2009, 7, 31);
            TextRequestDto textRequestDto = new TextRequestDto
                                                {
                                                    Id = Guid.NewGuid(),
                                                    Period =dateTimePeriodAssembler.DomainEntityToDto(period)
                                                };
            IRequest textRequest = target.DtoToDomainEntity(textRequestDto);
            Assert.AreEqual(period, textRequest.Period);
        }

        [Test]
        public void VerifyDoToDto()
        {
			var cultureForDetails = new CultureInfo("en-US");
			var dateTimePeriodAssembler = new DateTimePeriodAssembler();

			var target = new TextRequestAssembler(new TestCultureProvider(cultureForDetails), dateTimePeriodAssembler);
			DateTimePeriod period = new DateTimePeriod(2009, 7, 5, 2009, 7, 31);
            var textRequest = new PersonRequest(PersonFactory.CreatePerson(), new TextRequest(period)).Request;

            TextRequestDto textRequestDto = target.DomainEntityToDto(textRequest);
            Assert.AreEqual(period.StartDateTime,textRequestDto.Period.UtcStartTime);
            Assert.AreEqual(period.EndDateTime, textRequestDto.Period.UtcEndTime);
            Assert.AreEqual(textRequest.GetDetails(cultureForDetails), textRequestDto.Details);
        }
    }
}