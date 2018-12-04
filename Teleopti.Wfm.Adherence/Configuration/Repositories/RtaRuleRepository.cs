using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Wfm.Adherence.Configuration.Repositories
{
    public class RtaRuleRepository : Repository<IRtaRule>, IRtaRuleRepository
    {
	    public RtaRuleRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
	    {
	    }
    }
}