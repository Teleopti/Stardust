using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;


namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class ContractAssemblerTest
    {
        [Test]
        public void VerifyDomainEntityToDto()
        {
			var contractDomain = new Contract("My contract") { EmploymentType = EmploymentType.HourlyStaff }.WithId();
			contractDomain.AddMultiplicatorDefinitionSetCollection(new MultiplicatorDefinitionSet("My overtime",
																								   MultiplicatorType.Overtime));
			contractDomain.AddMultiplicatorDefinitionSetCollection(new MultiplicatorDefinitionSet("Shift Allowance", MultiplicatorType.OBTime));
			var contractRep = new FakeContractRepository();
			contractRep.Add(contractDomain);
			var target = new ContractAssembler(contractRep);
			
			ContractDto contractDto = target.DomainEntityToDto(contractDomain);

            Assert.AreEqual(contractDomain.Id, contractDto.Id);
            Assert.AreEqual(contractDomain.Description.Name, contractDto.Description);
			Assert.AreEqual(Teleopti.Interfaces.Domain.EmploymentType.HourlyStaff, contractDto.EmploymentType);
            Assert.AreEqual(1,contractDto.AvailableOvertimeDefinitionSets.Count);
			Assert.AreEqual(1, contractDto.AvailableShiftAllowanceDefinitionSets.Count);
            Assert.IsFalse(contractDto.IsDeleted);
        }

	    [Test]
	    public void VerifyDtoToDomainEntity()
	    {
		    var contractDomain = new Contract("My contract") {EmploymentType = EmploymentType.HourlyStaff}.WithId();
		    contractDomain.AddMultiplicatorDefinitionSetCollection(new MultiplicatorDefinitionSet("My overtime",
			    MultiplicatorType.Overtime));
		    contractDomain.AddMultiplicatorDefinitionSetCollection(new MultiplicatorDefinitionSet("Shift Allowance",
			    MultiplicatorType.OBTime));
		    var contractRep = new FakeContractRepository();
		    contractRep.Add(contractDomain);
		    var target = new ContractAssembler(contractRep);

		    var contractDto = new ContractDto {Id = contractDomain.Id};

		    IContract contract = target.DtoToDomainEntity(contractDto);
		    Assert.IsNotNull(contract);
	    }
    }
}
