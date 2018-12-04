using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using NHibernate;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for ShiftCategories
    /// </summary>
    public class ShiftCategoryRepository : Repository<IShiftCategory>, IShiftCategoryRepository
    {
#pragma warning disable 618
        public ShiftCategoryRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
#pragma warning restore 618
        {
        }

				public ShiftCategoryRepository(ICurrentUnitOfWork currentUnitOfWork)
					: base(currentUnitOfWork)
	    {
		    
	    }

        public IList<IShiftCategory> FindAll()
        {
            return Session.CreateCriteria(typeof (ShiftCategory))
                .Fetch("DayOfWeekJusticeValues.WrappedDictionary")
                .SetResultTransformer(Transformers.DistinctRootEntity)
                .List<IShiftCategory>();
        }
    }
}