using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Main
{
	public static class EntityEventAggregator
	{
		public static event EventHandler<EntitiesUpdatedEventArgs> EntitiesNeedsRefresh;

		public static void TriggerEntitiesNeedRefresh(object sender, IEnumerable<IRootChangeInfo> updatedEntities)
		{
		    IList<IAggregateRoot> aggregateRoots = updatedEntities.Select(updatedEntity => (IAggregateRoot)updatedEntity.Root).ToList();
		    TriggerEntitiesNeedRefresh(sender, aggregateRoots);

		    //if (EntitiesNeedsRefresh != null && updatedEntities != null)
		    //{
		    //    var multicast = EntitiesNeedsRefresh as MulticastDelegate;
		    //    var targets = multicast.GetInvocationList();
		    //    var targetsExcludingSender = (from t in targets where t.Target != sender select t).ToArray();
		    //    if (!targets.Any())
		    //        return;

		    //    var entitiesPerType = (from e in updatedEntities
		    //                       group e by e.Root.GetType() into g
		    //                       select g).ToArray();

		    //    foreach (var entities in entitiesPerType)
		    //    {
		    //        var ids = (from e in entities select ((IAggregateRoot)e.Root).Id.Value).ToArray();
		    //        var eventArgs = new EntitiesUpdatedEventArgs { EntityType = entities.Key, UpdatedIds = ids };				   

		    //        targetsExcludingSender.ForEach(t => t.Method.Invoke(t.Target, new[] { sender, eventArgs }));
		    //    }
		    //}
		}

        public static void TriggerEntitiesNeedRefresh(object sender, IEnumerable<IAggregateRoot> updatedEntities)
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
                                       group e by e.GetType() into g
                                       select g).ToArray();

                foreach (var entities in entitiesPerType)
                {
                    var ids = (from e in entities select e.Id.Value).ToArray();
                    var eventArgs = new EntitiesUpdatedEventArgs { EntityType = entities.Key, UpdatedIds = ids };

                    targetsExcludingSender.ForEach(t => t.Method.Invoke(t.Target, new[] { sender, eventArgs }));
                }
            }
        }
	}
}
