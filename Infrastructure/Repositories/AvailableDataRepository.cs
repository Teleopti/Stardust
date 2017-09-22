using System.Collections;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public class AvailableDataRepository : Repository<IAvailableData>,IAvailableDataRepository
    {
        public AvailableDataRepository(IUnitOfWork unitOfWork)
#pragma warning disable 618
            : base(unitOfWork)
#pragma warning restore 618
        {
        }

	    public AvailableDataRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
	    {
	    }

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

            var businessUnitRepository = new BusinessUnitRepository(UnitOfWork);
            foreach (IBusinessUnit businessUnit in availableData.AvailableBusinessUnits)
            {
                if (!UnitOfWork.Contains(businessUnit))
                UnitOfWork.Reassociate(businessUnit);
                businessUnitRepository.LoadHierarchyInformation(businessUnit);
            }

            foreach (ISite site in availableData.AvailableSites)
            {
                if (!UnitOfWork.Contains(site))
                UnitOfWork.Reassociate(site);
                if (!LazyLoadingManager.IsInitialized(site.TeamCollection))
                    LazyLoadingManager.Initialize(site.TeamCollection);
            }

            return availableData;
        }
    }
}
