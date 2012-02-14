using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// ExternalLogOnRepository
    /// </summary>
    public class ExternalLogOnRepository : Repository<IExternalLogOn>, IExternalLogOnRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalLogOnRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unitofwork</param>
        public ExternalLogOnRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public ExternalLogOnRepository(IUnitOfWorkFactory unitOfWorkFactory)
            : base(unitOfWorkFactory)
        {
        }

        /// <summary>
        /// Loads all logins.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-18
        /// </remarks>
        public IList<IExternalLogOn> LoadAllExternalLogOns()
        {
            return Session.CreateCriteria(typeof (ExternalLogOn)).List<IExternalLogOn>();
        }
    }
}