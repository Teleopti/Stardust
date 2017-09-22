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
	public class GetTeamByIdQueryHandlerTest
	{
		private MockRepository mocks;
		private GetTeamByIdQueryHandler target;
		private IAssembler<ITeam, TeamDto> assembler;
		private ITeamRepository teamRepository;
		private IUnitOfWorkFactory unitOfWorkFactory;
		private IUnitOfWork unitOfWork;
	    private ICurrentUnitOfWorkFactory currentUnitOfWorkFactory;

	    [SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			assembler = mocks.StrictMock<IAssembler<ITeam, TeamDto>>();
			teamRepository = mocks.StrictMock<ITeamRepository>();
			unitOfWorkFactory = mocks.DynamicMock<IUnitOfWorkFactory>();
            unitOfWork = mocks.DynamicMock<IUnitOfWork>();
            currentUnitOfWorkFactory = mocks.DynamicMock<ICurrentUnitOfWorkFactory>();
			target = new GetTeamByIdQueryHandler(assembler, teamRepository, currentUnitOfWorkFactory);
		}

		[Test]
		public void ShouldGetTeamById()
		{
			var team = TeamFactory.CreateTeam("MyTeam","MySite");
			team.SetId(Guid.NewGuid());
			using (mocks.Record())
			{
				Expect.Call(teamRepository.Get(team.Id.GetValueOrDefault())).Return(team);
				Expect.Call(assembler.DomainEntityToDto(team)).Return(new TeamDto { Description = team.Description.Name, Id = team.Id});
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(currentUnitOfWorkFactory.Current()).Return(unitOfWorkFactory);
			}
			using (mocks.Playback())
			{
				var result = target.Handle(new GetTeamByIdQueryDto { TeamId= team.Id.GetValueOrDefault()});
				result.Count.Should().Be.EqualTo(1);
			}
		}

		[Test]
		public void ShouldHandleTeamByIdNotFound()
		{
			var teamId = Guid.NewGuid();
			using (mocks.Record())
			{
				Expect.Call(teamRepository.Get(teamId)).Return(null);
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(currentUnitOfWorkFactory.Current()).Return(unitOfWorkFactory);
            }
			using (mocks.Playback())
			{
				var result = target.Handle(new GetTeamByIdQueryDto { TeamId = teamId });
				result.Count.Should().Be.EqualTo(0);
			}
		}
	}
}