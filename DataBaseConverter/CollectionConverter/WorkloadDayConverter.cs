using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.DatabaseConverter.CollectionConverter
{
    //TODO: The structure of WorkloadDay has been totaly changed, this converter needs a remake, but later 2008-01-24
    /// <summary>
    /// Workload Day converter
    /// </summary>
    public class WorkloadDayConverter : CccConverter<WorkloadDay, global::Domain.ForecastDay>
    {
        private readonly WorkloadDayRepository _rep;


        /// <summary>
        /// Initializes a new instance of the <see cref="WorkloadDayConverter"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="mapper">The mapper.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 3.12.2007
        /// </remarks>
        public WorkloadDayConverter(IUnitOfWork unitOfWork, Mapper<WorkloadDay, global::Domain.ForecastDay> mapper) : base(unitOfWork, mapper)
        {
            _rep = new WorkloadDayRepository(unitOfWork);
        }


        /// <summary>
        /// Gets the repository.
        /// </summary>
        /// <value>The repository.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 3.12.2007
        /// </remarks>
        public override IRepository<WorkloadDay> Repository
        {
            get { return null; } // _rep; }
        }


        /// <summary>
        /// Called when [persisted].
        /// Can be used eg to put stuff in mappedobjectpair instance
        /// </summary>
        /// <param name="pairCollection">The pair collection.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 3.12.2007
        /// </remarks>
        protected override void OnPersisted(ObjectPairCollection<global::Domain.ForecastDay, WorkloadDay> pairCollection)
        {
            Mapper.MappedObjectPair.WorkloadDay = pairCollection;
        }
    }
}
