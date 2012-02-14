using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DatabaseConverter.CollectionConverter
{
    /// <summary>
    /// Scenario converter
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 10/29/2007
    /// </remarks>
    public class ScenarioConverter : CccConverter<IScenario, global::Domain.Scenario>
    {
        private readonly IRepository<IScenario> _rep;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScenarioConverter"/> class.
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
        public ScenarioConverter(IUnitOfWork unitOfWork, 
                                Mapper<IScenario, global::Domain.Scenario> mapper)
            : base(unitOfWork, mapper)
        {
            _rep = new ScenarioRepository(unitOfWork);
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
        public override IRepository<IScenario> Repository
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
        protected override void OnPersisted(ObjectPairCollection<global::Domain.Scenario, IScenario> pairCollection)
        {
            Mapper.MappedObjectPair.Scenario = pairCollection;
        }
    }
}
