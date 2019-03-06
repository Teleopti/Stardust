using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest.Common

{
    /// <summary>
    /// Tests for Scenario class
    /// </summary>
    [TestFixture]
	[TestWithStaticDependenciesDONOTUSE]
	public class ScenarioTest
    {
        private Scenario _scenario;

        /// <summary>
        /// Runs once per test.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _scenario = new Scenario("Zefault") {DefaultScenario = true};
        }

        /// <summary>
        /// Can create Scenario and properties are correctly set to defualt values.
        /// </summary>
        [Test]
        public void CanCreateAndPropertiesAreSet()
        {
            Assert.AreEqual(null, _scenario.Id);
            Assert.AreEqual("Zefault", _scenario.Description.Name);
            Assert.AreEqual(_scenario.DefaultScenario,true);
            Assert.AreEqual(_scenario.EnableReporting, false);
            Assert.AreEqual(BusinessUnitUsedInTests.BusinessUnit, _scenario.GetOrFillWithBusinessUnit_DONTUSE());
            Assert.IsNull(_scenario.UpdatedBy);
            Assert.IsNull(_scenario.UpdatedOn);
            Assert.IsNull(_scenario.Version);
			Assert.IsFalse(_scenario.Restricted);
        }

        /// <summary>
        /// Can set name
        /// </summary>
        [Test]
        public void CanSetWorkspaceName()
        {
            _scenario.ChangeName("Dummy");
            Assert.AreEqual("Dummy", _scenario.Description.Name);
        }

		/// <summary>
        /// Can set defaultworkspace
        /// </summary>
        [Test]
        public void CanSetDefaultWorkspace()
        {
            _scenario.DefaultScenario = true;
            Assert.AreEqual(_scenario.DefaultScenario, true);
        }

        /// <summary>
        /// Can set reportable
        /// </summary>
        [Test]
        public void CanSetReportable()
        {
            _scenario.EnableReporting = true;
            Assert.AreEqual(_scenario.EnableReporting, true);
        }

        /// <summary>
        /// Verifies the protected constructor works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-10-26
        /// </remarks>
        [Test]
        public void VerifyProtectedConstructorWorks()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(_scenario.GetType(), true));
        }

        [Test]
        public void VerifyComparableWorks()
        {
            var theList = new List<IScenario>();
            theList.Add(_scenario);
            theList.Add(new Scenario("a"));
            theList.Sort();
            Assert.AreEqual("a", theList[0].Description.Name);
        }

        [Test]
        public void VerifyToString()
        {
            string correct = "Scenario, no id Zefault";
            Assert.AreEqual(correct, _scenario.ToString());
        }

		[Test]
		public void ShouldSetRestricted()
		{
			Assert.IsFalse(_scenario.Restricted);

			_scenario.Restricted = true;
			Assert.IsTrue(_scenario.Restricted);

			_scenario.Restricted = false;
			Assert.IsFalse(_scenario.Restricted);
		}
    }
}