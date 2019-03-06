using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public class SkillTypeRepository : Repository<ISkillType>, ISkillTypeRepository
    {
		public static SkillTypeRepository DONT_USE_CTOR(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new SkillTypeRepository(currentUnitOfWork, null, null);
		}

		public static SkillTypeRepository DONT_USE_CTOR(IUnitOfWork unitOfWork)
		{
			return new SkillTypeRepository(new ThisUnitOfWork(unitOfWork), null, null);
		}

		public SkillTypeRepository(ICurrentUnitOfWork currentUnitOfWork, ICurrentBusinessUnit currentBusinessUnit, Lazy<IUpdatedBy> updatedBy)
			: base(currentUnitOfWork, currentBusinessUnit, updatedBy)
		{
		}
    }
}