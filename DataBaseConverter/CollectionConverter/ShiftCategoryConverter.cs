using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DatabaseConverter.CollectionConverter
{
    /// <summary>
    /// ShiftCategory converter
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 10/29/2007
    /// </remarks>
    public class ShiftCategoryConverter : CccConverter<IShiftCategory, global::Domain.ShiftCategory>
    {
        private IRepository<IShiftCategory> _rep;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShiftCategoryConverter"/> class.
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
        public ShiftCategoryConverter(IUnitOfWork unitOfWork, Mapper<IShiftCategory, global::Domain.ShiftCategory> mapper)
            : base(unitOfWork, mapper)
        {
            _rep = new ShiftCategoryRepository(unitOfWork);
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
        public override IRepository<IShiftCategory> Repository
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
        protected override void OnPersisted(ObjectPairCollection<global::Domain.ShiftCategory, IShiftCategory> pairCollection)
        {
            Mapper.MappedObjectPair.ShiftCategory = pairCollection;
        }
    }
}
