using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Payroll;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll.PayrollExportPages.PayrollExportSmartPart;


namespace Teleopti.Ccc.WinCodeTest.Payroll.PayrollExportSmartPart
{
    [TestFixture]
    public class PayrollResultViewModelTest
    {
        private PayrollResultViewModel _target;
        private IPayrollResult _result;
        private IPayrollExport _export;
        private IPerson _person;
        private DateTime _dateTime;

        [SetUp]
        public void Setup()
        {
            _dateTime = DateTime.UtcNow;
            _person = new Person();
            _export = new PayrollExport();
            _result = new PayrollResult(_export, _person, _dateTime);
            _target = new PayrollResultViewModel(_result);
        }

        [Test]
        public void ShouldHandleResultWithError()
        {
            _result.AddDetail(new PayrollResultDetail(DetailLevel.Error,"error!",DateTime.UtcNow,null));
            Assert.IsAssignableFrom<StatusError>(_target.Status);
        }

        [Test]
        public void ShouldHandleResultWithTimeout()
        {
            _result = new PayrollResult(_export, _person, _dateTime.AddHours(-12.5));
            _target = new PayrollResultViewModel(_result);
            Assert.IsAssignableFrom<StatusError>(_target.Status);
        }

		[Test]
		public void ShouldHaveValidProgressFromStart()
		{
			_target.Progress.Message.Should().Be.EqualTo(UserTexts.Resources.WaitingThreeDots);
		}

        [Test]
        public void ShouldHandleResultInProgress()
        {
            Assert.IsAssignableFrom<StatusWorking>(_target.Status);
        }

        [Test]
        public void ShouldHandleResultDone()
        {
            _result.AddDetail(new PayrollResultDetail(DetailLevel.Info, "info!", DateTime.UtcNow, null));
            _result.XmlResult.SetResult(new FakeXml());
            Assert.IsAssignableFrom<StatusDone>(_target.Status);
            Assert.AreEqual("info!",_target.Progress.Message);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(_target.Owner,_result.Owner.Name.ToString());
            Assert.AreEqual(_target.PayrollFormatName,_result.PayrollFormatName);
            Assert.AreEqual(_target.Period,_result.Period);
            Assert.AreEqual(_target.Timestamp, _result.Timestamp);
            Assert.AreEqual(_target.Model, _result);
        }

        [Test]
        public void ShouldSetProgress()
        {
            bool progressUpdated = false;
            _target.PropertyChanged += (sender, e) => { if (e.PropertyName == "Progress") progressUpdated = true; };
            
            var guid = Guid.NewGuid();
            _result.SetId(guid);
            var p = new JobResultProgress();
            p.JobResultId = guid;
            p.Percentage = 42;
            p.Message = "Hej";

            _target.TrySetProgress(p);

            Assert.AreEqual(_target.Progress, p);
            Assert.IsTrue(progressUpdated);
        }

        [Test]
        public void ShouldNotSetProgress()
        {
            var guid = Guid.NewGuid();
            _result.SetId(guid);
            var p = new JobResultProgress();
            p.JobResultId = Guid.NewGuid();
            p.Percentage = 42;
            p.Message = "Hej";

            _target.TrySetProgress(p);

            Assert.AreNotEqual(_target.Progress, p);
        }

        [Test]
        public void ModelMustNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => _target = new PayrollResultViewModel(null));
        }

        [Test]
        public void DetailsShouldBeSorted()
        {
            var timeStamp1 = new DateTime(2011, 3, 1, 2, 3, 14, 12);
            var timeStamp2 = timeStamp1.AddMilliseconds(1);
            var timeStamp3 = timeStamp1.AddMilliseconds(2);
            var timeStamp4 = timeStamp1.AddMilliseconds(7);

            _result.AddDetail(new PayrollResultDetail(DetailLevel.Info, "Information", timeStamp1, null));
            _result.AddDetail(new PayrollResultDetail(DetailLevel.Info, "Information", timeStamp4, null));
            _result.AddDetail(new PayrollResultDetail(DetailLevel.Info, "Information", timeStamp2, null));
            _result.AddDetail(new PayrollResultDetail(DetailLevel.Info, "Information", timeStamp3, null));

            _target.Details.ElementAt(0).Timestamp.Should().Be.EqualTo(timeStamp4);
            _target.Details.ElementAt(1).Timestamp.Should().Be.EqualTo(timeStamp3);
            _target.Details.ElementAt(2).Timestamp.Should().Be.EqualTo(timeStamp2);
            _target.Details.ElementAt(3).Timestamp.Should().Be.EqualTo(timeStamp1);
        }

    }
}
