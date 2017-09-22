using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.Sdk.LogicTest.QueryHandler
{
	[TestFixture]
	public class GetSiteByIdQueryHandlerTest
	{
		private MockRepository mocks;
		private GetSiteByIdQueryHandler target;
		private IAssembler<ISite, SiteDto> assembler;
		private ISiteRepository siteRepository;
		private IUnitOfWorkFactory unitOfWorkFactory;
		private IUnitOfWork unitOfWork;
	    private ICurrentUnitOfWorkFactory currentUnitOfWorkFactory;

	    [SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			assembler = mocks.StrictMock<IAssembler<ISite, SiteDto>>();
			siteRepository = mocks.StrictMock<ISiteRepository>();
            unitOfWorkFactory = mocks.DynamicMock<IUnitOfWorkFactory>();
            currentUnitOfWorkFactory = mocks.DynamicMock<ICurrentUnitOfWorkFactory>();
			unitOfWork = mocks.DynamicMock<IUnitOfWork>();
			target = new GetSiteByIdQueryHandler(assembler, siteRepository, currentUnitOfWorkFactory);
		}

		[Test]
		public void ShouldGetSiteById()
		{
			var site = SiteFactory.CreateSimpleSite();
			var siteId = Guid.NewGuid();
			using (mocks.Record())
			{
				Expect.Call(siteRepository.Get(siteId)).Return(site);
				Expect.Call(assembler.DomainEntityToDto(site)).Return(new SiteDto { DescriptionName = site.Description.Name, Id = site.Id});
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(currentUnitOfWorkFactory.Current()).Return(unitOfWorkFactory);
			}
			using (mocks.Playback())
			{
				var result = target.Handle(new GetSiteByIdQueryDto {SiteId = siteId});
				result.Count.Should().Be.EqualTo(1);
			}
		}

		[Test]
		public void ShouldHandleSiteByIdNotFound()
		{
			var siteId = Guid.NewGuid();
			using (mocks.Record())
			{
				Expect.Call(siteRepository.Get(siteId)).Return(null);
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(currentUnitOfWorkFactory.Current()).Return(unitOfWorkFactory);
			}
			using (mocks.Playback())
			{
				var result = target.Handle(new GetSiteByIdQueryDto { SiteId = siteId });
				result.Count.Should().Be.EqualTo(0);
			}
		}
	}
}