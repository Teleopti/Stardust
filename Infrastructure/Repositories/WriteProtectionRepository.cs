using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public interface IWriteProtectionRepository : IRepository<IPersonWriteProtectionInfo>
    {
    }

    public class WriteProtectionRepository : Repository<IPersonWriteProtectionInfo>, IWriteProtectionRepository
    {
        public WriteProtectionRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public WriteProtectionRepository(IUnitOfWorkFactory unitOfWorkFactory)
            : base(unitOfWorkFactory)
        {
        }
    }
}