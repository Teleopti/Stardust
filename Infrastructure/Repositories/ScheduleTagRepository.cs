using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public class ScheduleTagRepository : Repository<IScheduleTag>, IScheduleTagRepository
    {
		public static ScheduleTagRepository DONT_USE_CTOR(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new ScheduleTagRepository(currentUnitOfWork, null, null);
		}

		public static ScheduleTagRepository DONT_USE_CTOR(IUnitOfWork unitOfWork)
		{
			return new ScheduleTagRepository(new ThisUnitOfWork(unitOfWork), null, null);
		}

		public ScheduleTagRepository(ICurrentUnitOfWork currentUnitOfWork, ICurrentBusinessUnit currentBusinessUnit, Lazy<IUpdatedBy> updatedBy)
			: base(currentUnitOfWork, currentBusinessUnit, updatedBy)
		{
		}

        public IList<IScheduleTag> FindAllScheduleTags()
        {
            return Session.CreateCriteria(typeof(ScheduleTag))
				.AddOrder(Order.Asc("Description"))
                .List<IScheduleTag>();
        }
    }
}