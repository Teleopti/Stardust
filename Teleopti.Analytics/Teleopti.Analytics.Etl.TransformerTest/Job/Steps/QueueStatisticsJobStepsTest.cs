using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job;
using Teleopti.Analytics.Etl.Transformer.Job.Jobs;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Analytics.Etl.TransformerTest.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Analytics.Etl.TransformerTest.Job.Steps
{
    [TestFixture]
    public class QueueStatisticsJobStepsTest
    {
        private IJobParameters _jobParameters;
        private MockRepository _mockRepository;
        private IMessageSender _messageSender;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository();
            _messageSender = _mockRepository.StrictMock<IMessageSender>();

            _jobParameters = JobParametersFactory.SimpleParameters(false);
            _jobParameters.Helper = new JobHelper(new RaptorRepositoryStub(), _messageSender,null);
        }

        [Test]
        public void VerifyQueueStatisticsJobSteps()
        {
            Expect.Call(_messageSender.IsAlive).Return(false);
            Expect.Call(_messageSender.InstantiateBrokerService);
            Expect.Call(_messageSender.IsAlive).Return(true);
            Expect.Call(()=>_messageSender.SendData(DateTime.Now, DateTime.Now, Guid.Empty, Guid.Empty,
                                                typeof (IStatisticTask), DomainUpdateType.Insert, null, Guid.Empty)).IgnoreArguments();
            _mockRepository.ReplayAll();
            IList<IJobStep> jobStepList = new QueueStatisticsJobCollection(_jobParameters);
            foreach (IJobStep jobStep in jobStepList)
            {
                IJobStepResult jobStepResult = jobStep.Run(new List<IJobStep>(), null, null, false);
                Assert.IsNotNull(jobStepResult);
            }
            _mockRepository.VerifyAll();
        }
    }
}
