﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Islands.CommandHandler
{
	[TestFixture(SUT.IntradayOptimization)]
	[TestFixture(SUT.Scheduling)]
	[DomainTest]
	public abstract class ResourcePlannerCommandHandlerTest
	{
		private readonly SUT _sut;
		
		public IntradayOptimizationCommandHandler IntradayOptimizationCommandHandler;
		public SchedulingCommandHandler SchedulingCommandHandler;
		public OptimizationPreferencesDefaultValueProvider OptimizationPreferencesProvider;
		public SchedulingOptionsProvider SchedulingOptionsProvider;

		protected ResourcePlannerCommandHandlerTest(SUT sut)
		{
			_sut = sut;
		}

		protected ICommandIdentifier ExecuteTarget(DateOnlyPeriod period)
		{
			return ExecuteTarget(period, null);
		}
		
		protected ICommandIdentifier ExecuteTarget(DateOnlyPeriod period, TeamBlockType teamBlockType)
		{
			return ExecuteTarget(period, null, teamBlockType);
		}

		protected ICommandIdentifier ExecuteTarget(DateOnlyPeriod period, IEnumerable<IPerson> agents)
		{
			return ExecuteTarget(period, agents, null);
		}
		
		protected ICommandIdentifier ExecuteTarget(DateOnlyPeriod period, IEnumerable<IPerson> agents, TeamBlockType? teamBlockType)
		{
			switch (_sut)
			{
				case SUT.Scheduling:
					if (teamBlockType.HasValue)
					{
						SchedulingOptionsProvider.SetFromTest(new SchedulingOptions
						{
							UseTeam = teamBlockType == TeamBlockType.Team
						});
					}
					var schedCmd = new SchedulingCommand {Period = period, AgentsToSchedule = agents};
					SchedulingCommandHandler.Execute(schedCmd);
					return schedCmd;
				case SUT.IntradayOptimization:
					if (teamBlockType.HasValue)
					{
						OptimizationPreferencesProvider.SetFromTestsOnly(new OptimizationPreferences
						{
							Extra = teamBlockType.Value.CreateExtraPreferences()
						});
					}
					var optCmd = new IntradayOptimizationCommand {Period = period, AgentsToOptimize = agents};
					IntradayOptimizationCommandHandler.Execute(optCmd);
					return optCmd;
				default:
					throw new NotSupportedException();
			}
		}
	}
}