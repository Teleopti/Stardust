using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// SourceQueueRepository
    /// </summary>
    public class QueueSourceRepository : Repository<IQueueSource>, IQueueSourceRepository
    {
#pragma warning disable 618
        public QueueSourceRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
#pragma warning restore 618
        {
        }

		public QueueSourceRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
		{
		}
    }
}