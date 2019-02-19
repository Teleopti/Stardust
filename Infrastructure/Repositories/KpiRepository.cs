using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for KeyPerformanceIndicator
    /// </summary>
    /// /// 
    /// <remarks>
    ///  Created by: Ola
    ///  Created date: 2008-04-07    
    /// /// </remarks>
    public class KpiRepository : Repository<IKeyPerformanceIndicator>, IKpiRepository
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="KpiRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unitofwork</param>
        public KpiRepository(IUnitOfWork unitOfWork) 
#pragma warning disable 618
            : base(unitOfWork)
#pragma warning restore 618
        {
        }

		public KpiRepository(ICurrentUnitOfWork currentUnitOfWork)
				: base(currentUnitOfWork, null, null)
	    {
		}
	}
}