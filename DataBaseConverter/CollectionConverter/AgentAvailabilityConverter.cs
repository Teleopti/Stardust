using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverter.CollectionConverter
{
    /// <summary>
    /// Person availibility converter
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 10/31/2007
    /// </remarks>
    public class AgentAvailabilityConverter : CccConverter<PersonAvailability, global::Domain.AgentDay>
    {
        private readonly IRepository<PersonAvailability> _rep;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentAvailabilityConverter"/> class.
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
        public AgentAvailabilityConverter(IUnitOfWork unitOfWork, Mapper<PersonAvailability, global::Domain.AgentDay> mapper)
            : base(unitOfWork, mapper)
        {
            _rep = new PersonAvailabilityRepository(unitOfWork);
        }

        /// <summary>
        /// Gets the repository.
        /// </summary>
        /// <value>The repository.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/29/2007
        /// </remarks>
        public override IRepository<PersonAvailability> Repository
        {
            get { return _rep; }
        }
    }
}
