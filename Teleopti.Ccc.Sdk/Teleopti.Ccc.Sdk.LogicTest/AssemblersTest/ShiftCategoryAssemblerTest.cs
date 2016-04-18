using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class ShiftCategoryAssemblerTest
    {
        [Test]
        public void VerifyDomainEntityToDto()
        {
			var shiftCategoryRepository = new FakeShiftCategoryRepository();
			var target = new ShiftCategoryAssembler(shiftCategoryRepository);

			var shiftCategoryDomain = new ShiftCategory("name")
			{
				DisplayColor = Color.Blue
			}.WithId();

			var shiftCategoryDto = target.DomainEntityToDto(shiftCategoryDomain);

            Assert.AreEqual(shiftCategoryDomain.Id, shiftCategoryDto.Id);
            Assert.AreEqual(shiftCategoryDomain.Description.Name, shiftCategoryDto.Name);
            Assert.AreEqual(shiftCategoryDomain.DisplayColor.ToArgb(), shiftCategoryDto.DisplayColor.ToColor().ToArgb());
        }

	    [Test]
	    public void VerifyDtoToDomainEntity()
	    {
		    var shiftCategoryRepository = new FakeShiftCategoryRepository();
		    var target = new ShiftCategoryAssembler(shiftCategoryRepository);

		    var shiftCategoryDomain = new ShiftCategory("name")
		    {
			    DisplayColor = Color.Blue
		    }.WithId();
		    shiftCategoryRepository.Add(shiftCategoryDomain);

		    var shiftCategoryDto = new ShiftCategoryDto {Id = shiftCategoryDomain.Id};

		    var shiftCategory = target.DtoToDomainEntity(shiftCategoryDto);
		    Assert.IsNotNull(shiftCategory);
	    }
    }
}
