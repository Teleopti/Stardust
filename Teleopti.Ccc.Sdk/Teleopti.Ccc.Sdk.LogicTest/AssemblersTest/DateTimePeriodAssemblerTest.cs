using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;


namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class DateTimePeriodAssemblerTest
    {
        private readonly TimeZoneInfo _timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
		
        [Test]
        public void VerifyDomainEntityToDto()
        {
			var target = new DateTimePeriodAssembler {TimeZone = _timeZone};
			
            var dateTimePeriod = new DateTimePeriod(2011, 1, 31, 2011, 2, 1);
			
            var dateTimePeriodDto = target.DomainEntityToDto(dateTimePeriod);

            Assert.AreEqual(dateTimePeriod.StartDateTime,dateTimePeriodDto.UtcStartTime);
            Assert.AreEqual(dateTimePeriod.EndDateTime,dateTimePeriodDto.UtcEndTime);
            Assert.AreEqual(dateTimePeriod.StartDateTimeLocal(_timeZone), dateTimePeriodDto.LocalStartDateTime);
            Assert.AreEqual(dateTimePeriod.EndDateTimeLocal(_timeZone), dateTimePeriodDto.LocalEndDateTime);
        }

        [Test]
        public void VerifyDtoToDomainEntityWithUtcTime()
		{
			var target = new DateTimePeriodAssembler { TimeZone = _timeZone };
			
			var dateTimePeriodDto = new DateTimePeriodDto();
			dateTimePeriodDto.UtcStartTime = new DateTime(2011,2,1,0,0,0,DateTimeKind.Utc);
            dateTimePeriodDto.UtcEndTime = new DateTime(2011,2,2,0,0,0,DateTimeKind.Utc);

            var dateTimePeriod = target.DtoToDomainEntity(dateTimePeriodDto);

            Assert.AreEqual(dateTimePeriodDto.UtcStartTime, dateTimePeriod.StartDateTime);
            Assert.AreEqual(dateTimePeriodDto.UtcEndTime, dateTimePeriod.EndDateTime);
        }

        [Test]
        public void VerifyDtoToDomainEntityWithLocalTimeOnly()
		{
			var target = new DateTimePeriodAssembler { TimeZone = _timeZone };
			
			var dateTimePeriodDto = new DateTimePeriodDto();
			dateTimePeriodDto.LocalStartDateTime = new DateTime(2011, 1, 1);
            dateTimePeriodDto.LocalEndDateTime= new DateTime(2011, 2, 2);

            var dateTimePeriod = target.DtoToDomainEntity(dateTimePeriodDto);

            Assert.AreEqual(TimeZoneInfo.ConvertTimeToUtc(dateTimePeriodDto.LocalStartDateTime, _timeZone), dateTimePeriod.StartDateTime);
            Assert.AreEqual(TimeZoneInfo.ConvertTimeToUtc(dateTimePeriodDto.LocalEndDateTime, _timeZone), dateTimePeriod.EndDateTime);
        }
    }
}
