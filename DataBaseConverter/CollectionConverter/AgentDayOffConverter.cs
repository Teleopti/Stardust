using System;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DatabaseConverter.CollectionConverter
{
    /// <summary>
    /// Person day off converter
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 10/31/2007
    /// </remarks>
    public class AgentDayOffConverter : CccConverter<IPersonDayOff, global::Domain.AgentDay>
    {
        private readonly IRepository<IPersonDayOff> _rep;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentDayOffConverter"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="mapper">The mapper.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/29/2007
        /// </remarks>
        public AgentDayOffConverter(IUnitOfWork unitOfWork, Mapper<IPersonDayOff, global::Domain.AgentDay> mapper)
            : base(unitOfWork, mapper)
        {
					_rep = null;// new PersonDayOffRepository(unitOfWork);
          throw new Exception("need to fix this");
        }

        /// <summary>
        /// Gets the repository.
        /// </summary>
        /// <value>The repository.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/29/2007
        /// </remarks>
        public override IRepository<IPersonDayOff> Repository
        {
            get { return _rep; }
        }
    }
}
