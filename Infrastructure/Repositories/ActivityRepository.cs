using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for activities
    /// </summary>
	public class ActivityRepository : Repository<IActivity>, IActivityRepository, IProxyForId<IActivity>
    {
#pragma warning disable 618
        public ActivityRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
#pragma warning restore 618
        {
        }

	    public ActivityRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork, null, null)
	    {
	    }

        // We have to filter so we don't get the masterActivities here but at the same time activate masterActivity.Activitycollection
        public override IEnumerable<IActivity> LoadAll()
        {
            var lst = base.LoadAll().ToArray();
			var masterActivities = lst.OfType<IMasterActivity>().ToArray();
			foreach (var activity in masterActivities)
			{
				LazyLoadingManager.Initialize(activity.ActivityCollection);
			}
            return lst.Except(masterActivities).ToList();
        }

        public IList<IActivity> LoadAllSortByName()
        {
            return LoadAll().OrderBy(a => a.Name).ToList();
        }

        public IList<IActivity> LoadAllWithUpdatedBy()
        {
            return Session.CreateCriteria(typeof(Activity))
               .Fetch("UpdatedBy")
               .SetResultTransformer(Transformers.DistinctRootEntity)
               .List<IActivity>();
        }
    }
}