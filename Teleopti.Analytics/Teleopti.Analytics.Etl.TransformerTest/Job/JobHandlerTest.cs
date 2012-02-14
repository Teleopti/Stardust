using System;
using System.Collections.Generic;
using NUnit.Framework;
using RaptorTransformer.Job;
using RaptorTransformerInfrastructure;

namespace RaptorTransformerTest.Job
{
    [TestFixture]
    public class JobHandlerTest
    {
        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            //_jobList = JobHandler.ListOfLoads;
            _jobList = JobHandler.GetJobList(new JobHelper(new RaptorRepositoryStub()));
        }

        #endregion

        private IList<IJob> _jobList;

        private void JobHandler_UpdateProgressEvent(object sender, CustomEventArgs e)
        {
        }

        [Test]
        public void VerifyRun()
        {
            JobHandler.UpdateProgressEvent += new EventHandler<CustomEventArgs>(JobHandler_UpdateProgressEvent);

            JobHandler.RunJob(_jobList);

            //_jobList[1].Run();
        }
    }
}