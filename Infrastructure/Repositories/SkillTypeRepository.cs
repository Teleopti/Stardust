using System.Collections.Generic;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// SkillTypeRepository
    /// </summary>
    public class SkillTypeRepository : Repository<ISkillType>, ISkillTypeRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SkillTypeRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unitofwork</param>
        public SkillTypeRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        /// <summary>
        /// Finds all and include workload and queues.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-12-10
        /// </remarks>
        public ICollection<ISkillType> FindAll()
        {
            return Session.CreateCriteria(typeof (SkillType))
                                .List<ISkillType>();
        }
    }
}