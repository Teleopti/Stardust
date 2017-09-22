using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.Sdk.LogicTest.QueryHandler
{
	[TestFixture]
	public class GetAllScenariosQueryHandlerTest
	{
		private MockRepository mocks;
		private IScenarioRepository scenarioRepository;
		private IUnitOfWorkFactory unitOfWorkFactory;
		private GetAllScenariosQueryHandler target;
	    private ICurrentUnitOfWorkFactory currentUnitOfWorkFactory;

	    [SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			scenarioRepository = mocks.DynamicMock<IScenarioRepository>();
			unitOfWorkFactory = mocks.DynamicMock<IUnitOfWorkFactory>();
			currentUnitOfWorkFactory = mocks.DynamicMock<ICurrentUnitOfWorkFactory>();
			target = new GetAllScenariosQueryHandler(scenarioRepository, currentUnitOfWorkFactory);
		}

		[Test]
		public void ShouldGetAllScenariosAvailable()
		{
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			scenario.SetId(Guid.NewGuid());

			var unitOfWork = mocks.DynamicMock<IUnitOfWork>();
			using (mocks.Record())
			{
				Expect.Call(scenarioRepository.FindAllSorted()).Return(new List<IScenario>{scenario});
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(currentUnitOfWorkFactory.Current()).Return(unitOfWorkFactory);
			}
			using (mocks.Playback())
			{
				var result = target.Handle(new GetAllScenariosQueryDto());
				result.First().Id.Should().Be.EqualTo(scenario.Id);
			}
		}
	}
}
