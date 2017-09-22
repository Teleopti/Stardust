using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;
using JobResult = Teleopti.Analytics.Etl.Common.Transformer.Job.JobResult;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer.Job
{
    [TestFixture]
    public class JobStepResultTest
    {
        private JobStepResult _target;
        private IBusinessUnit _businessUnit;
        private IBusinessUnit _previousBusinessUnit;
        private Guid _buCode;
        private string _jobStepNameSuccess;
        private string _jobStepNameFailure;

        [SetUp]
        public void Setup()
        {
            _buCode = new Guid("CB3411FD-D097-42B1-8B8F-5337CB463CC5");
            _businessUnit = new BusinessUnit("MyBu");
            _businessUnit.SetId(_buCode);
            _previousBusinessUnit = new BusinessUnit("PrevBU");
            _jobStepNameSuccess = "stg_person";
            _jobStepNameFailure = "stg_activity";
        }

        private IList<IJobResult> getJobResultCollection()
        {
            IJobResult jobResultFailure = new JobResult(_previousBusinessUnit, null) { Success = false, Name = "MyJob" };

            IJobStepResult jobStepResultSuccess = new JobStepResult(_jobStepNameSuccess, 100, 150, _previousBusinessUnit, null);
            IJobStepResult jobStepResultFailure = new JobStepResult(_jobStepNameFailure, 100, new ArgumentException("job step result error"),
                                                                    _previousBusinessUnit, null);
            jobResultFailure.JobStepResultCollection.Add(jobStepResultSuccess);
            jobResultFailure.JobStepResultCollection.Add(jobStepResultFailure);

            IList<IJobResult> jobResults = new List<IJobResult>();
            jobResults.Add(jobResultFailure);

            return jobResults;
        }

        [Test]
        public void VerifyJobStepResultProperties()
        {
            _target = new JobStepResult("Name", 1, 3000, _businessUnit, null);
            

            Assert.AreEqual("Name", _target.Name);
            Assert.AreEqual(1, _target.RowsAffected);
            Assert.AreEqual(3000/1000d, _target.Duration);
            Assert.AreEqual(_buCode, _target.CurrentBusinessUnit.Id);
            Assert.AreEqual(_businessUnit.Name, _target.CurrentBusinessUnit.Name);
        }
        [Test]
        public void VerifyJobStepResultProperties2()
        {
            _target = new JobStepResult("Name", 2500, new NotImplementedException("eee"), null, null);

            Assert.AreEqual("Name", _target.Name);
            Assert.AreEqual(2500/1000d, _target.Duration);
            Assert.AreEqual("eee", _target.JobStepException.Message);
        }

        [Test]
        public void VerifyCanSetProperties()
        {
            _target = new JobStepResult("nameYou", 0, 0, null, null);

            _target.RowsAffected = 11;
            _target.Duration = 1089;

            Assert.AreEqual(11, _target.RowsAffected);
            Assert.AreEqual(1089/1000d, _target.Duration);
        }

        [Test]
        public void VerifyBusinessUnitStatus()
        {
            string buStatus1 = string.Format(CultureInfo.CurrentCulture, "Error in business units: '{0}'",
                                            _previousBusinessUnit.Name);
            string buStatus2 = string.Format(CultureInfo.CurrentCulture, "Error in business units: '{0}'",
                                            _businessUnit.Name);
            IList<IJobResult> jobResultCollection = getJobResultCollection();
            
            _target = new JobStepResult(_jobStepNameFailure, 1, 1, _businessUnit, jobResultCollection);
            Assert.AreEqual(buStatus1, _target.BusinessUnitStatus);
            Assert.IsTrue(_target.HasError);

            _target = new JobStepResult(_jobStepNameSuccess, 1, 1, _businessUnit, jobResultCollection);
            Assert.AreEqual(string.Empty, _target.BusinessUnitStatus);
            Assert.IsFalse(_target.HasError);

            _target = new JobStepResult(_jobStepNameSuccess, 1, 1, _businessUnit, null);
            Assert.AreEqual(string.Empty, _target.BusinessUnitStatus);
            Assert.IsFalse(_target.HasError);

            _target = new JobStepResult("ErrorStepInCurrentBu", 1, new ArgumentException("job step result error"), _businessUnit, null);
            Assert.AreEqual(buStatus2, _target.BusinessUnitStatus);
            Assert.IsTrue(_target.HasError);
        }

        [Test]
        public void VerifyClearResult()
        {
            _target = new JobStepResult("job name", 12, 15, _businessUnit, null);
            _target.Status = "Done";

            _target.ClearResult();

            Assert.IsFalse(_target.Duration.HasValue);
            Assert.IsFalse(_target.RowsAffected.HasValue);
            Assert.IsEmpty(_target.Status);
        }

        [Test]
        public void ShouldMakeADeepClone()
        {
            _target = new JobStepResult("Name", 99, 2500, _businessUnit, getJobResultCollection()) {Status = "Done"};

            var clone = (JobStepResult)_target.Clone();

            Assert.AreEqual(_target.Name, clone.Name);
            Assert.AreSame(_target.CurrentBusinessUnit, clone.CurrentBusinessUnit);
            Assert.AreEqual(_target.Duration, clone.Duration);
            Assert.AreEqual(_target.RowsAffected, clone.RowsAffected);
            Assert.AreEqual(_target.Status, clone.Status);
            Assert.AreEqual(_target.HasError, clone.HasError);
            Assert.AreSame(_target.JobStepException, clone.JobStepException);
            Assert.AreEqual(_target.BusinessUnitStatus, clone.BusinessUnitStatus);

            IBusinessUnit otherBu = new BusinessUnit("other bu");
            _target = new JobStepResult("Other name", 44, 1300, otherBu, null) {Status = "yoda"};

	        Assert.AreNotEqual(_target.Name, clone.Name);
            Assert.AreNotSame(_target.CurrentBusinessUnit, clone.CurrentBusinessUnit);
            Assert.AreNotEqual(_target.Duration, clone.Duration);
            Assert.AreNotEqual(_target.RowsAffected, clone.RowsAffected);
            Assert.AreNotEqual(_target.Status, clone.Status);
            Assert.AreEqual(_target.HasError, clone.HasError);
            Assert.AreSame(_target.JobStepException, clone.JobStepException);
            Assert.AreEqual(_target.BusinessUnitStatus, clone.BusinessUnitStatus);
        }

        [Test]
        public void ShouldMakeADeepCloneWhenExceptionExist()
        {
            _target = new JobStepResult("Name", 2500, new NotImplementedException("eee"), _businessUnit, getJobResultCollection())
            {
	            Status = "Error"
            };

	        var clone = (JobStepResult)_target.Clone();

            Assert.AreEqual(_target.Name, clone.Name);
            Assert.AreSame(_target.CurrentBusinessUnit, clone.CurrentBusinessUnit);
            Assert.AreEqual(_target.Duration, clone.Duration);
            Assert.AreEqual(_target.RowsAffected, clone.RowsAffected);
            Assert.AreEqual(_target.Status, clone.Status);
            Assert.AreEqual(_target.HasError, clone.HasError);
            Assert.AreSame(_target.JobStepException, clone.JobStepException);
            Assert.AreEqual(_target.BusinessUnitStatus, clone.BusinessUnitStatus);

            IBusinessUnit otherBu = new BusinessUnit("other bu");
            _target = new JobStepResult("Other name", 1300, new NotImplementedException("yyyyy"), otherBu, null) { Status = "yoda" };

            Assert.AreNotEqual(_target.Name, clone.Name);
            Assert.AreNotSame(_target.CurrentBusinessUnit, clone.CurrentBusinessUnit);
            Assert.AreNotEqual(_target.Duration, clone.Duration);
            Assert.AreEqual(_target.RowsAffected, clone.RowsAffected);
            Assert.AreNotEqual(_target.Status, clone.Status);
            Assert.AreEqual(_target.HasError, clone.HasError);
            Assert.AreNotSame(_target.JobStepException, clone.JobStepException);
            Assert.AreNotEqual(_target.BusinessUnitStatus, clone.BusinessUnitStatus);
        }
    }
}