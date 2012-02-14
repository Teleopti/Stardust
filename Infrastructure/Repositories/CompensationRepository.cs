using System;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for compensations
    /// </summary>
    public class CompensationRepository : Repository<Compensation>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompensationRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unitofwork</param>
        public CompensationRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
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
            get { return typeof(Compensation); }
        }
    }
}