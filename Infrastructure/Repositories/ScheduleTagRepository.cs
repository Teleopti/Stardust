using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public class ScheduleTagRepository : Repository<IScheduleTag>, IScheduleTagRepository
    {
        public ScheduleTagRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public ScheduleTagRepository(IUnitOfWorkFactory unitOfWorkFactory)
            : base(unitOfWorkFactory)
        {
        }
    }
}