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
    public class SiteAssemblerTest
        {
            private SiteAssembler _target;
            private ISite _siteDomain;
            private SiteDto _siteDto;
            private MockRepository _mocks;
            private ISiteRepository _siteRep;

        [SetUp]
        public void Setup()
        {
            _mocks=new MockRepository();
            _siteRep = _mocks.StrictMock<ISiteRepository>();
            _target = new SiteAssembler(_siteRep);

            // Create domain object
            _siteDomain = new Site("Europe");
            _siteDomain.SetId(Guid.NewGuid());

            // Create Dto object
			_siteDto = new SiteDto { DescriptionName = _siteDomain.Description.Name, Id = _siteDomain.Id};
        }

        [Test]
        public void VerifyDomainEntityToDto()
        {
            SiteDto siteDto = _target.DomainEntityToDto(_siteDomain);

            Assert.AreEqual(_siteDomain.Id, siteDto.Id);
            Assert.AreEqual(_siteDomain.Description.Name, siteDto.DescriptionName);
        }

        [Test]
        public void VerifyDtoToDomainEntity()
        {
            using (_mocks.Record())
            {
                Expect.Call(_siteRep.Get(_siteDto.Id.Value)).Return(_siteDomain).Repeat.Once();
            }

            using (_mocks.Playback())
            {
                ISite siteDomain = _target.DtoToDomainEntity(_siteDto);
                Assert.IsNotNull(siteDomain);
            }
        }
    }
}
