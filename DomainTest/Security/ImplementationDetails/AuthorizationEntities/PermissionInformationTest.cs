using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest.Security.ImplementationDetails.AuthorizationEntities
{
    [TestFixture]
    public class PermissionInformationTest
    {
        private IPermissionInformation target;
        private IPerson person;

        [SetUp]
        public void Setup()
        {
            person = new Person();
            target = person.PermissionInformation;
        }

        [Test]
        public void VerifyDefaultProperties()
        {
            Assert.AreEqual(0, target.ApplicationRoleCollection.Count);
           Assert.AreEqual(Thread.CurrentThread.CurrentCulture, target.Culture());
            Assert.AreEqual(Thread.CurrentThread.CurrentUICulture, target.UICulture());
            Assert.AreEqual(TimeZoneInfo.Local.Id, ((PermissionInformation)target).DefaultTimeZone().Id);
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [Test]
        public void VerifyProtectedProperty()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(typeof(PermissionInformation)));
        }

        #region Application role
        [Test]
        public void VerifyBusinessUnitAccessCollection()
        {
            IApplicationRole normalRole = new ApplicationRole();
            IApplicationRole deletedBuRole = new ApplicationRole();

            IBusinessUnit normalBu = BusinessUnitUsedInTests.BusinessUnit;
            IBusinessUnit deletedBu = new BusinessUnit("deleted");
            ((IDeleteTag)deletedBu).SetDeleted();

            normalRole.SetBusinessUnit(normalBu);
            deletedBuRole.SetBusinessUnit(deletedBu);

            target.AddApplicationRole(normalRole);
            target.AddApplicationRole(deletedBuRole);

            Assert.AreEqual(1, target.BusinessUnitAccessCollection().Count);
            Assert.IsTrue(target.BusinessUnitAccessCollection().Contains(BusinessUnitUsedInTests.BusinessUnit));
            Assert.IsFalse(target.BusinessUnitAccessCollection().Contains(deletedBu));

        }

        [Test]
        public void CanRemoveApplicationRole()
        {
            ApplicationRole role = new ApplicationRole();
            target.AddApplicationRole(role);
            Assert.AreEqual(1, target.ApplicationRoleCollection.Count);
            target.RemoveApplicationRole(role);
            Assert.AreEqual(0, target.ApplicationRoleCollection.Count);
        }

        [Test]
        public void CanAddApplicationRole()
        {
            int rolesAtStart = target.ApplicationRoleCollection.Count;
            ApplicationRole role = new ApplicationRole();
            target.AddApplicationRole(role);
            //no duplicates
            target.AddApplicationRole(role);
            Assert.AreEqual(1, target.ApplicationRoleCollection.Count - rolesAtStart);
            Assert.IsTrue(target.ApplicationRoleCollection.Contains(role));
        }

        [Test]
        public void CannotAddNullAsApplicationRole()
        {
			Assert.Throws<ArgumentNullException>(() => target.AddApplicationRole(null));
        }

        [Test]
        public void VerifyApplicationRoleCollectionIsLocked()
        {
			Assert.Throws<NotSupportedException>(() => target.ApplicationRoleCollection.Add(new ApplicationRole()));
        }
        #endregion

        #region Time zone
        [Test]
        public void CanSetTimeZone()
        {
            ((PermissionInformation)target).SetDefaultTimeZone((TimeZoneInfo.Utc));
            Assert.AreEqual(TimeZoneInfo.Utc, ((PermissionInformation)target).DefaultTimeZone());
        }

        /// <summary>
        /// Verifies the not null as time zone.
        /// </summary>
        [Test]
        public void VerifyNotNullAsTimeZone()
        {
			Assert.Throws<ArgumentNullException>(() => ((PermissionInformation)target).SetDefaultTimeZone(null));
        }
        #endregion

        #region Culture
        /// <summary>
        /// Determines whether this instance [can set culture].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-30
        /// </remarks>
        [Test]
        public void CanSetCulture()
        {
            CultureInfo ci = CultureInfo.GetCultureInfo("de-AT");

            target.SetCulture(ci);
            Assert.AreEqual(ci, target.Culture());
            Assert.AreEqual(3079, target.CultureLCID());
        }

        /// <summary>
        /// Verifies the not null as culture.
        /// </summary>
        [Test]
        //[ExpectedException(typeof(ArgumentNullException))]
        public void VerifyNotNullAsCulture()
        {
            target.SetCulture(null);
            Assert.AreEqual(null, target.CultureLCID());
        }
        #endregion

        #region UI Culture
        /// <summary>
        /// Determines whether this instance [can set UI culture].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-30
        /// </remarks>
        [Test]
        public void CanSetUICulture()
        {
            CultureInfo ci = CultureInfo.GetCultureInfo("de-AT");

            target.SetUICulture(ci);
            Assert.AreEqual(ci, target.UICulture());
            Assert.AreEqual(3079, target.UICultureLCID());
        }

        /// <summary>
        /// Verifies the not null as UI culture.
        /// </summary>
        [Test]
        //[ExpectedException(typeof(ArgumentNullException))]
        public void VerifyNotNullAsUICulture()
        {
            target.SetUICulture(null);
            Assert.AreEqual(null, target.UICultureLCID());
        }
        #endregion

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifyICloneableEntity()
        {
            IPerson p = new Person();

            IPermissionInformation originalPermissionInfo = p.PermissionInformation;
            
            ApplicationRole role = new ApplicationRole();
            originalPermissionInfo.AddApplicationRole(role);

            IPermissionInformation clonedPermissionInfo = (IPermissionInformation)originalPermissionInfo.Clone();
            Assert.AreEqual(originalPermissionInfo.ApplicationRoleCollection.Count, clonedPermissionInfo.ApplicationRoleCollection.Count);

        }

        [Test]
        public void VerifyNoneEntityClone()
        {
			Assert.Throws<NotImplementedException>(() => target.NoneEntityClone());
        }


        [Test]
        public void IfUICultureInfoParentArabicRightToLeftShouldBeUsed()
        {
            CultureInfo ci = CultureInfo.GetCultureInfo("de-AT");
            target.SetUICulture(ci);
            Assert.IsFalse(target.RightToLeftDisplay);

            ci = CultureInfo.GetCultureInfo("ar-SA");
            target.SetUICulture(ci);
            Assert.IsTrue(target.RightToLeftDisplay);
        }
    }
}