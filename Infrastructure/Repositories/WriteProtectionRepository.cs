using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public interface IWriteProtectionRepository : IRepository<IPersonWriteProtectionInfo>
    {
    }

    public class WriteProtectionRepository : Repository<IPersonWriteProtectionInfo>, IWriteProtectionRepository
    {
				public WriteProtectionRepository(ICurrentUnitOfWork currentUnitOfWork)
					: base(currentUnitOfWork, null, null)
	    {
		    
	    }
    }
}