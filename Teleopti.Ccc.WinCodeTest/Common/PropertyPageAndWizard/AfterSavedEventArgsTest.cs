using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Common.PropertyPageAndWizard
{
    /// <summary>
    /// Tests for the AfterSavedEventArgs class
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-01-15
    /// </remarks>
    [TestFixture]
    public class AfterSavedEventArgsTest
    {
        AfterSavedEventArgs target;

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
            target = new AfterSavedEventArgs();
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
            Assert.IsNull(target.SavedAggregateRoot);
        }

        /// <summary>
        /// Verifies can set properties.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-17
        /// </remarks>
        [Test]
        public void VerifyCanSetProperties()
        {
            MockRepository mocks = new MockRepository();
            IAggregateRoot aggregateRoot = mocks.StrictMock<IAggregateRoot>();

            target.SavedAggregateRoot = aggregateRoot;
            Assert.AreEqual(aggregateRoot, target.SavedAggregateRoot);
        }
    }
}
