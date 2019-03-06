using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// MultisiteSkillRepository class
    /// </summary>
    public class MultisiteSkillRepository : Repository<IMultisiteSkill>
    {
		public static MultisiteSkillRepository DONT_USE_CTOR(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new MultisiteSkillRepository(currentUnitOfWork, null, null);
		}

		public static MultisiteSkillRepository DONT_USE_CTOR(IUnitOfWork unitOfWork)
		{
			return new MultisiteSkillRepository(new ThisUnitOfWork(unitOfWork), null, null);
		}
		
		public MultisiteSkillRepository(ICurrentUnitOfWork currentUnitOfWork, ICurrentBusinessUnit currentBusinessUnit, Lazy<IUpdatedBy> updatedBy)
			: base(currentUnitOfWork, currentBusinessUnit, updatedBy)
		{
		}
    }
}