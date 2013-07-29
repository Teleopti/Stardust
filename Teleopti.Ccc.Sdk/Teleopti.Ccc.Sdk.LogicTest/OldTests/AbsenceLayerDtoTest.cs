using System;
using NUnit.Framework;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.OldTests
{
    [TestFixture]
    public class AbsenceLayerDtoTest 
    {
        private AbsenceLayerDto   _target;

        [SetUp]
        public void Setup()
        {
            _target = new AbsenceLayerDto();
        }

        [Test]
        public void VerifyCanSetProperties()
        {
            DateTimePeriod periodDo = new DateTimePeriod(2009, 1, 1, 2009, 1, 2);
            AbsenceDto absenceDto = new AbsenceDto();
            absenceDto.Id = Guid.NewGuid();
            _target.Absence = absenceDto;
            Assert.AreSame(absenceDto, _target.Absence);
            _target.Period = new DateTimePeriodDto { UtcStartTime = periodDo.StartDateTime, UtcEndTime = periodDo.EndDateTime };
            Assert.AreEqual(periodDo.StartDateTime, _target.Period.UtcStartTime);
            Assert.AreEqual(periodDo.EndDateTime, _target.Period.UtcEndTime);
        }
    }
}