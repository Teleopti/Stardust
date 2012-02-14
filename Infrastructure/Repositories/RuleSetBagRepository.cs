using System.Collections.Generic;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for RuleSetBag
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-03-28
    /// </remarks>
    public class RuleSetBagRepository : Repository<IRuleSetBag>, IRuleSetBagRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RuleSetBagRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The UnitOfWork.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-28
        /// </remarks>
        public RuleSetBagRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public IEnumerable<IRuleSetBag> LoadAllWithRuleSets()
        {
            return Session.CreateCriteria(typeof (IRuleSetBag), "bag")
                .SetFetchMode("RuleSetCollection", FetchMode.Join)
                .SetResultTransformer(Transformers.DistinctRootEntity)
                .List<IRuleSetBag>();
        }
    }
}
