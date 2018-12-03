using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;


namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class PartTimePercentageAssemblerTest
    {
        [Test]
        public void VerifyDomainEntityToDto()
        {
			var partTimePercentageRep = new FakePartTimePercentageRepository();
			var target = new PartTimePercentageAssembler(partTimePercentageRep);

			var partTimePercentageDomain = new PartTimePercentage("My contract").WithId();
			partTimePercentageDomain.Percentage = new Percent(0.75);
			partTimePercentageRep.Add(partTimePercentageDomain);

			PartTimePercentageDto partTimePercentageDto = target.DomainEntityToDto(partTimePercentageDomain);

            Assert.AreEqual(partTimePercentageDomain.Id, partTimePercentageDto.Id);
            Assert.AreEqual(partTimePercentageDomain.Description.Name, partTimePercentageDto.Description);
            Assert.IsFalse(partTimePercentageDto.IsDeleted);
        }

	    [Test]
	    public void VerifyDtoToDomainEntity()
	    {
		    var partTimePercentageRep = new FakePartTimePercentageRepository();
		    var target = new PartTimePercentageAssembler(partTimePercentageRep);

		    var partTimePercentageDomain = new PartTimePercentage("My contract").WithId();
		    partTimePercentageDomain.Percentage = new Percent(0.75);
			partTimePercentageRep.Add(partTimePercentageDomain);

		    var partTimePercentageDto = new PartTimePercentageDto {Id = partTimePercentageDomain.Id};
		    var partTimePercentage = target.DtoToDomainEntity(partTimePercentageDto);
		    Assert.IsNotNull(partTimePercentage);
	    }
    }
}
