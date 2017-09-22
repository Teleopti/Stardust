using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class OvertimeDefinitionSetAssemblerTest
    {
        [Test]
        public void VerifyDomainEntityToDto()
        {
			var multiplicatorDefinitionSetRep = new FakeMultiplicatorDefinitionSetRepository();
			var target = new OvertimeDefinitionSetAssembler(multiplicatorDefinitionSetRep);

			var multiplicatorDefinitionSetDomain = new MultiplicatorDefinitionSet("My overtime", MultiplicatorType.Overtime).WithId();
			multiplicatorDefinitionSetRep.Add(multiplicatorDefinitionSetDomain);
			
			var overtimeDefinitionSetDto = target.DomainEntityToDto(multiplicatorDefinitionSetDomain);

            Assert.AreEqual(multiplicatorDefinitionSetDomain.Id, overtimeDefinitionSetDto.Id);
            Assert.AreEqual(multiplicatorDefinitionSetDomain.Name, overtimeDefinitionSetDto.Description);
            Assert.IsFalse(overtimeDefinitionSetDto.IsDeleted);
        }

	    [Test]
	    public void VerifyDtoToDomainEntity()
	    {
		    var multiplicatorDefinitionSetRep = new FakeMultiplicatorDefinitionSetRepository();
		    var target = new OvertimeDefinitionSetAssembler(multiplicatorDefinitionSetRep);

		    var multiplicatorDefinitionSetDomain =
			    new MultiplicatorDefinitionSet("My overtime", MultiplicatorType.Overtime).WithId();
			multiplicatorDefinitionSetRep.Add(multiplicatorDefinitionSetDomain);

			var partTimePercentageDto = new OvertimeDefinitionSetDto {Id = multiplicatorDefinitionSetDomain.Id};

		    var multiplicatorDefinitionSet = target.DtoToDomainEntity(partTimePercentageDto);
		    Assert.IsNotNull(multiplicatorDefinitionSet);
	    }
    }
}
