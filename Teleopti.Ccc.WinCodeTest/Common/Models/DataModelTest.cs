using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Models;
using Teleopti.Ccc.WinCodeTest.Helpers;

namespace Teleopti.Ccc.WinCodeTest.Common.Models
{
    [TestFixture]
    public class DataModelTest
    {
        private DataModel _target;

        [SetUp]
        public void Setup()
        {
            _target = new DataModelForTest();
        }

        [Test]
        public void VerifyStateProperty()
        {
            PropertyChangedListener listener = new PropertyChangedListener();
            listener.ListenTo(_target);
            _target.State = ModelState.Active;
            Assert.AreEqual(ModelState.Active,_target.State);
            Assert.IsTrue(listener.HasFired("State"));
        }

        [Test]
        public void VerifyTypeSafePropertyChanged()
        {
            PropertyChangedListener listener = new PropertyChangedListener();
            listener.ListenTo(_target);
            ((DataModelForTest)_target).TestProperty = "some stuff";
            Assert.IsTrue(listener.HasFired("TestProperty"));
        }

        [Test]
        public void ShouldHaveNonNullDispatcher()
        {
            Assert.IsNotNull(_target.Dispatcher);
        }

      
    }
}
