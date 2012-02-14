using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class OvertimeDefinitionSetAssemblerTest
    {
        private OvertimeDefinitionSetAssembler _target;
        private OvertimeDefinitionSetDto _partTimePercentageDto;
        private MockRepository _mocks;
        private IMultiplicatorDefinitionSetRepository _multiplicatorDefinitionSetRep;
        private IMultiplicatorDefinitionSet _multiplicatorDefinitionSetDomain;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _multiplicatorDefinitionSetRep = _mocks.StrictMock<IMultiplicatorDefinitionSetRepository>();
            _target = new OvertimeDefinitionSetAssembler(_multiplicatorDefinitionSetRep);

            // Create domain object
            _multiplicatorDefinitionSetDomain = new MultiplicatorDefinitionSet("My overtime",MultiplicatorType.Overtime);
            _multiplicatorDefinitionSetDomain.SetId(Guid.NewGuid());

            // Create Dto object
            _partTimePercentageDto = new OvertimeDefinitionSetDto { Id = _multiplicatorDefinitionSetDomain.Id };
        }

        [Test]
        public void VerifyDomainEntityToDto()
        {
            OvertimeDefinitionSetDto overtimeDefinitionSetDto = _target.DomainEntityToDto(_multiplicatorDefinitionSetDomain);

            Assert.AreEqual(_multiplicatorDefinitionSetDomain.Id, overtimeDefinitionSetDto.Id);
            Assert.AreEqual(_multiplicatorDefinitionSetDomain.Name, overtimeDefinitionSetDto.Description);
            Assert.IsFalse(overtimeDefinitionSetDto.IsDeleted);
        }

        [Test]
        public void VerifyDtoToDomainEntity()
        {
            using (_mocks.Record())
            {
                Expect.Call(_multiplicatorDefinitionSetRep.Get(_partTimePercentageDto.Id.Value)).Return(_multiplicatorDefinitionSetDomain).Repeat.Once();
            }

            using (_mocks.Playback())
            {
                IMultiplicatorDefinitionSet multiplicatorDefinitionSet = _target.DtoToDomainEntity(_partTimePercentageDto);
                Assert.IsNotNull(multiplicatorDefinitionSet);
            }
        }
    }
}
