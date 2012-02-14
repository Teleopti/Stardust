using System.Collections.Generic;
using NHibernate.Criterion;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for absences
    /// </summary>
    public class AbsenceRepository : Repository<IAbsence>, IAbsenceRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AbsenceRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unitofwork</param>
        public AbsenceRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public AbsenceRepository(IUnitOfWorkFactory unitOfWorkFactory)
            : base(unitOfWorkFactory)
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
        public IList<IAbsence> LoadAllSortByName()
        {
            IList<IAbsence> retList = Session.CreateCriteria(typeof(Absence))
                        .AddOrder(Order.Asc("Description.Name"))
                        .SetResultTransformer(Transformers.DistinctRootEntity)
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
            IList<IAbsence> retList = Session.CreateCriteria(typeof(Absence))
                        .Add(Expression.Eq("Requestable", true))
                        .AddOrder(Order.Asc("Description.Name"))
                        .SetResultTransformer(Transformers.DistinctRootEntity)
                        .List<IAbsence>();

            return retList;
        }

        public IList<IAbsence> FindAbsenceTrackerUsedByPersonAccount()
        {
            return Session.GetNamedQuery("FindAbsenceTrackerUsedByPersonAccount")
                            .List<IAbsence>();
        }
    }
}