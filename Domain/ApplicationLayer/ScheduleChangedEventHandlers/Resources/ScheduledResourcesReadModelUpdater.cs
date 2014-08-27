using System;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Resources
{
	public class ScheduledResourcesReadModelUpdater :
		IScheduledResourcesReadModelUpdater
	{
		private readonly IScheduledResourcesReadModelPersister _persister;
		private readonly IMessageBrokerSender _messageBroker;
		private readonly IEventSyncronization _eventSyncronization;

		public ScheduledResourcesReadModelUpdater(IScheduledResourcesReadModelPersister persister, IMessageBrokerSender messageBroker, IEventSyncronization eventSyncronization)
		{
			_persister = persister;
			_messageBroker = messageBroker;
			_eventSyncronization = eventSyncronization;
		}

		public void Update(string dataSource, Guid businessUnitId, Action<IScheduledResourcesReadModelUpdaterActions> action)
		{
			var actions = new actions(_persister);
			action.Invoke(actions);
			_eventSyncronization.WhenDone(() =>
				{
					_messageBroker.Send(dataSource, businessUnitId, actions.LowestDateTime, actions.HighestDateTime, Guid.Empty, Guid.Empty, typeof(IScheduledResourcesReadModel), DomainUpdateType.NotApplicable, null);
				});
		}

		private class actions : IScheduledResourcesReadModelUpdaterActions
		{
			private readonly IScheduledResourcesReadModelPersister _persister;

			public DateTime LowestDateTime;
			public DateTime HighestDateTime;

			public actions(IScheduledResourcesReadModelPersister persister)
			{
				_persister = persister;
			}

			public void AddResource(ResourceLayer resourceLayer, SkillCombination combination)
			{
				fetchPeriodInformation(resourceLayer);

				var resourceId = _persister.AddResources(resourceLayer.PayloadId, resourceLayer.RequiresSeat,
				                                         combination.Key, resourceLayer.Period,
				                                         resourceLayer.Resource, 1);
				foreach (var skillEfficiency in combination.SkillEfficiencies)
				{
					_persister.AddSkillEfficiency(resourceId, skillEfficiency.Key, skillEfficiency.Value);
				}
			}

			public void RemoveResource(ResourceLayer resourceLayer, SkillCombination combination)
			{
				fetchPeriodInformation(resourceLayer);

				var resourceId = _persister.RemoveResources(resourceLayer.PayloadId, combination.Key,
				                                            resourceLayer.Period, resourceLayer.Resource, 1);
				if (!resourceId.HasValue) return;

				foreach (var skillEfficiency in combination.SkillEfficiencies)
				{
					_persister.RemoveSkillEfficiency(resourceId.Value, skillEfficiency.Key, skillEfficiency.Value);
				}
			}

			private void fetchPeriodInformation(ResourceLayer resourceLayer)
			{
				if (LowestDateTime == DateTime.MinValue || LowestDateTime > resourceLayer.Period.StartDateTime)
					LowestDateTime = resourceLayer.Period.StartDateTime;
				if (HighestDateTime == DateTime.MinValue || HighestDateTime < resourceLayer.Period.EndDateTime)
					HighestDateTime = resourceLayer.Period.EndDateTime;
			}


		}

	}
}