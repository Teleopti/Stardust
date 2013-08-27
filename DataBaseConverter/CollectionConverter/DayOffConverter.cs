using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DatabaseConverter.CollectionConverter
{
    /// <summary>
    /// Absence converter
    /// </summary>
    /// <remarks>
    /// Created by: Madhuranga Pinnagoda
    /// Created date: 2008-11-26
    /// </remarks>
    public class DayOffConverter : CccConverter<IDayOffTemplate, global::Domain.Absence>
    {
        private readonly IRepository<IDayOffTemplate> _rep;

        /// <summary>
        /// Initializes a new instance of the <see cref="DayOffConverter"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="mapper">The mapper.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/29/2007
        /// </remarks>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-11-26
        /// </remarks>
        public DayOffConverter(IUnitOfWork unitOfWork, 
                            Mapper<IDayOffTemplate, global::Domain.Absence> mapper)
            : base(unitOfWork, mapper)
        {
            _rep = new DayOffTemplateRepository(unitOfWork);
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
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-11-26
        /// </remarks>
        public override IRepository<IDayOffTemplate> Repository
        {
            get { return _rep; }
        }

        /// <summary>
        /// Called when [persisted].
        /// </summary>
        /// <param name="pairCollection">The pair collection.</param>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-11-26
        /// </remarks>
        protected override void OnPersisted(ObjectPairCollection<global::Domain.Absence, IDayOffTemplate> pairCollection)
        {
            Mapper.MappedObjectPair.DayOff = pairCollection;
        }
    }
}
