using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class PartTimePercentageAssemblerTest
    {
        private PartTimePercentageAssembler _target;
        private PartTimePercentageDto _partTimePercentageDto;
        private MockRepository _mocks;
        private IPartTimePercentageRepository _partTimePercentageRep;
        private IPartTimePercentage _partTimePercentageDomain;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _partTimePercentageRep = _mocks.StrictMock<IPartTimePercentageRepository>();
            _target = new PartTimePercentageAssembler(_partTimePercentageRep);

            // Create domain object
            _partTimePercentageDomain = new PartTimePercentage("My contract");
            _partTimePercentageDomain.Percentage = new Percent(0.75);
            _partTimePercentageDomain.SetId(Guid.NewGuid());

            // Create Dto object
            _partTimePercentageDto = new PartTimePercentageDto { Id = _partTimePercentageDomain.Id };
        }

        [Test]
        public void VerifyDomainEntityToDto()
        {
            PartTimePercentageDto partTimePercentageDto = _target.DomainEntityToDto(_partTimePercentageDomain);

            Assert.AreEqual(_partTimePercentageDomain.Id, partTimePercentageDto.Id);
            Assert.AreEqual(_partTimePercentageDomain.Description.Name, partTimePercentageDto.Description);
            Assert.IsFalse(partTimePercentageDto.IsDeleted);
        }

        [Test]
        public void VerifyDtoToDomainEntity()
        {
            using (_mocks.Record())
            {
                Expect.Call(_partTimePercentageRep.Get(_partTimePercentageDto.Id.Value)).Return(_partTimePercentageDomain).Repeat.Once();
            }

            using (_mocks.Playback())
            {
                IPartTimePercentage partTimePercentage = _target.DtoToDomainEntity(_partTimePercentageDto);
                Assert.IsNotNull(partTimePercentage);
            }
        }
    }
}
