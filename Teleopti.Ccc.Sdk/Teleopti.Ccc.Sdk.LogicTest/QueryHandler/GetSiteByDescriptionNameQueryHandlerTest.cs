using System.Collections.Generic;
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
	public class GetSiteByDescriptionNameQueryHandlerTest
	{
		private MockRepository mocks;
		private GetSiteByDescriptionNameQueryHandler target;
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
			target = new GetSiteByDescriptionNameQueryHandler(assembler, siteRepository, currentUnitOfWorkFactory);
		}

		[Test]
		public void ShouldGetSitesByName()
		{
			var siteList = new List<ISite> {SiteFactory.CreateSimpleSite()};
			using (mocks.Record())
			{
				Expect.Call(siteRepository.FindSiteByDescriptionName("MySite")).Return(siteList);
				Expect.Call(assembler.DomainEntitiesToDtos(siteList)).Return(new[] { new SiteDto { DescriptionName = siteList[0].Description.Name, Id = siteList[0].Id } });
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(currentUnitOfWorkFactory.Current()).Return(unitOfWorkFactory);
			}
			using (mocks.Playback())
			{
				var result = target.Handle(new GetSiteByDescriptionNameQueryDto { DescriptionName= "MySite"});
				result.Count.Should().Be.EqualTo(1);
			}
		}
	}
}