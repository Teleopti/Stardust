using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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

	    public ActivityRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
	    {
	    }

        // We have to filter so we don't get the masterActivities here but at the same time activate masterActivity.Activitycollection
        public new IList<IActivity> LoadAll()
        {
            IList<IActivity> lst = base.LoadAll();
            foreach (var activity in lst)
            {
                var thisActivity = activity as IMasterActivity;
                if (thisActivity != null)
                {
                    LazyLoadingManager.Initialize(thisActivity.ActivityCollection);
                }
            }
            return lst.Where(activity => !(activity is IMasterActivity)).ToList();
        }

        public IList<IActivity> LoadAllSortByName()
        {
            return LoadAll().OrderBy(a => a.Name).ToList();
        }

        public IList<IActivity> LoadAllWithUpdatedBy()
        {
            return Session.CreateCriteria(typeof(Activity))
               .SetFetchMode("UpdatedBy", FetchMode.Join)
               .SetResultTransformer(Transformers.DistinctRootEntity)
               .List<IActivity>();
        }
    }
}