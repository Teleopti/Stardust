using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// MultisiteSkillRepository class
    /// </summary>
    public class MultisiteSkillRepository : Repository<IMultisiteSkill>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultisiteSkillRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unitofwork</param>
        public MultisiteSkillRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

		public MultisiteSkillRepository(ICurrentUnitOfWork currentUnitOfWork)
			: base(currentUnitOfWork)
		{
		}
    }
}