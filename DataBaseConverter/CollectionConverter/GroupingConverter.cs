#region Imports

using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

#endregion

namespace Teleopti.Ccc.DatabaseConverter.CollectionConverter
{
    /// <summary>
    /// Converts Grouping to GroupPage 
    /// </summary>
    /// <remarks>
    /// Created by: Sachintha Weerasekara
    /// Created date: 7/6/2008
    /// </remarks>
    public class GroupingConverter : CccConverter<IGroupPage, global::Domain.Grouping>
    {
        #region Fields - Instance Member

        private IRepository<IGroupPage> rep;

        #endregion

        #region Methods - Instance Members - GroupingConverter Members - (constructors)

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupingConverter"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="mapper">The mapper.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/29/2007
        /// </remarks>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 7/6/2008
        /// </remarks>
        public GroupingConverter(IUnitOfWork unitOfWork, Mapper<IGroupPage, global::Domain.Grouping> mapper)
            : base(unitOfWork, mapper)
        {
            rep = new GroupPageRepository(unitOfWork);
        }

        #endregion

        #region Methods - Instance Member - CccConverter Members

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
        /// Created date: 7/6/2008
        /// </remarks>
        public override IRepository<IGroupPage> Repository
        {
            get { return rep; }
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
        /// Created date: 7/6/2008
        /// </remarks>
        protected override void OnPersisted(ObjectPairCollection<global::Domain.Grouping, IGroupPage> pairCollection)
        {
            Mapper.MappedObjectPair.Grouping = pairCollection;
        }

        #endregion

    }
}
