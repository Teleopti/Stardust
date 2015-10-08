using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for Scorecard
    /// </summary>
    /// <remarks>
    ///  Created by: Ola
    ///  Created date: 2008-04-15    
    /// </remarks>
    public class ScorecardRepository : Repository<IScorecard>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KpiRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unitofwork</param>
        public ScorecardRepository(IUnitOfWork unitOfWork) 
#pragma warning disable 618
            : base(unitOfWork)
#pragma warning restore 618
        {
        }
    }
}
