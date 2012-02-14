using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for KpiTarget
    /// </summary>
    /// /// 
    /// <remarks>
    ///  Created by: Ola
    ///  Created date: 2008-04-07    
    /// /// </remarks>
    public class KpiTargetRepository : Repository<IKpiTarget>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KpiTargetRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unitofwork</param>
        public KpiTargetRepository(IUnitOfWork unitOfWork) 
            : base(unitOfWork)
        {
        }
    }
}
