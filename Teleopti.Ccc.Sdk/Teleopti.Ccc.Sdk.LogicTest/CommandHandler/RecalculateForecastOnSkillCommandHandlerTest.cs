using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Ccc.Sdk.LogicTest.OldTests;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
	[TestFixture]
	public class RecalculateForecastOnSkillCommandHandlerTest
	{
		private MockRepository _mocks;
		private RecalculateForecastOnSkillCommandHandler _target;
	    private IEventPublisher _publisher;

	    [SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
            _publisher = _mocks.DynamicMock<IEventPublisher>();
           // _publisher = new FakeEventPublisher();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			loggedOnUser.Stub(x => x.CurrentUser()).Return(LogOn.loggedOnPerson);
			_target = new RecalculateForecastOnSkillCommandHandler(_publisher, new DummyInfrastructureInfoPopulator(), loggedOnUser);
		}

		[Test]
		public void ShouldThrowIfBusNotRunning()
		{
			var scenarioId = Guid.NewGuid();
			var skillId = Guid.NewGuid();
			var workloadId = Guid.NewGuid();
			var command = new WorkloadOnSkillSelectionDto { SkillId = skillId, WorkloadId = new List<Guid> { workloadId } };
			var commands = new RecalculateForecastOnSkillCollectionCommandDto
			{
				ScenarioId = scenarioId,
				WorkloadOnSkillSelectionDtos = new List<WorkloadOnSkillSelectionDto> { command }
			};
            //Expect.Call(() => _busSender.Send(Arg<object>.Is.Anything, Arg<bool>.Is.Equal(true)));
            Expect.Call(() => _publisher.Publish(new RecalculateForecastOnSkillCollectionEvent())).IgnoreArguments();
            _mocks.ReplayAll();
			_target.Handle(commands);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldSendMessageToBus()
		{
			
            //	Expect.Call(() =>_busSender.Send(message, false)).IgnoreArguments();
            Expect.Call(() => _publisher.Publish(new RecalculateForecastOnSkillCollectionEvent())).IgnoreArguments();

            _mocks.ReplayAll();

            var scenarioId = Guid.NewGuid();
            var skillId = Guid.NewGuid();
            var workloadId = Guid.NewGuid();
            var command = new WorkloadOnSkillSelectionDto { SkillId = skillId, WorkloadId = new List<Guid> { workloadId } };
            var commands = new RecalculateForecastOnSkillCollectionCommandDto
            {
                ScenarioId = scenarioId,
                WorkloadOnSkillSelectionDtos = new List<WorkloadOnSkillSelectionDto> { command }
            };
            _target.Handle(commands);
			_mocks.VerifyAll();
		}
	}

	
}