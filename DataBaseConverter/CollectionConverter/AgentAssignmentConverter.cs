using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DatabaseConverter.CollectionConverter
{
    /// <summary>
    /// Person assignment converter
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 10/31/2007
    /// </remarks>
    public class AgentAssignmentConverter : CccConverter<IPersonAssignment, global::Domain.AgentDay>
    {
        private readonly IRepository<IPersonAssignment> _rep;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentAssignmentConverter"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="mapper">The mapper.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/29/2007
        /// </remarks>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/31/2007
        /// </remarks>
        public AgentAssignmentConverter(IUnitOfWork unitOfWork, Mapper<IPersonAssignment, global::Domain.AgentDay> mapper) : base(unitOfWork, mapper)
        {
            _rep = new PersonAssignmentRepository(unitOfWork);
        }

        /// <summary>
        /// Gets the repository.
        /// </summary>
        /// <value>The repository.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/29/2007
        /// </remarks>
        public override IRepository<IPersonAssignment> Repository
        {
            get { return _rep; }
        }
    }
}
