using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.DatabaseConverter.CollectionConverter
{
    /// <summary>
    /// Shift classification converter
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2007-10-30
    /// </remarks>
    public class ShiftClassificationConverter : CccConverter<ShiftClassification, global::Domain.ShiftClass>
    {
        private IRepository<ShiftClassification> _rep;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShiftClassificationConverter"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="mapper">The mapper.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-10-30
        /// </remarks>
        public ShiftClassificationConverter(IUnitOfWork unitOfWork, Mapper<ShiftClassification, global::Domain.ShiftClass> mapper)
            : base(unitOfWork, mapper)
        {
            _rep = new ShiftClassificationRepository(unitOfWork);
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
        public override IRepository<ShiftClassification> Repository
        {
            get { return _rep; }
        }
    }
}
