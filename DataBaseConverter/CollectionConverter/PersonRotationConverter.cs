using System.Data;
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
    ///  Created date: 2008-09-25    
    /// /// </remarks>
    public class PersonRotationConverter : CccConverter<IPersonRotation, DataRow>
    {
        private readonly IRepository<IPersonRotation> _rep;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonRotationConverter"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="mapper">The mapper.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/29/2007
        /// </remarks>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-09-25    
        /// /// </remarks>
        public PersonRotationConverter(IUnitOfWork unitOfWork, Mapper<IPersonRotation, DataRow> mapper)
            : base(unitOfWork, mapper)
        {
            _rep = new  PersonRotationRepository(unitOfWork);
            
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
        ///  Created date: 2008-09-25    
        /// /// </remarks>
        public override IRepository<IPersonRotation> Repository
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
        ///  Created date: 2008-09-25    
        /// /// </remarks>
        protected override void OnPersisted(ObjectPairCollection<DataRow, IPersonRotation> pairCollection)
        {
            Mapper.MappedObjectPair.PersonRotations = pairCollection;
        }
    }
}
