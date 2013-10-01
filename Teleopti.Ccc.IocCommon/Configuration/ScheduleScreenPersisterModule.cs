using Autofac;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.IocCommon.Configuration
{
    public class ScheduleScreenPersisterModule : Module
    {

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<LazyLoadingManagerWrapper>().As<ILazyLoadingManager>().SingleInstance();
            builder.RegisterType<ScheduleDictionaryModifiedCallback>().As<IScheduleDictionaryModifiedCallback>().SingleInstance();
            builder.RegisterType<ScheduleDictionaryConflictCollector>().As<IScheduleDictionaryConflictCollector>().SingleInstance();
            builder.RegisterType<PersonAbsenceAccountConflictCollector>().As<IPersonAbsenceAccountConflictCollector>().SingleInstance();
            builder.RegisterType<ScheduleDictionarySaver>().As<IScheduleDictionarySaver>().SingleInstance();
			builder.RegisterType<ScheduleDictionaryBatchPersister>().As<IScheduleDictionaryBatchPersister>();
			builder.RegisterGeneric(typeof(DifferenceEntityCollectionService<>)).As(typeof(IDifferenceCollectionService<>)).SingleInstance();
			builder.RegisterType<ScheduleScreenRetryingPersister>().As<IScheduleScreenPersister>();

        }
    }
}