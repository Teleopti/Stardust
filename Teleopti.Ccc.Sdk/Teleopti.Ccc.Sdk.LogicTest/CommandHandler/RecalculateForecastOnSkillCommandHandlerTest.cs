﻿using System;
using System.Collections.Generic;
using System.ServiceModel;
using NUnit.Framework;
using Rhino.Mocks;
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
		private IServiceBusSender _busSender;
		private RecalculateForecastOnSkillCommandHandler _target;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_busSender = _mocks.DynamicMock<IServiceBusSender>();

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
			var buId = Guid.NewGuid();
			var scenarioId = Guid.NewGuid();
			var ownerId = Guid.NewGuid();
			var skillId = Guid.NewGuid();
			var workloadId = Guid.NewGuid();
			var command = new WorkloadOnSkillSelectionDto{SkillId = skillId, WorkloadId = new List<Guid>{workloadId}};
			var commands = new RecalculateForecastOnSkillCollectionCommandDto
			               	{
			               		BusinessUnitId = buId,
			               		DataSource = "datasource",
			               		OwnerPersonId = ownerId,
			               		ScenarioId = scenarioId,
			               		WorkloadOnSkillSelectionDtos = new List<WorkloadOnSkillSelectionDto> {command}
			               	};
			var message = new RecalculateForecastOnSkillMessageCollection();
			Expect.Call(_busSender.EnsureBus()).Return(true);
			Expect.Call(() =>_busSender.NotifyServiceBus(message)).IgnoreArguments();
			_mocks.ReplayAll();
			_target.Handle(commands);
			_mocks.VerifyAll();
		}
	}

	
}