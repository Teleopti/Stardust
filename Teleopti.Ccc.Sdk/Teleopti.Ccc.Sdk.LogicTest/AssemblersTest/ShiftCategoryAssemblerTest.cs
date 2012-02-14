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
    public class ShiftCategoryAssemblerTest
    {
        private ShiftCategoryAssembler _target;
        private IShiftCategory _shiftCategoryDomain;
        private ShiftCategoryDto _shiftCategoryDto;
        private MockRepository _mocks;
        private IShiftCategoryRepository _shiftCategoryRepository;

        [SetUp]
        public void Setup()
        {
            _mocks=new MockRepository();
            _shiftCategoryRepository = _mocks.StrictMock<IShiftCategoryRepository>();
            _target = new ShiftCategoryAssembler(_shiftCategoryRepository);

            // Create domain object
            _shiftCategoryDomain = new ShiftCategory("name")
                                 {
                                     DisplayColor = Color.Blue
                                 };
            _shiftCategoryDomain.SetId(Guid.NewGuid());

            // Create Dto object
            _shiftCategoryDto = new ShiftCategoryDto {Id = _shiftCategoryDomain.Id};
        }

        [Test]
        public void VerifyDomainEntityToDto()
        {
            ShiftCategoryDto shiftCategoryDto = _target.DomainEntityToDto(_shiftCategoryDomain);

            Assert.AreEqual(_shiftCategoryDomain.Id, shiftCategoryDto.Id);
            Assert.AreEqual(_shiftCategoryDomain.Description.Name, shiftCategoryDto.Name);
            Assert.AreEqual(_shiftCategoryDomain.DisplayColor.ToArgb(), shiftCategoryDto.DisplayColor.ToColor().ToArgb());
        }

        [Test]
        public void VerifyDtoToDomainEntity()
        {
            using (_mocks.Record())
            {
                Expect.Call(_shiftCategoryRepository.Get(_shiftCategoryDto.Id.Value)).Return(_shiftCategoryDomain).Repeat.Once();
            }

            using (_mocks.Playback())
            {
                IShiftCategory shiftCategory = _target.DtoToDomainEntity(_shiftCategoryDto);
                Assert.IsNotNull(shiftCategory);
            }
        }
    }
}
