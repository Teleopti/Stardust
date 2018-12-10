using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;


namespace Teleopti.Ccc.Sdk.LogicTest.OldTests
{
    [TestFixture]
    public class AbsenceLayerDtoTest 
    {
        [Test]
        public void VerifyCanSetProperties()
        {
			var target = new AbsenceLayerDto();
			DateTimePeriod periodDo = new DateTimePeriod(2009, 1, 1, 2009, 1, 2);
            AbsenceDto absenceDto = new AbsenceDto();
            absenceDto.Id = Guid.NewGuid();
            target.Absence = absenceDto;
            Assert.AreSame(absenceDto, target.Absence);
            target.Period = new DateTimePeriodDto { UtcStartTime = periodDo.StartDateTime, UtcEndTime = periodDo.EndDateTime };
            Assert.AreEqual(periodDo.StartDateTime, target.Period.UtcStartTime);
            Assert.AreEqual(periodDo.EndDateTime, target.Period.UtcEndTime);
        }
    }
}