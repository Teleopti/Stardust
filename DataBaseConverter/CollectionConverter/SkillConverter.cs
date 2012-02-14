using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DatabaseConverter.CollectionConverter
{
    /// <summary>
    /// Skill converter
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 10/30/2007
    /// </remarks>
    public class SkillConverter : CccConverter<ISkill, global::Domain.Skill>
    {
        private readonly SkillRepository _rep;

        /// <summary>
        /// Initializes a new instance of the <see cref="SkillConverter"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="mapper">The mapper.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/29/2007
        /// </remarks>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/30/2007
        /// </remarks>
        public SkillConverter(IUnitOfWork unitOfWork, Mapper<ISkill, global::Domain.Skill> mapper)
            : base(unitOfWork, mapper)
        {
            _rep = new SkillRepository(unitOfWork);
        }

        /// <summary>
        /// Gets the repository.
        /// </summary>
        /// <value>The repository.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/29/2007
        /// </remarks>
        public override IRepository<ISkill> Repository
        {
            get { return _rep; }
        }

        /// <summary>
        /// Called when [persisted].
        /// </summary>
        /// <param name="pairCollection">The pair collection.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-02
        /// </remarks>
        protected override void OnPersisted(ObjectPairCollection<global::Domain.Skill, ISkill> pairCollection)
        {
            Mapper.MappedObjectPair.Skill = pairCollection;
        }
    }
}
