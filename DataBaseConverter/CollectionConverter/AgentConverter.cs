using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DatabaseConverter.CollectionConverter
{
    /// <summary>
    /// Person converter
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 10/29/2007
    /// </remarks>
    public class AgentConverter : CccConverter<IPerson, global::Domain.Agent>
    {
        private readonly IRepository<IPerson> _rep;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentConverter"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="mapper">The mapper.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/29/2007
        /// </remarks>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/29/2007
        /// </remarks>
        public AgentConverter(IUnitOfWork unitOfWork, Mapper<IPerson, global::Domain.Agent> mapper) : base(unitOfWork, mapper)
        {
            _rep = new PersonRepository(unitOfWork);
        }

        /// <summary>
        /// Gets the repository.
        /// </summary>
        /// <value>The repository.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/29/2007
        /// </remarks>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/29/2007
        /// </remarks>
        public override IRepository<IPerson> Repository
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
        /// Created date: 10/29/2007
        /// </remarks>
        protected override void OnPersisted(ObjectPairCollection<global::Domain.Agent, IPerson> pairCollection)
        {
            Mapper.MappedObjectPair.Agent = pairCollection;
        }
    }
}
