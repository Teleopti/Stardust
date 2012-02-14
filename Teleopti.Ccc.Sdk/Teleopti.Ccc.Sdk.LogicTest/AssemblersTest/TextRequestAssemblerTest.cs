﻿using System;
using System.Globalization;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class TextRequestAssemblerTest
    {
        private TextRequestAssembler target;
        private CultureInfo _cultureForDetails;
        private DateTimePeriodAssembler _dateTimePeriodAssembler;

        [SetUp]
        public void Setup()
        {
            _cultureForDetails = new CultureInfo("en-US");
            _dateTimePeriodAssembler = new DateTimePeriodAssembler();

            target = new TextRequestAssembler(new TestCultureProvider(_cultureForDetails),_dateTimePeriodAssembler);
        }
        
        [Test]
        public void VerifyDtoToDo()
        {
            DateTimePeriod period = new DateTimePeriod(2009, 7, 5, 2009, 7, 31);
            TextRequestDto textRequestDto = new TextRequestDto
                                                {
                                                    Id = Guid.NewGuid(),
                                                    Period =_dateTimePeriodAssembler.DomainEntityToDto(period)
                                                };
            IRequest textRequest = target.DtoToDomainEntity(textRequestDto);
            Assert.AreEqual(period, textRequest.Period);
        }

        [Test]
        public void VerifyDoToDto()
        {
            DateTimePeriod period = new DateTimePeriod(2009, 7, 5, 2009, 7, 31);
            TextRequest textRequest = new TextRequest(period);

            TextRequestDto textRequestDto = target.DomainEntityToDto(textRequest);
            Assert.AreEqual(period.StartDateTime,textRequestDto.Period.UtcStartTime);
            Assert.AreEqual(period.EndDateTime, textRequestDto.Period.UtcEndTime);
            Assert.AreEqual(textRequest.GetDetails(_cultureForDetails), textRequestDto.Details);
        }
    }

    public class TestCultureProvider : IUserCultureProvider
    {
        public CultureInfo Culture { get; private set; }

        public TestCultureProvider(CultureInfo cultureInfo)
        {
            Culture = cultureInfo;
        }
    }
}