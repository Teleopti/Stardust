using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DatabaseConverter.CollectionConverter
{
    /// <summary>
    /// Workload converter
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2007-10-31
    /// </remarks>
    public class WorkloadConverter : CccConverter<IWorkload, global::Domain.Forecast>
    {
        private readonly WorkloadRepository _rep;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkloadConverter"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="mapper">The mapper.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/29/2007
        /// </remarks>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-10-31
        /// </remarks>
        public WorkloadConverter(IUnitOfWork unitOfWork, Mapper<IWorkload, global::Domain.Forecast> mapper) : base(unitOfWork, mapper)
        {
            _rep = new WorkloadRepository(unitOfWork);
        }

        /// <summary>
        /// Gets the repository.
        /// </summary>
        /// <value>The repository.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/29/2007
        /// </remarks>
        public override IRepository<IWorkload> Repository
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
        /// Created date: 2007-10-31
        /// </remarks>
        protected override void OnPersisted(ObjectPairCollection<global::Domain.Forecast, IWorkload> pairCollection)
        {
            if (Mapper.MappedObjectPair.Workload == null)
            {
                Mapper.MappedObjectPair.Workload = pairCollection;
            }
            else
            {
                foreach (ObjectPair<global::Domain.Forecast, IWorkload> workloadPair in pairCollection)
                {
                    Mapper.MappedObjectPair.Workload.Add(workloadPair.Obj1, workloadPair.Obj2);
                }
            }
        }
    }
}
