using System;
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
	public class GetTeamByIdQueryHandlerTest
	{
		private MockRepository mocks;
		private GetTeamByIdQueryHandler target;
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
			target = new GetTeamByIdQueryHandler(assembler, teamRepository, unitOfWorkFactory);
		}

		[Test]
		public void ShouldGetTeamsById()
		{
			var team = TeamFactory.CreateTeam("MyTeam","MySite");
			team.SetId(Guid.NewGuid());
			using (mocks.Record())
			{
				Expect.Call(teamRepository.Get(team.Id.GetValueOrDefault())).Return(team);
				Expect.Call(assembler.DomainEntityToDto(team)).Return(new TeamDto(team));
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			}
			using (mocks.Playback())
			{
				var result = target.Handle(new GetTeamByIdQueryDto { TeamId= team.Id.GetValueOrDefault()});
				result.Count.Should().Be.EqualTo(1);
			}
		}
	}
}