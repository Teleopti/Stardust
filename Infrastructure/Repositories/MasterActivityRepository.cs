using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for masteractivities
    /// </summary>
    public class MasterActivityRepository : Repository<IMasterActivity>, IMasterActivityRepository
    {
        public MasterActivityRepository(IUnitOfWork unitOfWork)
#pragma warning disable 618
            : base(unitOfWork)
#pragma warning restore 618
        {
        }
    }
}