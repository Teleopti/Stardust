using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Islands.CommandHandler
{
	[TestFixture(SUT.IntradayOptimization, false)]
	[TestFixture(SUT.Scheduling, false)]
	[TestFixture(SUT.DayOffOptimization, false)]
	[TestFixture(SUT.IntradayOptimization, true)]
	[TestFixture(SUT.Scheduling, true)]
	[TestFixture(SUT.DayOffOptimization, true)]
	[DomainTest]
	[Toggle(Toggles.ResourcePlanner_DayOffOptimizationIslands_47208)]
	public abstract class ResourcePlannerCommandHandlerTest : IConfigureToggleManager
	{
		private readonly SUT _sut;
		private readonly bool _noPytteIslands47500;

		public IntradayOptimizationCommandHandler IntradayOptimizationCommandHandler;
		public SchedulingCommandHandler SchedulingCommandHandler;
		public IDayOffOptimizationCommandHandler DayOffOptimizationCommandHandler;
		public OptimizationPreferencesDefaultValueProvider OptimizationPreferencesProvider;
		public SchedulingOptionsProvider SchedulingOptionsProvider;

		protected ResourcePlannerCommandHandlerTest(SUT sut, bool noPytteIslands47500)
		{
			_sut = sut;
			_noPytteIslands47500 = noPytteIslands47500;
		}

		protected ICommandIdentifier ExecuteTarget()
		{
			return ExecuteTarget(DateOnly.Today.ToDateOnlyPeriod());
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
							UseTeam = teamBlockType == TeamBlockType.Team || teamBlockType == TeamBlockType.TeamAndBlock
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
				case SUT.DayOffOptimization:
					if (teamBlockType.HasValue)
					{
						OptimizationPreferencesProvider.SetFromTestsOnly(new OptimizationPreferences
						{
							Extra = teamBlockType.Value.CreateExtraPreferences()
						});
					}
					var doCmd = new DayOffOptimizationCommand {Period = period, AgentsToOptimize = agents};
					DayOffOptimizationCommandHandler.Execute(doCmd, null);
					return doCmd;
				default:
					throw new NotSupportedException();
			}
		}

		public void Configure(FakeToggleManager toggleManager)
		{
			if(_noPytteIslands47500)
				toggleManager.Enable(Toggles.ResourcePlanner_NoPytteIslands_47500);
		}
	}
}