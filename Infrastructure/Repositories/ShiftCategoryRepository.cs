using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using NHibernate;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for ShiftCategories
    /// </summary>
    public class ShiftCategoryRepository : Repository<IShiftCategory>, IShiftCategoryRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShiftCategoryRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unitofwork</param>
        public ShiftCategoryRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public ShiftCategoryRepository(IUnitOfWorkFactory unitOfWorkFactory)
            : base(unitOfWorkFactory)
        {
        }

        public IList<IShiftCategory> FindAll()
        {
            return Session.CreateCriteria(typeof (ShiftCategory))
                .SetFetchMode("DayOfWeekJusticeValues.WrappedDictionary", FetchMode.Join)
                .SetResultTransformer(Transformers.DistinctRootEntity)
                .List<IShiftCategory>();
        }
    }
}