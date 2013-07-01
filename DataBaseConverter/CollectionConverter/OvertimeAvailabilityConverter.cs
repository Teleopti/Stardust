using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DatabaseConverter.CollectionConverter
{
    public class OvertimeAvailabilityConverter : CccConverter<IOvertimeAvailability , global::Domain.AgentDay>
    {
        private readonly IRepository<IOvertimeAvailability> _rep;

        ///<summary>
        /// Initializes a new instance of the <see cref="OvertimeAvailabilityConverter"/> class.
        ///</summary>
        ///<param name="unitOfWork"></param>
        ///<param name="mapper"></param>
        public OvertimeAvailabilityConverter(IUnitOfWork unitOfWork, Mapper<IOvertimeAvailability, global::Domain.AgentDay> mapper)
            : base(unitOfWork, mapper)
        {
            _rep = new OvertimeAvailabilityRepository(unitOfWork);
        }

        /// <summary>
        /// Gets the repository.
        /// </summary>
        /// <value>The repository.</value>
        public override IRepository<IOvertimeAvailability > Repository
        {
            get { return _rep; }
        }

        /// <summary>
        /// Called when [persisted].
        /// Can be used eg to put stuff in mappedobjectpair instance
        /// </summary>
        /// <param name="pairCollection">The pair collection.</param>
        protected override void OnPersisted(ObjectPairCollection<global::Domain.AgentDay, IOvertimeAvailability > pairCollection)
        {
            Mapper.MappedObjectPair.OvertimeAvailability  = pairCollection;
        }
    }
}
