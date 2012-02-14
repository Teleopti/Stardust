using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DatabaseConverter.CollectionConverter
{
    /// <summary>
    /// Multiplicator converter
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 10/29/2007
    /// </remarks>
    public class MultiplicatorConverter : CccConverter<IMultiplicatorDefinitionSet, IMultiplicator>
    {
        private readonly IRepository<IMultiplicatorDefinitionSet> _rep;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiplicatorConverter"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="mapper">The mapper.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/29/2007
        /// </remarks>
        public MultiplicatorConverter(IUnitOfWork unitOfWork,
                                Mapper<IMultiplicatorDefinitionSet, IMultiplicator> mapper)
            : base(unitOfWork, mapper)
        {
            _rep = new MultiplicatorDefinitionSetRepository(unitOfWork);
        }

        /// <summary>
        /// Gets the repository.
        /// </summary>
        /// <value>The repository.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/29/2007
        /// </remarks>
        public override IRepository<IMultiplicatorDefinitionSet> Repository
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
        protected override void OnPersisted(ObjectPairCollection<IMultiplicator, IMultiplicatorDefinitionSet> pairCollection)
        {
            Mapper.MappedObjectPair.MultiplicatorDefinitionSet = pairCollection;
        }
    }
}
