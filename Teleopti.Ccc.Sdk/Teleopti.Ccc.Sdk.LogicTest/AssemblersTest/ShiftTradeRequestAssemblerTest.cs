using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.TestCommon.Services;



namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class ShiftTradeRequestAssemblerTest
    {
        private ShiftTradeRequestAssembler _target;
        private CultureInfo _cultureForDetails;
        private DateTimePeriodAssembler _dateTimePeriodAssembler;

        [SetUp]
        public void Setup()
        {
            _cultureForDetails = new CultureInfo("en-US");
            _dateTimePeriodAssembler = new DateTimePeriodAssembler();
			_target = new ShiftTradeRequestAssembler(new TestCultureProvider(_cultureForDetails), new PersonRequestAuthorizationCheckerForTest(), _dateTimePeriodAssembler, new ShiftTradeRequestStatusCheckerForTestDoesNothing());
        }

        [Test]
        public void VerifyDtoToDo()
        {
            DateTimePeriod period = new DateTimePeriod(2009, 7, 5, 2009, 7, 31);
            ShiftTradeRequestDto shiftTradeRequestDto = new ShiftTradeRequestDto
                                                   {
                                                       Id = Guid.NewGuid(),
                                                       Period =
                                                           _dateTimePeriodAssembler.DomainEntityToDto(period),
                                                       ShiftTradeStatus = ShiftTradeStatusDto.OkByBothParts
                                                   };
            IShiftTradeRequest shiftTradeRequest = _target.DtoToDomainEntity(shiftTradeRequestDto);
            Assert.AreEqual(shiftTradeRequestDto.ShiftTradeStatus, (ShiftTradeStatusDto)shiftTradeRequest.GetShiftTradeStatus(new ShiftTradeRequestStatusCheckerForTestDoesNothing()));
        }

        [Test]
        public void VerifyDoToDto()
        {
            IShiftTradeRequest shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail>());
            shiftTradeRequest.SetShiftTradeStatus(ShiftTradeStatus.OkByBothParts, new PersonRequestAuthorizationCheckerForTest());

            ShiftTradeRequestDto shiftTradeRequestDto = _target.DomainEntityToDto(shiftTradeRequest);
            Assert.AreEqual((ShiftTradeStatusDto)shiftTradeRequest.GetShiftTradeStatus(new ShiftTradeRequestStatusCheckerForTestDoesNothing()), shiftTradeRequestDto.ShiftTradeStatus);
            Assert.AreEqual(shiftTradeRequest.GetDetails(_cultureForDetails), shiftTradeRequestDto.Details);
        }

        [Test]
        public void VerifyInjectionForDoToDto()
        {
            _target = new ShiftTradeRequestAssembler(new TestCultureProvider(_cultureForDetails), new PersonRequestAuthorizationCheckerForTest(), new DateTimePeriodAssembler(),null);

            IShiftTradeRequest shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail>());
            shiftTradeRequest.SetShiftTradeStatus(ShiftTradeStatus.OkByBothParts, new PersonRequestAuthorizationCheckerForTest());
            Assert.Throws<InvalidOperationException>(() => _target.DomainEntityToDto(shiftTradeRequest));
        }
    }
}