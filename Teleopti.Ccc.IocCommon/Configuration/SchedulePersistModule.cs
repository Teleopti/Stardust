using Autofac;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling;
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
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ScheduleDictionaryPersister>().As<IScheduleDictionaryPersister>().SingleInstance();
			builder.RegisterType<ScheduleRangePersister>().As<IScheduleRangePersister>().SingleInstance();
			builder.RegisterType<KeepScheduleEvents>().As<IClearScheduleEvents>().SingleInstance();
			builder.RegisterGeneric(typeof (DifferenceEntityCollectionService<>)).As(typeof (IDifferenceCollectionService<>)).SingleInstance();
			builder.RegisterType<NoScheduleRangeConflictCollector>().As<IScheduleRangeConflictCollector>().SingleInstance();
			builder.RegisterType<ScheduleDifferenceSaver>().As<IScheduleDifferenceSaver>().SingleInstance();
			builder.RegisterType<LazyLoadingManagerWrapper>().As<ILazyLoadingManager>().SingleInstance();
			builder.RegisterType<EmptyInitiatorIdentifier>().As<IInitiatorIdentifier>().SingleInstance(); // shouldnt be registered at all, inject ICurrentInitiatorIdentifier!
			builder.RegisterType<dontReassociateDataForSchedules>().As<IReassociateDataForSchedules>().SingleInstance();
		}

		private class dontReassociateDataForSchedules : IReassociateDataForSchedules
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