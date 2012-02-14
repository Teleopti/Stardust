using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DatabaseConverter.CollectionConverter
{
    /// <summary>
    /// Converts a users.
    /// </summary>
    public class UserConverter : CccConverter<IPerson, global::Domain.User>
    {
        private readonly IRepository<IPerson> _rep;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserConverter"/> class.
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
        public UserConverter(IUnitOfWork unitOfWork, Mapper<IPerson, global::Domain.User> mapper)
            : base(unitOfWork, mapper)
        {
            _rep = new PersonRepository(unitOfWork);
        }

        /// <summary>
        /// Gets the repository.
        /// </summary>
        /// <value>The repository.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/29/2007
        /// </remarks>
        public override IRepository<IPerson> Repository
        {
            get { return _rep; }
        }
    }
}