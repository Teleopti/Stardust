using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest.Common
{
    /// <summary>
    /// Tests for BusinessUnit Class
    /// </summary>
    [TestFixture]
    public class BusinessUnitTest
    {
        private BusinessUnit _target;
        
        /// <summary>
        /// Setups the tests.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _target = BusinessUnitFactory.CreateSimpleBusinessUnit("myName");
        }

        /// <summary>
        /// Test to make new BusinessUnit and checks the default Values
        /// </summary>
        [Test]
        public void CanCreateAndLoadDefaults()
        {
            Assert.AreEqual("myName", _target.Description.Name);
            Assert.IsNull(_target.UpdatedBy);
            Assert.IsNull(_target.UpdatedOn);
            Assert.IsNull(_target.Id);
            Assert.IsNull(_target.Version);
        }

        /// <summary>
        /// Verifies the doesnt belong to business unit.
        /// </summary>
        [Test]
        public void VerifyDoesNotBelongToBusinessUnit()
        {
            Assert.IsFalse(_target is IFilterOnBusinessUnit);
        }

        /// <summary>
        /// Make sure the constructor does not take null as argument
        /// </summary>
        [Test]
        public void CannotSetNameToNull()
        {
           Assert.Throws<ArgumentException>(() => _target = BusinessUnitFactory.CreateSimpleBusinessUnit(null));
        }

        /// <summary>
        /// Make sure the constructor does not take empty string as argument
        /// </summary>
        [Test]
        public void CannotSetEmptyName()
        {
			Assert.Throws<ArgumentException>(() => _target = BusinessUnitFactory.CreateSimpleBusinessUnit(string.Empty));
        }

        /// <summary>
        /// Can change name if not empty or null
        /// </summary>
        [Test]
        public void CanChangeName()
        {
            String myName = "name";
            String myNewName = "newName";
            _target = BusinessUnitFactory.CreateSimpleBusinessUnit(myName);
            _target.Description = new Description(myNewName);
            Assert.AreEqual(_target.Description.Name, myNewName);
        }

        /// <summary>
        /// Test protectedConstructorWorks
        /// </summary>
        [Test]
        public void TestProtectedConstructorForNHibernate()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(_target.GetType()));
        }

        /// <summary>
        /// Verifies that agents can be added.
        /// </summary>
        [Test]
        public void VerifyUnitsCanBeAdded()
        {
            int countBefore = _target.SiteCollection.Count;
            Site testUnit1 = SiteFactory.CreateSimpleSite();
            Site testUnit2 = SiteFactory.CreateSimpleSite();

            _target.AddSite(testUnit1);
            _target.AddSite(testUnit2);

            Assert.Contains(testUnit1, _target.SiteCollection);
            Assert.Contains(testUnit2, _target.SiteCollection);
            Assert.AreEqual(countBefore + 2, _target.SiteCollection.Count);
        }

        /// <summary>
        /// Duplicate layer instances should be ignored when added to list.
        /// </summary>
        [Test]
        public void DoNotDuplicateUnitInstancesWhenAddedToList()
        {
            int countBefore = _target.SiteCollection.Count;
            int lastPosBeforeAdded = countBefore - 1;
            Site testUnit = SiteFactory.CreateSimpleSite();

            _target.AddSite(testUnit);
            _target.AddSite(testUnit);
            _target.AddSite(testUnit);

            Assert.AreSame(testUnit, _target.SiteCollection[lastPosBeforeAdded + 1]);
            Assert.AreEqual(countBefore + 1, _target.SiteCollection.Count);
        }

        /// <summary>
        /// Null units are not allowed.
        /// </summary>
        [Test]
        public void NullUnitsAreNotAllowed()
        {
            Assert.Throws<ArgumentNullException>(() => _target.AddSite(null));
        }

        [Test]
        public void NullSitesAreNotAllowedToRemove()
        {
			Assert.Throws<ArgumentNullException>(() => _target.RemoveSite(null));
        }

        [Test]
        public void VerifyCanUseNameProperties()
        {
            _target.Name = "Test BU";
            _target.ShortName = "TBU";

            Assert.AreEqual("Test BU",_target.Name);
            Assert.AreEqual("TBU",_target.ShortName);
        }

        [Test]
        public void VerifyRemoveSiteFromBusinessUnit()
        {
            BusinessUnit bu = BusinessUnitFactory.CreateBusinessUnitWithSitesAndTeams();
            int siteCount = bu.SiteCollection.Count;

            bu.RemoveSite(bu.SiteCollection[0]);
            Assert.AreEqual(siteCount - 1, bu.SiteCollection.Count);
        }
    }
}