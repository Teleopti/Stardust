using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.PropertyPageAndWizard;

namespace Teleopti.Ccc.WinCodeTest.Common.PropertyPageAndWizard
{
    /// <summary>
    /// Tests for the WizardNameChangedEventArgs class
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-01-15
    /// </remarks>
    [TestFixture]
    public class WizardNameChangedEventArgsTest
    {
        WizardNameChangedEventArgs target;

        /// <summary>
        /// Setups this instance.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-17
        /// </remarks>
        [SetUp]
        public void Setup()
        {
            target = new WizardNameChangedEventArgs("NewName1");
        }

        /// <summary>
        /// Verifies the can create.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-17
        /// </remarks>
        [Test]
        public void VerifyCanCreate()
        {
            Assert.IsNotNull(target);
        }

        /// <summary>
        /// Verifies the default properties.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-17
        /// </remarks>
        [Test]
        public void VerifyDefaultProperties()
        {
            Assert.AreEqual("NewName1", target.NewName);
        }
    }
}
