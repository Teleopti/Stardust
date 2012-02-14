using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DatabaseConverter.CollectionConverter
{
    /// <summary>
    /// Overtime Activity converter
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 10/29/2007
    /// </remarks>
    public class OvertimeActivityConverter : CccConverter<IMultiplicatorDefinitionSet, global::Domain.Overtime>
    {
        private readonly IRepository<IMultiplicatorDefinitionSet> _rep;

        /// <summary>
        /// Initializes a new instance of the <see cref="OvertimeActivityConverter"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="mapper">The mapper.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/29/2007
        /// </remarks>
        public OvertimeActivityConverter(IUnitOfWork unitOfWork,
                                Mapper<IMultiplicatorDefinitionSet, global::Domain.Overtime> mapper)
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
        protected override void OnPersisted(ObjectPairCollection<global::Domain.Overtime, IMultiplicatorDefinitionSet> pairCollection)
        {
            Mapper.MappedObjectPair.MultiplicatorDefinitionSet = pairCollection;
        }
    }
}
