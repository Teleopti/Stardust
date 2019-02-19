using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public class ScheduleTagRepository : Repository<IScheduleTag>, IScheduleTagRepository
    {
#pragma warning disable 618
        public ScheduleTagRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
#pragma warning restore 618
        {
        }

				public ScheduleTagRepository(ICurrentUnitOfWork currentUnitOfWork)
					: base(currentUnitOfWork, null, null)
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