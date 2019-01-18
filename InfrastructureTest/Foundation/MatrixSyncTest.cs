using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.InfrastructureTest.Helper;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
    [TestFixture]
    [Category("BucketB")]
    public class MatrixSyncTest : DatabaseTest
    {
        [Test]
        public void VerifySynchronizeQueueSources()
        {
            IQueueSource matrixQueue = new QueueSource("q 1", "queue 1", "1", 2, 3, 4);
            IList<IQueueSource> matrixQueues = new List<IQueueSource>();
            matrixQueues.Add(matrixQueue);

            int retVal = MatrixSync.SynchronizeQueueSources(UnitOfWork, matrixQueues);

            Assert.AreEqual(1, retVal);
        }

        [Test]
        public void VerifySynchronizeExternalLogOns()
        {
            IExternalLogOn externalLogOn = new ExternalLogOn(1, 2, "3", "agent 1", true);
            IList<IExternalLogOn> externalLogOnCollection = new List<IExternalLogOn>();
            externalLogOnCollection.Add(externalLogOn);

            int retVal = MatrixSync.SynchronizeExternalLogOns(UnitOfWork, externalLogOnCollection);

            Assert.AreEqual(1, retVal);
        }
		
        protected override void SetupForRepositoryTest(){}
    }
}
