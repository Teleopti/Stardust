using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for alarm types
    /// </summary>
    public class RtaRuleRepository : Repository<IRtaRule>, IRtaRuleRepository
    {
	    public RtaRuleRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
	    {
	    }
    }
}