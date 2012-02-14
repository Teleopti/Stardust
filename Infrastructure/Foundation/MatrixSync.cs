using System.Collections.Generic;
//using System.Windows.Forms;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Matrix;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
    public sealed class MatrixSync
    {
        private MatrixSync()
        {
        }

        public static int SynchronizeQueueSources(IUnitOfWork uow, IList<IQueueSource> matrixQueues)
        {
            IQueueSourceRepository rep = new QueueSourceRepository(uow);
            var matrixSync = new MatrixRaptorQueueSynchronization(rep);
            return matrixSync.SynchronizeQueues(matrixQueues);
        }

        public static int SynchronizeExternalLogOns(IUnitOfWork uow, IList<IExternalLogOn> matrixAgentLogins)
        {
            IExternalLogOnRepository rep = new ExternalLogOnRepository(uow);
            var matrixSync = new MatrixRaptorExternalLogOnSynchronization(rep);
            return matrixSync.SynchronizeExternalLogOns(matrixAgentLogins);
        }

        public static void SynchronizeReports(IUnitOfWork uow, IRepositoryFactory repositoryFactory)
        {
            var synchronizer = new MatrixReportsSynchronizer(repositoryFactory, uow);
            synchronizer.SynchronizeMatrixReports();
        }
    }
}