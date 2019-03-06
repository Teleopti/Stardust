using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public class MultiplicatorRepository : Repository<IMultiplicator>, IMultiplicatorRepository
    {
		public static MultiplicatorRepository DONT_USE_CTOR(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new MultiplicatorRepository(currentUnitOfWork, null, null);
		}

		public static MultiplicatorRepository DONT_USE_CTOR(IUnitOfWork unitOfWork)
		{
			return new MultiplicatorRepository(new ThisUnitOfWork(unitOfWork), null, null);
		}

		public MultiplicatorRepository(ICurrentUnitOfWork currentUnitOfWork, ICurrentBusinessUnit currentBusinessUnit, Lazy<IUpdatedBy> updatedBy)
		    : base(currentUnitOfWork, currentBusinessUnit, updatedBy)
	    {
	    }
		
        public IList<IMultiplicator> LoadAllSortByName()
        {
            IList<IMultiplicator> retList = Session.CreateCriteria(typeof(Multiplicator))
                       .AddOrder(Order.Asc("Description.Name"))
                       .List<IMultiplicator>();

            return retList;
        }
		
        /// <summary>
        /// Loads a list of <see cref="IMultiplicator"/> s by definition type and sort by name.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public IList<IMultiplicator> LoadAllByTypeAndSortByName(MultiplicatorType type)
        {
            IList<IMultiplicator> retList = Session.CreateCriteria(typeof (Multiplicator))
                .AddOrder(Order.Asc("Description.Name"))
                .Add(Restrictions.Eq("MultiplicatorType", type))
                .List<IMultiplicator>();
            
            return retList;
        }
    }
}
