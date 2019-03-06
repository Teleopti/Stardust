using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Common
{
    /// <summary>
    /// Tests for PartTimePercentage class.
    /// </summary>
    [TestFixture]
	[TestWithStaticDependenciesDONOTUSE]
	public class PartTimePercentageTest
    {
        private PartTimePercentage testPartTimePercentage;

        /// <summary>
        /// Setups the tests.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            testPartTimePercentage = new PartTimePercentage("test");
        }

        /// <summary>
        /// Verifies that default properties are set
        /// </summary>
        [Test]
        public void VerifyDefaultPropertiesAreSet()
        {
            Assert.AreEqual("test", testPartTimePercentage.Description.Name);
            Assert.AreEqual(BusinessUnitUsedInTests.BusinessUnit, testPartTimePercentage.GetOrFillWithBusinessUnit_DONTUSE());
            Assert.AreEqual(1d, testPartTimePercentage.Percentage.Value);
            Assert.IsTrue(testPartTimePercentage.IsChoosable);
        }

        /// <summary>
        /// Verifies that name can be set
        /// </summary>
        [Test]
        public void VerifyNameCanBeSet()
        {
            testPartTimePercentage.Description = new Description("new");
            Assert.AreEqual("new", testPartTimePercentage.Description.Name);
        }

        /// <summary>
        /// Verifies that employment type for contract can be set
        /// </summary>
        [Test]
        public void VerifyPercentageCanBeSet()
        {
            testPartTimePercentage.Percentage = new Percent(0.75d);
            Assert.AreEqual(0.75d, testPartTimePercentage.Percentage.Value);
        }

        /// <summary>
        /// Verifies that percentage is withing valid range.
        /// </summary>
        [Test]
        public void VerifyValidPercentageCanSet()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => testPartTimePercentage.Percentage = new Percent(1.3d));
        }

        [Test]
        public void VerifyPercentCannotBeSetBelowZero()
        {
			Assert.Throws<ArgumentOutOfRangeException>(() => testPartTimePercentage.Percentage = new Percent(-0.8d));
        }

        [Test]
        public void VerifyPercentCanBeSetToZeroAndToOneHundred()
        {
            testPartTimePercentage.Percentage = new Percent(0.0d);
            Assert.AreEqual(0.0d, testPartTimePercentage.Percentage.Value);

            testPartTimePercentage.Percentage = new Percent(1.0d);
            Assert.AreEqual(1.0d, testPartTimePercentage.Percentage.Value);
        }

        /// <summary>
        /// Verifies that percentage can be overwritten.
        /// </summary>
        [Test]
        public void VerifyPercentageCanSet2()
        {
            testPartTimePercentage.Percentage = new Percent(0.3d);
            testPartTimePercentage.Percentage = new Percent(0.75d);
            Assert.AreEqual(0.75d, testPartTimePercentage.Percentage.Value);
        }

        /// <summary>
        /// Protected constructor works.
        /// </summary>
        [Test]
        public void ProtectedConstructorWorks()
        {
            MockRepository mocks = new MockRepository();
            PartTimePercentage protPartTimePercentage = mocks.StrictMock<PartTimePercentage>();
            Assert.IsNotNull(protPartTimePercentage);
        }
    }
}