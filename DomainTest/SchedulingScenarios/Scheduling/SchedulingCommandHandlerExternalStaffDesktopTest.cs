﻿using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	[UseIocForFatClient]
	public class SchedulingCommandHandlerExternalStaffDesktopTest
	{
		public SchedulingCommandHandler Target;
		public FakeEventPublisher EventPublisher;
		public Func<ISchedulingResultStateHolder> SchedulingResultStateHolder;

		[Test]
		[Ignore("#46845")]
		public void ShouldConsiderBpoAgentsWhenCreatingIslands()
		{
			var period = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 10);
			var skill1 = new Skill().DefaultResolution(60).WithId();
			var skill2 = new Skill().DefaultResolution(60).WithId();
			var agent1 = new Person().WithId().WithPersonPeriod(skill1);
			var agent2 = new Person().WithId().WithPersonPeriod(skill2);
			//prevents creating two different islands
			var externalStaff = new ExternalStaff(1, new[] { skill1, skill2 }, new DateTimePeriod(new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc), new DateTime(2000, 1, 1, 13, 0, 0, DateTimeKind.Utc)));
			var stateHolder = SchedulingResultStateHolder();
			stateHolder.LoadedAgents = new[] { agent1, agent2 };
			stateHolder.ExternalStaff = new[] { externalStaff };

			Target.Execute(new SchedulingCommand
			{
				Period = period
			});

			EventPublisher.PublishedEvents.OfType<IntradayOptimizationWasOrdered>().Count().Should().Be.EqualTo(1);
		}
	}
}