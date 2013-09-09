using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Presentation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Presentation
{
    /// <summary>
    /// Tests for TeamListBoxPresenter Class
    /// </summary>
    [TestFixture]
    public class TeamListBoxPresenterTest
    {
        private Team _entity;
        private TeamListBoxPresenter _target;

        /// <summary>
        /// Setups the tests.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _entity = TeamFactory.CreateSimpleTeam("myTeam");
            _target = new TeamListBoxPresenter(_entity);
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
        public void VerifyNameValue()
        {

            // Declare return variable to hold property get method
            string getNameValue;
            string setNameValue = "NAME";

            // call set method
            _entity.Description = new Description("DESC", setNameValue);

            // call get method
            getNameValue = _target.DataBindText;

            // Assert result
            Assert.AreEqual(setNameValue, getNameValue);
        }

        [Test]
        public void VerifyDescriptionValue()
        {

            // Declare variable to hold property set method
            string setDescriptionValue = "DESCRIPTION";

            // call set method
            _entity.Description = new Description(setDescriptionValue, "");

            // Declare return variable to hold property get method
            string getDescriptionValue;

            // call get method
            getDescriptionValue = _target.DataBindDescriptionText;

            // Assert result
            Assert.AreEqual(setDescriptionValue, getDescriptionValue);
        }

        #endregion 
    }
}