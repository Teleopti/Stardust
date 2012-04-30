﻿using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			assembler = mocks.StrictMock<IAssembler<ISite, SiteDto>>();
			siteRepository = mocks.StrictMock<ISiteRepository>();
			unitOfWorkFactory = mocks.DynamicMock<IUnitOfWorkFactory>();
			unitOfWork = mocks.DynamicMock<IUnitOfWork>();
			target = new GetSiteByDescriptionNameQueryHandler(assembler, siteRepository, unitOfWorkFactory);
		}

		[Test]
		public void ShouldGetSitesByName()
		{
			var siteList = new List<ISite> {SiteFactory.CreateSimpleSite()};
			using (mocks.Record())
			{
				Expect.Call(siteRepository.FindSiteByDescriptionName("MySite")).Return(siteList);
				Expect.Call(assembler.DomainEntitiesToDtos(siteList)).Return(new[] {new SiteDto(siteList[0])});
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			}
			using (mocks.Playback())
			{
				var result = target.Handle(new GetSiteByDescriptionNameQueryDto { DescriptionName= "MySite"});
				result.Count.Should().Be.EqualTo(1);
			}
		}
	}
}