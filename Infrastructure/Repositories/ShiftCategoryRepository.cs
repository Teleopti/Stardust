using System;
using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using NHibernate;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for ShiftCategories
    /// </summary>
    public class ShiftCategoryRepository : Repository<IShiftCategory>, IShiftCategoryRepository
    {
		public static ShiftCategoryRepository DONT_USE_CTOR(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new ShiftCategoryRepository(currentUnitOfWork, null, null);
		}

		public static ShiftCategoryRepository DONT_USE_CTOR(IUnitOfWork unitOfWork)
		{
			return new ShiftCategoryRepository(new ThisUnitOfWork(unitOfWork), null, null);
		}
		
		public ShiftCategoryRepository(ICurrentUnitOfWork currentUnitOfWork, ICurrentBusinessUnit currentBusinessUnit, Lazy<IUpdatedBy> updatedBy)
			: base(currentUnitOfWork, currentBusinessUnit, updatedBy)
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