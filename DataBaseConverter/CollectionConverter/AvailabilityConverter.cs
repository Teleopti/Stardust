using System.Data;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;

namespace Teleopti.Ccc.DatabaseConverter.CollectionConverter
{
    /// <summary>
    /// Converts Available
    /// </summary>
    /// <remarks>
    /// Created by: ZoeT
    /// Created date: 2008-12-05
    /// </remarks>
    public class AvailabilityConverter : CccConverter<IAvailabilityRotation, DataRow>
    {
        private readonly IRepository<IAvailabilityRotation> _rep;

        /// <summary>
        /// Initializes a new instance of the <see cref="AvailabilityConverter"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="mapper">The mapper.</param>
        /// <remarks>
        /// Created by: ZoeT
        /// Created date: 2008-12-05
        /// </remarks>
        public AvailabilityConverter(IUnitOfWork unitOfWork, Mapper<IAvailabilityRotation, DataRow> mapper)
            : base(unitOfWork, mapper)
        {
            _rep = new AvailabilityRepository(unitOfWork);
        }

        /// <summary>
        /// Gets the repository.
        /// </summary>
        /// <value>The repository.</value>
        /// <remarks>
        /// Created by: ZoeT
        /// Created date: 2008-12-05
        /// </remarks>
        public override IRepository<IAvailabilityRotation> Repository
        {
            get { return _rep; }
        }
        /// <summary>
        /// Called when [persisted].
        /// Can be used eg to put stuff in mappedobjectpair instance
        /// </summary>
        /// <param name="pairCollection">The pair collection.</param>
        /// <remarks>
        /// Created by: ZoeT
        /// Created date: 2008-12-05
        /// </remarks>
        protected override void OnPersisted(ObjectPairCollection<DataRow, IAvailabilityRotation> pairCollection)
        {
            Mapper.MappedObjectPair.Availability = pairCollection;
        }
    }
}
