using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
	[TestFixture]
	public class SiteAssemblerTest
	{
		[Test]
		public void VerifyDomainEntityToDto()
		{
			var siteRep = new FakeSiteRepository();
			var target = new SiteAssembler(siteRep);

			var siteDomain = new Site("Europe").WithId();
			
			var siteDto = target.DomainEntityToDto(siteDomain);

			Assert.AreEqual(siteDomain.Id, siteDto.Id);
			Assert.AreEqual(siteDomain.Description.Name, siteDto.DescriptionName);
		}

		[Test]
		public void VerifyDtoToDomainEntity()
		{
			var siteRep = new FakeSiteRepository();
			var target = new SiteAssembler(siteRep);

			var siteDomain = new Site("Europe").WithId();
			siteRep.Add(siteDomain);

			var siteDto = new SiteDto {DescriptionName = siteDomain.Description.Name, Id = siteDomain.Id};

			var result = target.DtoToDomainEntity(siteDto);
			Assert.IsNotNull(result);
		}
	}
}
