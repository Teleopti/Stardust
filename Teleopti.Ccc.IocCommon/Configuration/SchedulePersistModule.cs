using Autofac;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class SchedulePersistModule : Module
	{
		private readonly bool _checkConflicts;
		private readonly IInitiatorIdentifier _initiatorIdentifier;
		private readonly IReassociateDataForSchedules _reassociateDataForSchedules;

		//todo: försök att bli av med dessa beroende - bara bool:en ska vara kvar
		private SchedulePersistModule(IInitiatorIdentifier initiatorIdentifier, IReassociateDataForSchedules reassociateDataForSchedules, bool checkConflicts)
		{
			_checkConflicts = checkConflicts;
			_initiatorIdentifier = initiatorIdentifier ?? new EmptyInitiatorIdentifier();
			_reassociateDataForSchedules = reassociateDataForSchedules ?? new nullReassociateDataForSchedules();
		}

		public static SchedulePersistModule ForScheduler(IInitiatorIdentifier initiatorIdentifier, IReassociateDataForSchedules reassociateDataForSchedules)
		{
			return new SchedulePersistModule(initiatorIdentifier, reassociateDataForSchedules, true);
		}

		public static SchedulePersistModule ForOtherModules()
		{
			return new SchedulePersistModule(null, null, false);
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ScheduleDictionaryPersister>().As<IScheduleDictionaryPersister>().SingleInstance();
			builder.RegisterType<ScheduleRangePersister>().As<IScheduleRangePersister>().SingleInstance();
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