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
	public class GetTeamByDescriptionNameQueryHandlerTest
	{
		private MockRepository mocks;
		private GetTeamByDescriptionNameQueryHandler target;
		private IAssembler<ITeam, TeamDto> assembler;
		private ITeamRepository teamRepository;
		private IUnitOfWorkFactory unitOfWorkFactory;
		private IUnitOfWork unitOfWork;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			assembler = mocks.StrictMock<IAssembler<ITeam, TeamDto>>();
			teamRepository = mocks.StrictMock<ITeamRepository>();
			unitOfWorkFactory = mocks.DynamicMock<IUnitOfWorkFactory>();
			unitOfWork = mocks.DynamicMock<IUnitOfWork>();
			target = new GetTeamByDescriptionNameQueryHandler(assembler, teamRepository, unitOfWorkFactory);
		}

		[Test]
		public void ShouldGetTeamsByName()
		{
			var teamList = new List<ITeam> {TeamFactory.CreateTeam("MyTeam","MySite")};
			using (mocks.Record())
			{
				Expect.Call(teamRepository.FindTeamByDescriptionName("MyTeam")).Return(teamList);
				Expect.Call(assembler.DomainEntitiesToDtos(teamList)).Return(new[] {new TeamDto(teamList[0])});
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			}
			using (mocks.Playback())
			{
				var result = target.Handle(new GetTeamByDescriptionNameQueryDto { DescriptionName= "MyTeam"});
				result.Count.Should().Be.EqualTo(1);
			}
		}
	}
}