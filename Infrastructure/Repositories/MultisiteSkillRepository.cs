using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// MultisiteSkillRepository class
    /// </summary>
    public class MultisiteSkillRepository : Repository<IMultisiteSkill>
    {
        public MultisiteSkillRepository(IUnitOfWork unitOfWork)
#pragma warning disable 618
            : base(unitOfWork)
#pragma warning restore 618
        {
        }

		public MultisiteSkillRepository(ICurrentUnitOfWork currentUnitOfWork)
			: base(currentUnitOfWork, null, null)
		{
		}
    }
}