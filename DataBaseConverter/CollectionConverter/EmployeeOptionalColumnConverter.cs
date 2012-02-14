using Domain;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DatabaseConverter.CollectionConverter
{
    /// <summary>
    /// Converts old entity EmployeeOptionalColumn to new entity OptionalColumn
    /// </summary>
    /// <remarks>
    /// Created by: Sachintha Weerasekara
    /// Created date: 8/13/2008
    /// </remarks>
    public class EmployeeOptionalColumnConverter : CccConverter<IOptionalColumn, EmployeeOptionalColumn>
    {
        private readonly IOptionalColumnRepository repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmployeeOptionalColumnConverter"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="mapper">The mapper.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/29/2007
        /// </remarks>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 8/13/2008
        /// </remarks>
        public EmployeeOptionalColumnConverter(IUnitOfWork unitOfWork, Mapper<IOptionalColumn, EmployeeOptionalColumn> mapper)
            : base(unitOfWork, mapper)
        {
            repository = new OptionalColumnRepository(unitOfWork);
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
        /// Created by: Sachintha Weerasekara
        /// Created date: 8/13/2008
        /// </remarks>
        public override IRepository<IOptionalColumn> Repository
        {
            get { return repository; }
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
        /// Created by: Sachintha Weerasekara
        /// Created date: 8/13/2008
        /// </remarks>
        protected override void OnPersisted(ObjectPairCollection<EmployeeOptionalColumn, IOptionalColumn> pairCollection)
        {
           Mapper.MappedObjectPair.OptionalColumn = pairCollection;
        }
    }
}
