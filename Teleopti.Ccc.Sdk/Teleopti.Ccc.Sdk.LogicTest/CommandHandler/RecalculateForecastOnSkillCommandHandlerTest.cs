using System;
using System.Collections.Generic;
using System.ServiceModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
	[TestFixture]
	public class RecalculateForecastOnSkillCommandHandlerTest
	{
		private MockRepository _mocks;
		private IServiceBusEventPublisher _busSender;
		private RecalculateForecastOnSkillCommandHandler _target;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_busSender = _mocks.DynamicMock<IServiceBusEventPublisher>();

			_target = new RecalculateForecastOnSkillCommandHandler(_busSender);
		}

		[Test, ExpectedException(typeof(FaultException))]
		public void ShouldThrowIfBusNotRunning()
		{
			Expect.Call(_busSender.EnsureBus()).Return(false);
			_mocks.ReplayAll();
			_target.Handle(null);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldSendMessageToBus()
		{
			var scenarioId = Guid.NewGuid();
			var skillId = Guid.NewGuid();
			var workloadId = Guid.NewGuid();
			var command = new WorkloadOnSkillSelectionDto{SkillId = skillId, WorkloadId = new List<Guid>{workloadId}};
			var commands = new RecalculateForecastOnSkillCollectionCommandDto
			               	{
			               		ScenarioId = scenarioId,
			               		WorkloadOnSkillSelectionDtos = new List<WorkloadOnSkillSelectionDto> {command}
			               	};
			var message = new RecalculateForecastOnSkillMessageCollection();
			Expect.Call(_busSender.EnsureBus()).Return(true);
			Expect.Call(() =>_busSender.Publish(message)).IgnoreArguments();
			_mocks.ReplayAll();
			_target.Handle(commands);
			_mocks.VerifyAll();
		}
	}

	
}