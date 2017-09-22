using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job;
using Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;
using JobResult = Teleopti.Analytics.Etl.Common.Transformer.Job.JobResult;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer.Job
{
    [TestFixture]
    public class JobResultTest
    {
        private JobResult _target;
        private IBusinessUnit _businessUnit;
        private IBusinessUnit _previousBusinessUnit;
        private Guid _buCode;
        private IList<IJobResult> _jobResultCollection;
        private string _jobStepNameSuccess;
        private string _jobStepNameFailure;

        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            _buCode = new Guid("CB3411FD-D097-42B1-8B8F-5337CB463CC5");
            _businessUnit = new BusinessUnit("MyBu");
            _businessUnit.SetId(_buCode);
            _previousBusinessUnit = new BusinessUnit("PrevBU");
            _jobResultCollection = getJobResultCollection();
            _jobStepNameSuccess = "stg_person";
            _jobStepNameFailure = "stg_activity";

            _target = new JobResult(_businessUnit, _jobResultCollection);

            foreach (IJobStepResult jobStepResult in JobStepResultFactory.GetJobStepResultList(_businessUnit, _jobResultCollection))
            {
                _target.JobStepResultCollection.Add(jobStepResult);
            }
        }

        private IList<IJobResult> getJobResultCollection()
        {
            IJobResult jobResultFailure = new JobResult(_previousBusinessUnit, null) { Success = false, Name = "MyJob" };

            IJobStepResult jobStepResultSuccess = new JobStepResult(_jobStepNameSuccess, 100, 150, _previousBusinessUnit, null);
            IJobStepResult jobStepResultFailure = new JobStepResult(_jobStepNameFailure, 100, new ArgumentException("job result error"),
                                                                    _previousBusinessUnit, null);
            jobResultFailure.JobStepResultCollection.Add(jobStepResultSuccess);
            jobResultFailure.JobStepResultCollection.Add(jobStepResultFailure);

            IList<IJobResult> jobResults = new List<IJobResult>();
            jobResults.Add(jobResultFailure);

            return jobResults;
        }

        #endregion

        [Test]
        public void VerifyDuration()
        {
            Assert.AreEqual(1 + 2 + 3, _target.Duration * 1000);

            Assert.AreEqual(6, _target.RowsAffected);

        }

        [Test]
        public void VerifyProperty()
        {
            DateTime startTime = new DateTime(2010, 1, 1, 0, 0, 0);
            DateTime endTime = new DateTime(2010, 1, 1, 0, 0, 20);

            _target.Name = "name";
            _target.Status = "status";
            _target.StartTime = startTime;
            _target.EndTime = endTime;

            Assert.AreEqual("status", _target.Status);
            Assert.AreEqual("name", _target.Name);
            Assert.AreEqual(startTime, _target.StartTime);
            Assert.AreEqual(endTime, _target.EndTime);

            Assert.AreEqual(_businessUnit.Id, _target.CurrentBusinessUnit.Id);
            Assert.AreEqual(_businessUnit.Name, _target.CurrentBusinessUnit.Name);
        }

        [Test]
        public void VerifyBusinessUnitStatus()
        {
            string buStatus1 = string.Format(CultureInfo.CurrentCulture, "Error in business units: '{0}'",
                                            _businessUnit.Name);
            string buStatus2 = string.Format(CultureInfo.CurrentCulture, "Error in business units: '{0}', '{1}'",
                                            _previousBusinessUnit.Name, _businessUnit.Name);

            _target = new JobResult(_businessUnit, null);
            Assert.AreEqual(buStatus1, _target.BusinessUnitStatus);
            Assert.IsTrue(_target.HasError);

            _target = new JobResult(_businessUnit, _jobResultCollection);
            Assert.AreEqual(buStatus2, _target.BusinessUnitStatus);
            Assert.IsTrue(_target.HasError);

            _target = new JobResult(_businessUnit, null);
            _target.Success = true;
            Assert.AreEqual(string.Empty, _target.BusinessUnitStatus);
            Assert.IsFalse(_target.HasError);
        }
    }
}