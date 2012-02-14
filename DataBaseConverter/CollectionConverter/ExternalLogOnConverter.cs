using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DatabaseConverter.CollectionConverter
{
    /// <summary>
    /// Login source converter
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2007-10-31
    /// </remarks>
    public class ExternalLogOnConverter : CccConverter<IExternalLogOn, int>
    {
        private readonly IRepository<IExternalLogOn> _rep;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalLogOnConverter"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="mapper">The mapper.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-10-31
        /// </remarks>
        public ExternalLogOnConverter(IUnitOfWork unitOfWork, Mapper<IExternalLogOn, int> mapper) : base(unitOfWork, mapper)
        {
            _rep = new ExternalLogOnRepository(unitOfWork);
        }

        /// <summary>
        /// Gets the repository.
        /// </summary>
        /// <value>The repository.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/29/2007
        /// </remarks>
        public override IRepository<IExternalLogOn> Repository
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
        /// Created date: 2007-10-31
        /// </remarks>
        protected override void OnPersisted(ObjectPairCollection<int, IExternalLogOn> pairCollection)
        {
            Mapper.MappedObjectPair.ExternalLogOn = pairCollection;
        }
    }
}
