using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

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
		                                                        		typeof (ISkill),
		                                                        		typeof (IPerson)
		                                                        	};

		public SettingsForPersonPeriodChangedEventPublisher(IEventPopulatingPublisher eventsPublisher)
		{
			_eventsPublisher = eventsPublisher;
		}

		public void AfterCompletion(IEnumerable<IRootChangeInfo> modifiedRoots)
		{
            var affectedInterfaces = from r in modifiedRoots
                                     from i in r.Root.GetType().GetInterfaces()
                                     select i;

            if (affectedInterfaces.Any(t => _triggerInterfaces.Contains(t)))
            {
				var notPerson = (from p in modifiedRoots where !(p.Root is IPerson) select p.Root).ToList();
				foreach (var notpersonList in notPerson.Batch(25))
				{
					var idsChanged = notpersonList.Select(p => ((IAggregateRoot) p).Id.GetValueOrDefault()).ToArray();
                    var personPeriodCollectionChangedEvent = new SettingsForPersonPeriodChangedEvent();
					personPeriodCollectionChangedEvent.SetIdCollection(idsChanged);
					_eventsPublisher.Publish(personPeriodCollectionChangedEvent);
				}
            }
        }
    }
}