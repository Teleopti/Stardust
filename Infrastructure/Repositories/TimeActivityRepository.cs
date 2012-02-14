using System;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for Time Activities
    /// </summary>
    public class TimeActivityRepository : Repository<ITimeActivity>, ITimeActivityRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimeActivityRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unitofwork</param>
        public TimeActivityRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        /// <summary>
        /// Gets the concrete type.
        /// Used when loading one instance by id
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-07-06
        /// </remarks>
        protected override Type ConcreteType
        {
            get { return typeof(TimeActivity); }
        }
    }
}