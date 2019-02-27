using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Wfm.Adherence.Configuration.Repositories
{
    public class RtaRuleRepository : Repository<IRtaRule>, IRtaRuleRepository
    {
		public static RtaRuleRepository DONT_USE_CTOR(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new RtaRuleRepository(currentUnitOfWork);
		}

		public RtaRuleRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
	    {
	    }
    }
}