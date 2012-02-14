using System.ComponentModel;
using NUnit.Framework;
using Teleopti.Ccc.WinCode.Common.Models;
using Teleopti.Ccc.WinCodeTest.Helpers;

namespace Teleopti.Ccc.WinCodeTest.Common.Models
{
    [TestFixture]
    public class DataModelWithDataErrorInfoTest
    {
        private const string FakeProperty = "Fakeproperty";
        private DataModelWithInvalidState _target;

        [SetUp]
        public  void Setup()
        {
            _target = new DataModelWithInvalidStateForTest(FakeProperty);
        }

        [Test]
        public void VerifyDefaults()
        {
            Assert.IsTrue(_target.IsValid);
            Assert.AreEqual(UserTexts.Resources.InvalidCredential, _target.Error);
           
        }

        [Test]
        public void VerifyIsValidStateChangesWhenDataError()
        {
            PropertyChangedListener listener = new PropertyChangedListener();
            listener.ListenTo(_target);
           
            var result = ((IDataErrorInfo) _target)[FakeProperty];
            Assert.IsNotNull(result);
            Assert.IsFalse(_target.IsValid);
            Assert.IsTrue(listener.HasFired("IsValid"));

            //ChangeBack
            listener = new PropertyChangedListener();
            listener.ListenTo(_target);

            result = ((IDataErrorInfo)_target)["AnyOtherProperty"];
            Assert.IsNull(result);
            Assert.IsTrue(_target.IsValid);
            Assert.IsTrue(listener.HasFired("IsValid"));


        }


    }
}
