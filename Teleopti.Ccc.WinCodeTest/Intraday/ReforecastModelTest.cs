using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Intraday;

namespace Teleopti.Ccc.WinCodeTest.Intraday
{
    [TestFixture]
    public class ReforecastModelTest
    {
        private ReforecastModel _target;

        [SetUp]
        public void Setup()
        {
            _target = new ReforecastModel();
        }

        [Test]
        public void ShouldInitializeWorkloadList()
        {
            Assert.That(_target.Workload, Is.Not.Null);
        }
    }

    [TestFixture]
    public class ReforecastModelCollectionTest
    {
        private ReforecastModelCollection _target;

        [SetUp]
        public void Setup()
        {
            _target = new ReforecastModelCollection();
        }

        [Test]
        public void ShouldInitializeWorkloadList()
        {
            Assert.That(_target.ReforecastModels, Is.Not.Null);
        }
    }
}
