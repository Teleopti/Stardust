using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Main
{
	public static class EntityEventAggregator
	{
		public static event EventHandler<EntitiesUpdatedEventArgs> EntitiesNeedsRefresh;

		public static void TriggerEntitiesNeedRefresh(object sender, IEnumerable<IRootChangeInfo> updatedEntities)
		{
		    var aggregateRoots = updatedEntities.Select(updatedEntity => new UpdatedEntities{UpdatedEntity = (IAggregateRoot) updatedEntity.Root,EntityStatus = updatedEntity.Status}).ToList();
            TriggerEntitiesNeedRefresh(sender, aggregateRoots);
		}

        public static void TriggerEntitiesNeedRefresh(object sender, IEnumerable<UpdatedEntities> updatedEntities)
        {
        	var handler = EntitiesNeedsRefresh;
            if (handler != null && updatedEntities != null)
            {
                var multicast = handler as MulticastDelegate;
                var targets = multicast.GetInvocationList();
                var targetsExcludingSender = (from t in targets where t.Target != sender select t).ToArray();
                if (!targets.Any())
                    return;

                var entitiesPerType = (from e in updatedEntities
                                       group e by e.UpdatedEntity.GetType() into g
                                       select g).ToArray();

                foreach (var entities in entitiesPerType)
                {
                    var entitiesPerUpdateType = (from e in entities
                                           group e by e.EntityStatus into g
                                           select g).ToArray();

                    foreach(var entity in entitiesPerUpdateType)
                    {
                        var ids = (from e in entity select e.UpdatedEntity.Id.Value).ToArray();
                        var eventArgs = new EntitiesUpdatedEventArgs { EntityType = entities.Key, UpdatedIds = ids, EntityStatus = entity.Key };
                        targetsExcludingSender.ForEach(t => t.Method.Invoke(t.Target, new[] { sender, eventArgs }));
                    }
                }
            }
        }
	}
}
