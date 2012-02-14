using System.Data;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;

namespace Teleopti.Ccc.DatabaseConverter.CollectionConverter
{
    /// <summary>
    /// Person Availability Converter
    /// </summary>
    /// <remarks>
    /// Created by: ZoeT
    /// Created date: 2008-12-05
    /// </remarks>
    public class PersonAvailabilityConverter : CccConverter<IPersonAvailability, DataRow>
    {
        private readonly IRepository<IPersonAvailability> _rep;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonAvailabilityConverter"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="mapper">The mapper.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/29/2007
        /// </remarks>
        /// <remarks>
        /// Created by: ZoeT
        /// Created date: 2008-12-05
        /// </remarks>
        public PersonAvailabilityConverter(IUnitOfWork unitOfWork, Mapper<IPersonAvailability, DataRow> mapper)
            : base(unitOfWork, mapper)
        {
            _rep = new PersonAvailabilityRepository(unitOfWork);

        }

        /// <summary>
        /// Gets the repository.
        /// </summary>
        /// <value>The repository.</value>
        /// <remarks>
        /// Created by: ZoeT
        /// Created date: 2008-12-05
        /// </remarks>
        public override IRepository<IPersonAvailability> Repository
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
        protected override void OnPersisted(ObjectPairCollection<DataRow, IPersonAvailability> pairCollection)
        {
            Mapper.MappedObjectPair.PersonAvailability = pairCollection;
        }
    }
}
