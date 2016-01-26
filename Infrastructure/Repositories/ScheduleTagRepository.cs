using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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
					: base(currentUnitOfWork)
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