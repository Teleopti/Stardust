using System.Collections.Generic;
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
#pragma warning disable 618
        public QueueSourceRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
#pragma warning restore 618
        {
        }

		public QueueSourceRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
		{
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