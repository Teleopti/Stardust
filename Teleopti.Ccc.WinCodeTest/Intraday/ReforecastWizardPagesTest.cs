using System;
using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Intraday;

namespace Teleopti.Ccc.WinCodeTest.Intraday
{
    [TestFixture]
    public class ReforecastWizardPagesTest : IDisposable
    {
        private ReforecastWizardPages _target;
        private ReforecastModelCollection _modelColletion;
        private ReforecastModel _model;

        [SetUp]
        public void Setup()
        {
            _model = new ReforecastModel();
            _modelColletion = new ReforecastModelCollection();
            _modelColletion.ReforecastModels.Add(_model);
            _target = new ReforecastWizardPages(_modelColletion);
        }

        [Test]
        public void ShouldHaveName()
        {
            Assert.That(_target.Name, Is.Not.Null);
        }

        [Test]
        public void ShouldHaveWindowText()
        {
            Assert.That(_target.WindowText, Is.Not.Null);
        }

        [Test]
        public void ShouldReturnObject()
        {
            Assert.That(_target.CreateNewStateObj(), Is.Not.Null);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
                ReleaseResources();

        }

        private void ReleaseResources()
        {
            _target = new ReforecastWizardPages(null);
            _target.Dispose();
        }
    }
}
