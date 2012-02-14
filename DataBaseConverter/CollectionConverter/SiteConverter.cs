using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DatabaseConverter.CollectionConverter
{
    /// <summary>
    /// Site converter
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 10/30/2007
    /// </remarks>
    public class SiteConverter : CccConverter<ISite, global::Domain.Unit>
    {
        private readonly IRepository<ISite> _rep;

        /// <summary>
        /// Initializes a new instance of the <see cref="SiteConverter"/> class.
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
        public SiteConverter(IUnitOfWork unitOfWork, Mapper<ISite, global::Domain.Unit> mapper)
            : base(unitOfWork, mapper)
        {
            _rep = new SiteRepository(unitOfWork);
        }

        /// <summary>
        /// Gets the repository.
        /// </summary>
        /// <value>The repository.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/29/2007
        /// </remarks>
        public override IRepository<ISite> Repository
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
        protected override void OnPersisted(ObjectPairCollection<global::Domain.Unit, ISite> pairCollection)
        {
            Mapper.MappedObjectPair.Site = pairCollection;
        }
    }
}
