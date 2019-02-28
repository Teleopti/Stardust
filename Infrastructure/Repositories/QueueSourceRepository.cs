using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// SourceQueueRepository
    /// </summary>
    public class QueueSourceRepository : Repository<IQueueSource>, IQueueSourceRepository
    {
		public static QueueSourceRepository DONT_USE_CTOR(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new QueueSourceRepository(currentUnitOfWork, null, null);
		}

		public static QueueSourceRepository DONT_USE_CTOR(IUnitOfWork unitOfWork)
		{
			return new QueueSourceRepository(new ThisUnitOfWork(unitOfWork), null, null);
		}

		public QueueSourceRepository(ICurrentUnitOfWork currentUnitOfWork, ICurrentBusinessUnit currentBusinessUnit, Lazy<IUpdatedBy> updatedBy)
			: base(currentUnitOfWork, currentBusinessUnit, updatedBy)
		{
		}
    }
}