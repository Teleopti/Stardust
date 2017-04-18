using System.Globalization;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin.Models
{
    /// <summary>
    /// ExternalLogOnModelTest
    /// </summary>
    /// <remarks>
    /// Created by: Dinesh Ranasinghe
    /// Created date: 2008-08-15
    /// </remarks>
  [TestFixture]
  public class ExternalLogOnModelTest
    {
        private ExternalLogOnModel _target;
        private ExternalLogOn _externalLogOn;

        [SetUp]
        public void TestInit()
        {
            _target = new ExternalLogOnModel();

            _externalLogOn = ExternalLogOnFactory.CreateExternalLogOn();
            _target.ContainedEntity = _externalLogOn;
        }

        /// <summary>
        /// Tests the dispose.
        /// </summary>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-08-15
        /// </remarks>
        [TearDown]
        public void TestDispose()
        {
            _externalLogOn = null;
            _target = null;
        }

        /// <summary>
        /// Verifies the current properties.
        /// </summary>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-06-15
        /// </remarks>
        [Test]
        public void VerifyCurrentProperties()
        {
            Assert.IsNotNull(_target);
            Assert.IsNotNull(_target.DescriptionText);
        }

        /// <summary>
        /// Verifies the can get person skill.
        /// </summary>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-06-15
        /// </remarks>
        [Test]
        public void VerifyCanGetPersonSkill()
        {
            Assert.AreEqual(_externalLogOn, _target.ExternalLogOn);
        }

        /// <summary>
        /// Verifies the state of the can get and set tri.
        /// </summary>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-08-15
        /// </remarks>
        [Test]
        public void VerifyCanGetAndSetTriState()
        {
            _target.TriState = 2;
            Assert.AreEqual(2, _target.TriState);
        }

        /// <summary>
        /// Verifies the can get and set person skill exists in person count.
        /// </summary>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-08-15
        /// </remarks>
        [Test]
        public void VerifyCanGetAndSetPersonSkillExistsInPersonCount()
        {
            _target.ExternalLogOnInPersonCount = 2;
            Assert.AreEqual(2, _target.ExternalLogOnInPersonCount);
        }

        /// <summary>
        /// Verifies the can get description.
        /// </summary>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-08-15
        /// </remarks>
        [Test]
        public void VerifyCanGetDescription()
        {
            Assert.AreEqual(string.Format(CultureInfo.CurrentCulture, "{0}", _externalLogOn.AcdLogOnName), _target.DescriptionText);
        }

		[Test]
		public void VerifyCanGetAcdDescription()
		{
			Assert.AreEqual(string.Format(CultureInfo.CurrentCulture, "{0}", _externalLogOn.DataSourceId), _target.AcdText);
		}

		[Test]
		public void ShouldGetLogObjectName()
		{
			Assert.AreEqual(string.Format(CultureInfo.CurrentCulture, "{0}", _externalLogOn.DataSourceName), _target.LogObjectName);	
		}
    }
}
