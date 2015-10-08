using System.Collections.Generic;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public class SkillTypeRepository : Repository<ISkillType>, ISkillTypeRepository
    {

        public SkillTypeRepository(IUnitOfWork unitOfWork)
#pragma warning disable 618
            : base(unitOfWork)
#pragma warning restore 618
        {
        }

		public SkillTypeRepository(ICurrentUnitOfWork currentUnitOfWork)
			: base(currentUnitOfWork)
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