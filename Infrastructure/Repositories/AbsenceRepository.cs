using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for absences
    /// </summary>
	public class AbsenceRepository : Repository<IAbsence>, IAbsenceRepository, IWriteSideRepository<IAbsence>, IProxyForId<IAbsence>
    {
        public AbsenceRepository(IUnitOfWork unitOfWork)
#pragma warning disable 618
            : base(unitOfWork)
#pragma warning restore 618
        {
        }

	    public AbsenceRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork, null, null)
	    {
	    }

	    /// <summary>
	    /// Loads the All absences by sorting it's by name.
	    /// </summary>
	    /// <returns></returns>
	    /// <remarks>
	    /// Created by: Sumedah
	    /// Created date: 2008-10-05
	    /// </remarks>
	    public IEnumerable<IAbsence> LoadAllSortByName()
        {
            IList<IAbsence> retList = Session.CreateCriteria(typeof(Domain.Scheduling.Absence))
                        .AddOrder(Order.Asc("Description.Name"))
                        .List<IAbsence>();

            return retList;
        }

        /// <summary>
        /// Loads Requestable absences
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-10-07
        /// </remarks>
        public IList<IAbsence> LoadRequestableAbsence()
        {
            IList<IAbsence> retList = Session.CreateCriteria(typeof(Domain.Scheduling.Absence))
                        .Add(Restrictions.Eq("Requestable", true))
                        .AddOrder(Order.Asc("Description.Name"))
                        .List<IAbsence>();

            return retList;
        }

		public IList<IAbsence> FindAbsenceTrackerUsedByPersonAccount()
        {
            return Session.GetNamedQuery("FindAbsenceTrackerUsedByPersonAccount")
                            .List<IAbsence>();
        }

	    public IAbsence LoadAggregate(Guid id) { return Load(id); }
    }
}