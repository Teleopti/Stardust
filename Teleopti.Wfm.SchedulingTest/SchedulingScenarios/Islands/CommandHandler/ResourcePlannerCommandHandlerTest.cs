using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.SchedulingTest.SchedulingScenarios.Islands.CommandHandler
{
	[TestFixture(SUT.IntradayOptimization)]
	[TestFixture(SUT.Scheduling)]
	[TestFixture(SUT.DayOffOptimization)]
	[DomainTest]
	public abstract class ResourcePlannerCommandHandlerTest : ITestInterceptor, IIsolateSystem
	{
		private readonly SUT _sut;

		public IntradayOptimizationCommandHandler IntradayOptimizationCommandHandler;
		public SchedulingCommandHandler SchedulingCommandHandler;
		public DayOffOptimizationCommandHandler DayOffOptimizationCommandHandler;
		public OptimizationPreferencesDefaultValueProvider OptimizationPreferencesProvider;
		public SchedulingOptionsProvider SchedulingOptionsProvider;
		public FakePersonRepository PersonRepository;

		protected ResourcePlannerCommandHandlerTest(SUT sut)
		{
			_sut = sut;
		}

		protected ICommandIdentifier ExecuteTarget()
		{
			return ExecuteTarget(DateOnly.Today.ToDateOnlyPeriod());
		}
		
		protected ICommandIdentifier ExecuteTarget(DateOnlyPeriod period)
		{
			return ExecuteTarget(period, PersonRepository.LoadAll());
		}
		
		protected ICommandIdentifier ExecuteTarget(DateOnlyPeriod period, TeamBlockType teamBlockType)
		{
			return ExecuteTarget(period, PersonRepository.LoadAll(), teamBlockType);
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
						SchedulingOptionsProvider.SetFromTest_LegacyDONOTUSE(null, new SchedulingOptions
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
						OptimizationPreferencesProvider.SetFromTestsOnly_LegacyDONOTUSE(new OptimizationPreferences
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
						OptimizationPreferencesProvider.SetFromTestsOnly_LegacyDONOTUSE(new OptimizationPreferences
						{
							Extra = teamBlockType.Value.CreateExtraPreferences()
						});
					}
					var doCmd = new DayOffOptimizationCommand {Period = period, AgentsToOptimize = agents};
					DayOffOptimizationCommandHandler.Execute(doCmd);
					return doCmd;
				default:
					throw new NotSupportedException();
			}
		}

		public virtual void OnBefore()
		{
			//hack to remove logon user when using "loadall" in executetarget methods
			PersonRepository.LoadAll().ToArray().ForEach(x => PersonRepository.Remove(x));
		}

		public void Isolate(IIsolate isolate)
		{
			//these are not needed when web supports team scheduling
			isolate.UseTestDouble<SchedulingOptionsProvider>().For<ISchedulingOptionsProvider>();
			isolate.UseTestDouble<OptimizationPreferencesDefaultValueProvider>().For<IOptimizationPreferencesProvider>();
			//
		}
	}
}