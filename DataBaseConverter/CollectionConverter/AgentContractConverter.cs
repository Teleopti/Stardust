using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.DatabaseConverter.CollectionConverter
{
    /// <summary>
    /// Person contract converter
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 10/30/2007
    /// </remarks>
    public class AgentContractConverter : CccConverter<PersonContract, global::Domain.Agent>
    {
        private readonly IRepository<PersonContract> _rep;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentContractConverter"/> class.
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
        public AgentContractConverter(IUnitOfWork unitOfWork, Mapper<PersonContract, global::Domain.Agent> mapper)
            : base(unitOfWork, mapper)
        {
            _rep = new PersonContractRepository(unitOfWork);
        }

        /// <summary>
        /// Gets the repository.
        /// </summary>
        /// <value>The repository.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/29/2007
        /// </remarks>
        public override IRepository<PersonContract> Repository
        {
            get { return _rep; }
        }
    }
}
