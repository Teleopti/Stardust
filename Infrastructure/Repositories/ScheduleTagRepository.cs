using System.Collections.Generic;
using System.Linq;
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
            IList<IScheduleTag> scheduleTags = Session.CreateCriteria(typeof(ScheduleTag))
                .List<IScheduleTag>();

            return scheduleTags.OrderBy(sc => sc.Description).ToList();
        }
    }
}