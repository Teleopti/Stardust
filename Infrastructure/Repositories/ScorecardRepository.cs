using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;

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
		public static ScorecardRepository DONT_USE_CTOR(IUnitOfWork unitOfWork)
		{
			return new ScorecardRepository(new ThisUnitOfWork(unitOfWork), null, null);
		}

		/// <summary>
        /// Initializes a new instance of the <see cref="KpiRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unitofwork</param>
		public ScorecardRepository(ICurrentUnitOfWork currentUnitOfWork, ICurrentBusinessUnit currentBusinessUnit, Lazy<IUpdatedBy> updatedBy)
			: base(currentUnitOfWork, currentBusinessUnit, updatedBy)
		{
		}
    }
}
