using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Wfm.Adherence.Domain.Configuration;

namespace Teleopti.Wfm.Adherence.Domain.Infrastructure.Repositories
{
    public class RtaRuleRepository : Repository<IRtaRule>, IRtaRuleRepository
    {
	    public RtaRuleRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
	    {
	    }
    }
}