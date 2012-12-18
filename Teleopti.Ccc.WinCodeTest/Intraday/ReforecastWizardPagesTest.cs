using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.WinCode.Intraday;

namespace Teleopti.Ccc.WinCodeTest.Intraday
{
    [TestFixture]
    public class ReforecastWizardPagesTest
    {
        private ReforecastWizardPages _target;
        private MockRepository _mock;
        private ReforecastModelCollection _modelColletion;
        private ReforecastModel _model;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
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
    }
}
