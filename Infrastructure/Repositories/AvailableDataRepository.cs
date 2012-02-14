#region

using System.Collections;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

#endregion

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for SiteRepository
    /// </summary>
    public class AvailableDataRepository : Repository<IAvailableData>,IAvailableDataRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AvailableDataRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unitofwork</param>
        public AvailableDataRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        /// <summary>
        /// Reads all available data
        /// </summary>
        /// <returns>The AvailableData list.</returns>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-05-01
        /// </remarks>
        public virtual IList<IAvailableData> LoadAllAvailableData()
        {
            var q1 = Session.CreateCriteria(typeof(AvailableData))
                .SetFetchMode("ApplicationRole", FetchMode.Join)
                .SetResultTransformer(Transformers.DistinctRootEntity);
            var q2 = Session.CreateCriteria(typeof(AvailableData))
                .SetFetchMode("AvailableSites", FetchMode.Join);
            var q3 = Session.CreateCriteria(typeof(AvailableData))
                .SetFetchMode("AvailableTeams", FetchMode.Join);
            var q4 = Session.CreateCriteria(typeof(AvailableData))
                .SetFetchMode("AvailablePersons", FetchMode.Join);
            var q5 = Session.CreateCriteria(typeof(AvailableData))
                .SetFetchMode("AvailableBusinessUnits", FetchMode.Join);

            IList res = Session.CreateMultiCriteria()
                            .Add(q1)
                            .Add(q2)
                            .Add(q3)
                            .Add(q4)
                            .Add(q5)
                            .List();

            return new List<IAvailableData>(CollectionHelper.ToDistinctGenericCollection<IAvailableData>(res[0]));
        }

        /// <summary>
        /// Loads all collections in available data.
        /// </summary>
        /// <param name="availableData">The available data.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 11/14/2008
        /// </remarks>
        public IAvailableData LoadAllCollectionsInAvailableData(IAvailableData availableData)
        {
            if (!UnitOfWork.Contains(availableData))
                UnitOfWork.Reassociate(availableData);

            if (!LazyLoadingManager.IsInitialized(availableData.AvailableBusinessUnits))
                LazyLoadingManager.Initialize(availableData.AvailableBusinessUnits);
            if (!LazyLoadingManager.IsInitialized(availableData.AvailableSites))
                LazyLoadingManager.Initialize(availableData.AvailableSites);
            if (!LazyLoadingManager.IsInitialized(availableData.AvailableTeams))
                LazyLoadingManager.Initialize(availableData.AvailableTeams);

            BusinessUnitRepository businessUnitRepository = new BusinessUnitRepository(UnitOfWork);
            foreach (IBusinessUnit businessUnit in availableData.AvailableBusinessUnits)
            {
                if (!base.UnitOfWork.Contains(businessUnit))
                base.UnitOfWork.Reassociate(businessUnit);
                businessUnitRepository.LoadHierarchyInformation(businessUnit);
            }

            foreach (ISite site in availableData.AvailableSites)
            {
                if (!base.UnitOfWork.Contains(site))
                base.UnitOfWork.Reassociate(site);
                if (!LazyLoadingManager.IsInitialized(site.TeamCollection))
                    LazyLoadingManager.Initialize(site.TeamCollection);
            }

            return availableData;
        }

    }
}
