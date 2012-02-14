using NUnit.Framework;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Presentation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Presentation
{
    /// <summary>
    /// Tests for PersonListBoxPresenter Class
    /// </summary>
    [TestFixture]
    public class PersonListBoxPresenterTest
    {
        private IPerson _entity;
        private PersonListBoxPresenter _target;
        private string _commonNameSettingString = "{LastName} - {FirstName}";
        private CommonNameDescriptionSetting _commonNameDescriptionSetting;

        /// <summary>
        /// Setups the tests.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _entity = PersonFactory.CreatePerson("FirstName", "LastName");
            _commonNameDescriptionSetting = new CommonNameDescriptionSetting(_commonNameSettingString);
            _target = new PersonListBoxPresenter(_entity, _commonNameDescriptionSetting);
        }

        #region Verify constructor

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target);
        }

        #endregion

        #region Verify IListBoxPresenter interface members

        [Test]
        public void VerifyDataBindText()
        {
            string expNameValue = "LastName - FirstName";
            string getNameValue = _target.DataBindText;
            Assert.AreEqual(expNameValue, getNameValue);
        }

        [Test]
        public void VerifyDataBindDescriptionText()
        {
            string expNameValue = "LastName - FirstName";
            string getNameValue = _target.DataBindDescriptionText;
            Assert.AreEqual(expNameValue, getNameValue);
        }

        #endregion
    }
}