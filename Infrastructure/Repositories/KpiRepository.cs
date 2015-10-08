using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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
    public class KpiRepository : Repository<IKeyPerformanceIndicator>
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
    }
}