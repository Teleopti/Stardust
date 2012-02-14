using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DatabaseConverter.CollectionConverter
{
    /// <summary>
    /// Team converter
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 10/30/2007
    /// </remarks>
    public class TeamConverter : CccConverter<ITeam, global::Domain.UnitSub>
    {
        private readonly IRepository<ITeam> _rep;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamConverter"/> class.
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
        public TeamConverter(IUnitOfWork unitOfWork, Mapper<ITeam, global::Domain.UnitSub> mapper) : base(unitOfWork, mapper)
        {
            _rep = new TeamRepository(unitOfWork);
        }

        /// <summary>
        /// Gets the repository.
        /// </summary>
        /// <value>The repository.</value>
        /// <remarks>
        /// Created by: rogerkr 
        /// Created date: 10/29/2007
        /// </remarks>
        public override IRepository<ITeam> Repository
        {
            get { return _rep; }
        }

        /// <summary>
        /// Called when [persisted].
        /// Can be used eg to put stuff in mappedobjectpair instance
        /// </summary>
        /// <param name="pairCollection">The pair collection.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/29/2007
        /// </remarks>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/30/2007
        /// </remarks>
        protected override void OnPersisted(ObjectPairCollection<global::Domain.UnitSub, ITeam> pairCollection)
        {
            Mapper.MappedObjectPair.Team = pairCollection;
        }
    }
}
