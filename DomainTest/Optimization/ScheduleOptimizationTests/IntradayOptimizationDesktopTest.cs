using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.ScheduleOptimizationTests
{
	[DomainTest]
	[Toggle(Toggles.ResourcePlanner_IntradayIslands_36939)]
	[UseEventPublisher(typeof(RunInProcessEventPublisher))]
	public class IntradayOptimizationDesktopTest : ISetup
	{
		public IIntradayOptimizationCommandHandler Target;
		public ShedulerStateHolderFiller Filler;

		[Test, Ignore("not yet fixed")]
		public void ShouldUseShiftThatCoverHigherDemand()
		{
			var agent = new Person();
			var dateOnly = new DateOnly(2015, 10, 12);

			//setup "schedulingscreen state holder"
			var schedulingScreenStateHolder = new SchedulerStateHolder(null,null,null);

			Filler.Add(schedulingScreenStateHolder);

			Target.Execute(new IntradayOptimizationCommand
			{
				Agents = new[] {agent},
				Period = new DateOnlyPeriod(dateOnly, dateOnly)
			});

			//asserta på nåt i schedulingscreenstateholder
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<ShedulerStateHolderFiller>().For<IFillSchedulerStateHolder>();
		}
	}

	public class ShedulerStateHolderFiller : IFillSchedulerStateHolder
	{
		//private SchedulerStateHolder _schedulerStateHolderFrom;

		public WebSchedulingSetupResult Fill(ISchedulerStateHolder schedulerStateHolder, DateOnlyPeriod period)
		{
			throw new System.NotImplementedException();
		}

		public void Add(SchedulerStateHolder schedulingScreenStateHolder)
		{
			//_schedulerStateHolderFrom = schedulingScreenStateHolder;
		}
	}
}