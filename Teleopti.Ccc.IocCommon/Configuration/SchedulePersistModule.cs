using System;
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
		private readonly IMessageBrokerIdentifier _messageBrokerIdentifier;
		private readonly IOwnMessageQueue _ownMessageQueue;

		//todo: försök att bli av med dessa beroende - bara bool:en ska vara kvar
		public SchedulePersistModule(IMessageBrokerIdentifier messageBrokerIdentifier, IOwnMessageQueue ownMessageQueue, bool checkConflicts)
		{
			_checkConflicts = checkConflicts;
			_messageBrokerIdentifier = messageBrokerIdentifier ?? new EmptyMessageBrokerIdentifier();
			_ownMessageQueue = ownMessageQueue ?? new nullOwnMessageQueue();
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
			builder.Register(c => _messageBrokerIdentifier).As<IMessageBrokerIdentifier>();
			builder.Register(c => _ownMessageQueue).As<IOwnMessageQueue>();
		}

		private class nullOwnMessageQueue : IOwnMessageQueue
		{
			public void ReassociateDataWithAllPeople()
			{
			}

			public void NotifyMessageQueueSizeChange()
			{
			}
		}
	}
}