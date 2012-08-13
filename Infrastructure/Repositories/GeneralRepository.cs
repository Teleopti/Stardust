using System;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Class for General db things
    /// </summary>
    /// <remarks>
    /// Created by: peterwe
    /// Created date: 2008-06-30
    /// </remarks>
    public class GeneralRepository : Repository<IAggregateRoot>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GeneralRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The UnitOfWork.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-06-30
        /// </remarks>
        public GeneralRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {

        }

        /// <summary>
        /// Gets the server time.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-06-30
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public DateTime ServerTime
        {
            //Now we just get DateTime.Now
            //todo: Roger will get the sql server time, but later...
            get
            {
                return DateTime.Now;
            }
        }
    }
}
