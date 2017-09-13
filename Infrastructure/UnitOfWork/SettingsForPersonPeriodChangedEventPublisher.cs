using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class SettingsForPersonPeriodChangedEventPublisher : ITransactionHook
    {
		private readonly IEventPopulatingPublisher _eventsPublisher;

	    private readonly IEnumerable<Type> _triggerInterfaces = new List<Type>
		                                                        	{
		                                                        		typeof (ITeam),
		                                                        		typeof (ISite),
		                                                        		typeof (IContract),
		                                                        		typeof (IContractSchedule),
		                                                        		typeof (IPartTimePercentage),
		                                                        		typeof (IRuleSetBag),
		                                                        		typeof (ISkill)
		                                                        	};

		public SettingsForPersonPeriodChangedEventPublisher(IEventPopulatingPublisher eventsPublisher)
		{
			_eventsPublisher = eventsPublisher;
		}

		public void AfterCompletion(IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			var affectedRoots = from r in modifiedRoots
				where r.Root.GetType().GetInterfaces().Any(t => _triggerInterfaces.Contains(t))
				select r;

			foreach (var notpersonList in affectedRoots.Batch(25))
			{
				var idsChanged = notpersonList.Select(p => ((IAggregateRoot) p.Root).Id.GetValueOrDefault()).ToArray();
				var settingsForPersonPeriodChangedEvent = new SettingsForPersonPeriodChangedEvent();
				settingsForPersonPeriodChangedEvent.SetIdCollection(idsChanged);
				_eventsPublisher.Publish(settingsForPersonPeriodChangedEvent);
			}
		}
    }
}