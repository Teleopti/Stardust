using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for activities
    /// </summary>
    public class ActivityRepository : Repository<IActivity>, IActivityRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unitofwork</param>
        public ActivityRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public ActivityRepository(IUnitOfWorkFactory unitOfWorkFactory)
            : base(unitOfWorkFactory)
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
            IList<IActivity> retList = LoadAll();
            var lst = from a in retList orderby a.Name select a;
            return new List<IActivity>(lst);
            
        }
    }
}