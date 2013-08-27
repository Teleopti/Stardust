using System;
using System.Drawing;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class DayOffAssemblerTest
    {
        private DayOffAssembler _target;
        private DayOffInfoDto _dayOffIntoDto;
        private MockRepository _mocks;
        private IDayOffTemplateRepository _dayOffRep;
        private IDayOffTemplate _dayOffDomain;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _dayOffRep = _mocks.StrictMock<IDayOffTemplateRepository>();
            _target = new DayOffAssembler(_dayOffRep);

            // Create domain object
            _dayOffDomain = new DayOffTemplate(new Description("Day Off"))
                                 {
                                     DisplayColor = Color.Blue,
                                     Anchor = TimeSpan.FromHours(12),
									 PayrollCode = "payrollcode007"
                                 };
            _dayOffDomain.SetTargetAndFlexibility(TimeSpan.FromHours(36),TimeSpan.FromHours(10));
            _dayOffDomain.SetId(Guid.NewGuid());

            // Create Dto object
            _dayOffIntoDto = new DayOffInfoDto { Id = _dayOffDomain.Id };
        }

        [Test]
        public void VerifyDomainEntityToDto()
        {
            DayOffInfoDto dayOffInfoDto = _target.DomainEntityToDto(_dayOffDomain);

            Assert.AreEqual(_dayOffDomain.Id, dayOffInfoDto.Id);
            Assert.AreEqual(_dayOffDomain.Description.Name, dayOffInfoDto.Name);
            Assert.AreEqual(_dayOffDomain.Description.ShortName, dayOffInfoDto.ShortName);
			Assert.AreEqual(_dayOffDomain.PayrollCode, dayOffInfoDto.PayrollCode);
            Assert.IsFalse(dayOffInfoDto.IsDeleted);
        }

        [Test]
        public void VerifyDtoToDomainEntity()
        {
            using (_mocks.Record())
            {
                Expect.Call(_dayOffRep.Get(_dayOffIntoDto.Id.Value)).Return(_dayOffDomain).Repeat.Once();
            }

            using (_mocks.Playback())
            {
                IDayOffTemplate dayOffTemplate = _target.DtoToDomainEntity(_dayOffIntoDto);
                Assert.IsNotNull(dayOffTemplate);
            }
        }
    }
}
