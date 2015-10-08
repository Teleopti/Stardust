using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public class KpiTargetRepository : Repository<IKpiTarget>
    {
        public KpiTargetRepository(IUnitOfWork unitOfWork) 
#pragma warning disable 618
            : base(unitOfWork)
#pragma warning restore 618
        {
        }
    }
}
