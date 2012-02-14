using System.Data;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;

namespace Teleopti.Ccc.DatabaseConverter.CollectionConverter
{
    /// <summary>
    /// 
    /// </summary>
    /// /// 
    /// <remarks>
    ///  Created by: Ola
    ///  Created date: 2008-09-24    
    /// /// </remarks>
    public class RotationsConverter : CccConverter<IRotation, DataRow>
    {
        private readonly IRepository<IRotation> _rep;
        

        /// <summary>
        /// Initializes a new instance of the <see cref="RotationsConverter"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="mapper">The mapper.</param>
        /// ///
        /// <remarks>
        /// Created by: Ola
        /// Created date: 2008-09-24
        /// /// </remarks>
        public RotationsConverter(IUnitOfWork unitOfWork, Mapper<IRotation, DataRow> mapper)
            : base(unitOfWork, mapper)
        {
            _rep = new  RotationRepository(unitOfWork);
            
        }

        /// <summary>
        /// Gets the repository.
        /// </summary>
        /// <value>The repository.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/29/2007
        /// </remarks>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-09-24    
        /// /// </remarks>
        public override IRepository<IRotation> Repository
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
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-09-24    
        /// /// </remarks>
        protected override void OnPersisted(ObjectPairCollection<DataRow, IRotation> pairCollection)
        {
            Mapper.MappedObjectPair.Rotations = pairCollection;
        }
    }
}
