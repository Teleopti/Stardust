﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
    [TestFixture]
    [Category("LongRunning")]
    public class MatrixSyncTest : DatabaseTest
    {
        [Test]
        public void VerifySynchronizeQueueSources()
        {
            IQueueSource matrixQueue = new QueueSource("q 1", "queue 1", 1, 2, 3, 4);
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

        //[Test]
        //public void VerifySynchronizeReports()
        //{
        //        MatrixSync.SynchronizeReports(UnitOfWork, repositoryFactory);
        //}

        protected override void SetupForRepositoryTest(){}
    }
}
