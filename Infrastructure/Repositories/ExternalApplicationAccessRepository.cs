using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class ExternalApplicationAccessRepository : Repository<IExternalApplicationAccess>
    {
	    public ExternalApplicationAccessRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
	    {
	    }
    }
}