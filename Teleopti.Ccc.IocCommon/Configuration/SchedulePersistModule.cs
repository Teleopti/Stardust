using Autofac;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class SchedulePersistModule : Module
	{
		private readonly bool _checkConflicts;
		private readonly IToggleManager _toggleManager;
		private readonly IInitiatorIdentifier _initiatorIdentifier;
		private readonly IReassociateDataForSchedules _reassociateDataForSchedules;

		//todo: försök att bli av med dessa beroende - bara bool:en ska vara kvar
		private SchedulePersistModule(IInitiatorIdentifier initiatorIdentifier, IReassociateDataForSchedules reassociateDataForSchedules, bool checkConflicts, IToggleManager toggleManager)
		{
			_checkConflicts = checkConflicts;
			_toggleManager = toggleManager ?? new TrueToggleManager();
			_initiatorIdentifier = initiatorIdentifier ?? new EmptyInitiatorIdentifier();
			_reassociateDataForSchedules = reassociateDataForSchedules ?? new nullReassociateDataForSchedules();
		}

		public static SchedulePersistModule ForScheduler(IInitiatorIdentifier initiatorIdentifier, IReassociateDataForSchedules reassociateDataForSchedules, IToggleManager toggleManager)
		{
			return new SchedulePersistModule(initiatorIdentifier, reassociateDataForSchedules, true, toggleManager);
		}

		public static SchedulePersistModule ForOtherModules()
		{
			return new SchedulePersistModule(null, null, false, null);
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ScheduleDictionaryPersister>().As<IScheduleDictionaryPersister>().SingleInstance();
			builder.RegisterType<ScheduleRangePersister>().As<IScheduleRangePersister>().SingleInstance();
			if (_toggleManager.IsEnabled(Toggles.MessageBroker_SchedulingScreenMailbox_32733))
				builder.RegisterType<AggregatedScheduleChangeSender>().As<IAggregatedScheduleChangeSender>().SingleInstance();
			else
				builder.RegisterType<EmptyAggregatedScheduleChangeSender>().As<IAggregatedScheduleChangeSender>().SingleInstance();
			builder.RegisterGeneric(typeof(DifferenceEntityCollectionService<>)).As(typeof(IDifferenceCollectionService<>)).SingleInstance();
			if (_checkConflicts)
			{
				builder.RegisterType<ScheduleRangeConflictCollector>().As<IScheduleRangeConflictCollector>().SingleInstance();				
			}
			else
			{
				builder.RegisterType<NoScheduleRangeConflictCollector>().As<IScheduleRangeConflictCollector>().SingleInstance();
			}
			builder.RegisterType<ScheduleDifferenceSaver>().As<IScheduleDifferenceSaver>().SingleInstance();
			builder.RegisterType<LazyLoadingManagerWrapper>().As<ILazyLoadingManager>().SingleInstance();
			builder.Register(c => _initiatorIdentifier).As<IInitiatorIdentifier>();
			builder.Register(c => _reassociateDataForSchedules).As<IReassociateDataForSchedules>();
		}

		private class nullReassociateDataForSchedules : IReassociateDataForSchedules
		{
			public void ReassociateDataForAllPeople()
			{
			}

			public void ReassociateDataFor(IPerson person)
			{
			}
		}
	}
}