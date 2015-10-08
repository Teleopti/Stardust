#region Imports

using System.Collections.Generic;
using NHibernate.Criterion;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

#endregion

namespace Teleopti.Ccc.Infrastructure.Repositories
{

    /// <summary>
    /// Represents MultiplicatorRepository.
    /// </summary>
    public class MultiplicatorRepository : Repository<IMultiplicator>, IMultiplicatorRepository
    {

        #region Methods - Instance Member

        #region Constructor - (2)
        public MultiplicatorRepository(IUnitOfWork unitOfWork)
#pragma warning disable 618
            : base(unitOfWork)
#pragma warning restore 618
        {
        }

				public MultiplicatorRepository(ICurrentUnitOfWork currentUnitOfWork)
					: base(currentUnitOfWork)
	    {
		    
	    }
        #endregion

        #region Overrides - (1)

        #endregion

        #region Methods - Instance Member - MultiplicatorRepository Members
        /// <summary>
        /// Loads the name of all sort by.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2009-01-08
        /// </remarks>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2009-01-08
        /// </remarks>
        public IList<IMultiplicator> LoadAllSortByName()
        {
            IList<IMultiplicator> retList = Session.CreateCriteria(typeof(Multiplicator))
                       .AddOrder(Order.Asc("Description.Name"))
                       .SetResultTransformer(Transformers.DistinctRootEntity)
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
        #endregion

        #endregion

    }

}
