using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DatabaseConverter.CollectionConverter
{
    /// <summary>
    /// Skill Day converter
    /// </summary>
    public class SkillDayConverter : CccConverter<ISkillDay, global::Domain.SkillDay>
    {
        private readonly SkillDayRepository _rep;


        /// <summary>
        /// Initializes a new instance of the <see cref="SkillDayConverter"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="mapper">The mapper.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 3.12.2007
        /// </remarks>
        public SkillDayConverter(IUnitOfWork unitOfWork, Mapper<ISkillDay, global::Domain.SkillDay> mapper)
            : base(unitOfWork, mapper)
        {
            _rep = new SkillDayRepository(unitOfWork);
        }

        /// <summary>
        /// Gets the repository.
        /// </summary>
        /// <value>The repository.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 3.12.2007
        /// </remarks>
        public override IRepository<ISkillDay> Repository
        {
            get { return _rep; }
        }
    }
}
