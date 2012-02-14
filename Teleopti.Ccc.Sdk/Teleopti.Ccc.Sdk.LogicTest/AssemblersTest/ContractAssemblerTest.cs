using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class ContractAssemblerTest
    {
        private ContractAssembler _target;
        private ContractDto _contractDto;
        private MockRepository _mocks;
        private IContractRepository _contractRep;
        private IContract _contractDomain;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _contractRep = _mocks.StrictMock<IContractRepository>();
            _target = new ContractAssembler(_contractRep);

            // Create domain object
            _contractDomain = new Contract("My contract")
                                 {
                                     EmploymentType = EmploymentType.HourlyStaff
                                 };
            _contractDomain.AddMultiplicatorDefinitionSetCollection(new MultiplicatorDefinitionSet("My overtime",
                                                                                                   MultiplicatorType.Overtime));
            _contractDomain.SetId(Guid.NewGuid());

            // Create Dto object
            _contractDto = new ContractDto { Id = _contractDomain.Id };
        }

        [Test]
        public void VerifyDomainEntityToDto()
        {
            ContractDto contractDto = _target.DomainEntityToDto(_contractDomain);

            Assert.AreEqual(_contractDomain.Id, contractDto.Id);
            Assert.AreEqual(_contractDomain.Description.Name, contractDto.Description);
            Assert.AreEqual(EmploymentType.HourlyStaff, contractDto.EmploymentType);
            Assert.AreEqual(1,contractDto.AvailableOvertimeDefinitionSets.Count);
            Assert.IsFalse(contractDto.IsDeleted);
        }

        [Test]
        public void VerifyDtoToDomainEntity()
        {
            using (_mocks.Record())
            {
                Expect.Call(_contractRep.Get(_contractDto.Id.Value)).Return(_contractDomain).Repeat.Once();
            }

            using (_mocks.Playback())
            {
                IContract contract = _target.DtoToDomainEntity(_contractDto);
                Assert.IsNotNull(contract);
            }
        }
    }
}
