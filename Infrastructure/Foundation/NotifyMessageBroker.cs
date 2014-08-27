using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Events;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
    /// <summary>
    /// Sends messages to message broker of domain entity changes
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-06-10
    /// </remarks>
    public class NotifyMessageBroker
    {
        private readonly IMessageBroker _messageBroker;
	    private readonly EventMessageFactory _eventMessageFactory;

	    public NotifyMessageBroker(IMessageBroker messageBroker)
        {
	        _messageBroker = messageBroker;
	        _eventMessageFactory = new EventMessageFactory();
        }

	    public void Notify(Guid moduleId, IEnumerable<IRootChangeInfo> rootModifications)
        {
            if (_messageBroker==null || !_messageBroker.IsConnected) return;
            
			var eventMessages = new List<IEventMessage>();
            foreach (var change in rootModifications)
            {
                if (!MessageFilterManager.Instance.HasType(change.Root.GetType())) continue;

                var rootBrokerConditions = change.Root as IAggregateRootBrokerConditions;
                if (rootBrokerConditions != null)
                {
                    if (rootBrokerConditions.SendChangeOverMessageBroker())
                        eventMessages.Add(CreateEventMessage(change, moduleId));
                    continue;
                }


                eventMessages.Add(CreateEventMessage(change, moduleId));

                var provideCustomChangeInfo = change.Root as IProvideCustomChangeInfo;
                if (provideCustomChangeInfo == null) continue;

                var changes = provideCustomChangeInfo.CustomChanges(change.Status);
                eventMessages.AddRange(changes.Select(rootChangeInfo => CreateEventMessage(rootChangeInfo, moduleId)));
            }
            if (eventMessages.Count > 0)
            {
				var businessUnitId = Guid.Empty;
				var dataSourceName = string.Empty;
            	var currentPrincipal = TeleoptiPrincipal.Current;
				if (currentPrincipal!=null)
				{
					var identity = currentPrincipal.Identity as ITeleoptiIdentity;
					if (identity != null)
					{
						var businessUnit = identity.BusinessUnit;
						if (businessUnit != null)
						{
							businessUnitId = businessUnit.Id.GetValueOrDefault();
						}
						if (UnitOfWorkFactory.Current != null)
						{
							dataSourceName = UnitOfWorkFactory.Current.Name;
						}
					}
				}
            	_messageBroker.Send(dataSourceName, businessUnitId, eventMessages.ToArray());
            }
        }

        protected IEventMessage CreateEventMessage(IRootChangeInfo change, Guid moduleId)
        {
            IMainReference changeWithRoot = change.Root as IMainReference; 
            IPeriodized periodRoot = change.Root as IPeriodized;
        	Guid rootId = extractId(change.Root);
            Type rootType = change.Root.GetType();
            IEventMessage eventMessage;
            if (periodRoot == null)
            {
                if(changeWithRoot==null)
					eventMessage = _eventMessageFactory.CreateEventMessage(moduleId, rootId, rootType, change.Status);
                else
					eventMessage = _eventMessageFactory.CreateEventMessage(moduleId, 
                                                                    changeWithRoot.MainRoot.Id.Value,
                                                                    realType(changeWithRoot.MainRoot),
                                                                    rootId, 
                                                                    rootType, 
                                                                    change.Status);
            }
            else
            {
                DateTimePeriod period = periodRoot.Period;
                if(changeWithRoot==null)
					eventMessage = _eventMessageFactory.CreateEventMessage(period.StartDateTime, period.EndDateTime, moduleId, rootId, rootType, change.Status);
                else
					eventMessage = _eventMessageFactory.CreateEventMessage(period.StartDateTime, 
                                                                     period.EndDateTime,
                                                                     moduleId, 
                                                                     changeWithRoot.MainRoot.Id.Value,
                                                                     realType(changeWithRoot.MainRoot),
                                                                     rootId, 
                                                                     rootType, 
                                                                     change.Status);
            }
            return eventMessage;
        }

    	private static Guid extractId(object root)
    	{
    		var entity = root as IEntity;
			if (entity != null) return entity.Id.GetValueOrDefault();

    		var custom = root as ICustomChangedEntity;
			if (custom != null) return custom.Id.GetValueOrDefault();

    		return Guid.Empty;
    	}

    	private static Type realType(object possibleProxy)
        {
            return NHibernateUtil.GetClass(possibleProxy);
        }
    }
}
