using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public class MultiplicatorRepository : Repository<IMultiplicator>, IMultiplicatorRepository
    {
	    public MultiplicatorRepository(IUnitOfWork unitOfWork)
#pragma warning disable 618
            : base(unitOfWork)
#pragma warning restore 618
        {
        }

	    public MultiplicatorRepository(ICurrentUnitOfWork currentUnitOfWork)
		    : base(currentUnitOfWork, null, null)
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
