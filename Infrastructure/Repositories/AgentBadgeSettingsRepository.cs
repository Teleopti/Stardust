using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public class AgentBadgeSettingsRepository : Repository<AgentBadgeThresholdSettings>
    {
	    public AgentBadgeSettingsRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
	    {
	    }

	    public AgentBadgeSettingsRepository(IUnitOfWorkFactory unitOfWorkFactory) : base(unitOfWorkFactory)
	    {
	    }

	    public AgentBadgeSettingsRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
	    {
	    }


    }
}
