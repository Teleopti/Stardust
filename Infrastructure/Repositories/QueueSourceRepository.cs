using System.Collections.Generic;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// SourceQueueRepository
    /// </summary>
    public class QueueSourceRepository : Repository<IQueueSource>, IQueueSourceRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SkillTypeRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unitofwork</param>
        public QueueSourceRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        /// <summary>
        /// Loads all queues.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-05-07
        /// </remarks>
        public IList<IQueueSource> LoadAllQueues()
        {
            return Session.CreateCriteria(typeof(QueueSource)).List<IQueueSource>();
        }

        public IDictionary<int, string> GetDistinctLogItemName()
        {
            var dbResult = Session.GetNamedQuery("distinctLogItemName").List();
            var res = new Dictionary<int, string>();
            foreach (object[] dbItem in dbResult)
            {
                res[(int)dbItem[0]] = (string)dbItem[1];
            }
            return res;
        }
    }
}