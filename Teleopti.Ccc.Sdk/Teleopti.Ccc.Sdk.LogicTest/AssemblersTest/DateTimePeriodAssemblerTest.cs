using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class DateTimePeriodAssemblerTest
    {
        private DateTimePeriodAssembler _target;
        private ICccTimeZoneInfo _timeZone;
        private DateTimePeriod _dateTimePeriod;
        private DateTimePeriodDto _dateTimePeriodDto;

        [SetUp]
        public void Setup()
        {
            _timeZone = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            _target = new DateTimePeriodAssembler {TimeZone = _timeZone};

            // Create domain object
            _dateTimePeriod = new DateTimePeriod(2011, 1, 31, 2011, 2, 1);

            // Create Dto object
            _dateTimePeriodDto = new DateTimePeriodDto();
        }

        [Test]
        public void VerifyDomainEntityToDto()
        {
            _dateTimePeriodDto = _target.DomainEntityToDto(_dateTimePeriod);

            Assert.AreEqual(_dateTimePeriod.StartDateTime,_dateTimePeriodDto.UtcStartTime);
            Assert.AreEqual(_dateTimePeriod.EndDateTime,_dateTimePeriodDto.UtcEndTime);
            Assert.AreEqual(_dateTimePeriod.StartDateTimeLocal(_timeZone), _dateTimePeriodDto.LocalStartDateTime);
            Assert.AreEqual(_dateTimePeriod.EndDateTimeLocal(_timeZone), _dateTimePeriodDto.LocalEndDateTime);
        }

        [Test]
        public void VerifyDtoToDomainEntityWithUtcTime()
        {
            _dateTimePeriodDto.UtcStartTime = new DateTime(2011,2,1,0,0,0,DateTimeKind.Utc);
            _dateTimePeriodDto.UtcEndTime = new DateTime(2011,2,2,0,0,0,DateTimeKind.Utc);

            _dateTimePeriod = _target.DtoToDomainEntity(_dateTimePeriodDto);

            Assert.AreEqual(_dateTimePeriodDto.UtcStartTime, _dateTimePeriod.StartDateTime);
            Assert.AreEqual(_dateTimePeriodDto.UtcEndTime, _dateTimePeriod.EndDateTime);
        }

        [Test]
        public void VerifyDtoToDomainEntityWithLocalTimeOnly()
        {
            _dateTimePeriodDto.LocalStartDateTime = new DateTime(2011, 1, 1);
            _dateTimePeriodDto.LocalEndDateTime= new DateTime(2011, 2, 2);

            _dateTimePeriod = _target.DtoToDomainEntity(_dateTimePeriodDto);

            Assert.AreEqual(_timeZone.ConvertTimeToUtc(_dateTimePeriodDto.LocalStartDateTime), _dateTimePeriod.StartDateTime);
            Assert.AreEqual(_timeZone.ConvertTimeToUtc(_dateTimePeriodDto.LocalEndDateTime), _dateTimePeriod.EndDateTime);
        }
    }
}
