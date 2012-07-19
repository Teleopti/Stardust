using System;
using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
    /// <summary>
    /// Test class for Absence
    /// </summary>
    [TestFixture]
    public class AbsenceTest
    {
        private Absence target;
        private GroupingAbsence groupingAbsence;

        /// <summary>
        /// Runs once for every test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            target = new Absence();
            groupingAbsence = new GroupingAbsence("Test");
        }

        /// <summary>
        /// Verifies that an instance can be created.
        /// </summary>
        [Test]
        public void CanCreateAbsence()
        {
            Assert.IsNotNull(target);
            Assert.AreEqual(100, target.Priority);
        }


        [Test]
        public void CanAlwaysGetRealDisplayValuesIfAbsenceBelongsToMe()
        {
            target.DisplayColor = Color.Red;
            target.Confidential = true;
            var loggedOn = ((IUnsafePerson)TeleoptiPrincipal.Current).Person;
            Assert.AreEqual(target.DisplayColor, target.ConfidentialDisplayColor(loggedOn,DateOnly.Today));
            Assert.AreEqual(target.Description, target.ConfidentialDescription(loggedOn,DateOnly.Today));
        }

        /// <summary>
        /// Verifies that properties can be set.
        /// </summary>
        [Test]
        public void CanSetProperties()
        {
            target.DisplayColor = Color.DarkSalmon;
            target.Description = new Description("Sjuk","SJ");
            target.GroupingAbsence = groupingAbsence;
            target.Priority = 37;
            target.InWorkTime = true;
            target.InPaidTime = true;
            target.PayrollCode = "aabbcc";

            Assert.AreEqual(Color.DarkSalmon.ToArgb(), target.DisplayColor.ToArgb());
            Assert.AreEqual("Sjuk", target.Description.Name);
            Assert.AreEqual("SJ", target.Description.ShortName);
            Assert.AreSame(BusinessUnitFactory.BusinessUnitUsedInTest, target.BusinessUnit);
            Assert.AreEqual("Test", target.GroupingAbsence.Description.Name);
            Assert.AreEqual(37, target.Priority);
            Assert.IsFalse(target.Requestable);
            target.Requestable = true;
            Assert.IsTrue(target.Requestable);
            Assert.AreEqual("Sjuk", target.Name);
            Assert.IsTrue(target.InWorkTime);
            Assert.IsTrue(target.InPaidTime);
            Assert.AreEqual("aabbcc",target.PayrollCode );
        }

        [Test]
        public void VerifyConfidentialValues()
        {
            Assert.IsFalse(target.Confidential);
            target.Confidential = true;
            using(new CustomAuthorizationContext(new PrincipalAuthorizationWithNoPermission()))
            {
                Assert.AreEqual(ConfidentialPayloadValues.Description, target.ConfidentialDescription(null,DateOnly.Today));
                Assert.AreEqual(ConfidentialPayloadValues.DisplayColor, target.ConfidentialDisplayColor(null,DateOnly.Today));
            }
        }


        /// <summary>
        /// GroupingAbsence may not be null
        /// </summary>
        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void VerifyThatExceptionIsThrownWhenGroupingAbsenceIsNotNull()
        {
            target.GroupingAbsence = null;
        }

        /// <summary>
        /// Change the parent GroupingAbsence
        /// </summary>
        [Test]
        public void VerifyGroupingAbsenceCanBeChanged()
        {
            GroupingAbsence newGroupingAbsence = new GroupingAbsence("Holiday");
            target.GroupingAbsence = groupingAbsence;
            target.GroupingAbsence = newGroupingAbsence;
        }

        /// <summary>
        /// Verifies the InContractTime property works.
        /// </summary>
        [Test]
        public void VerifyInContractTime()
        {
            bool getValue;
            bool setValue;

            getValue = target.InContractTime;
            setValue = !getValue;
            target.InContractTime = setValue;

            getValue = target.InContractTime;
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyCanSetGetTracker()
        {
            target.Tracker = Tracker.CreateDayTracker();
            Assert.AreEqual(target.Tracker, Tracker.CreateDayTracker());
        }

        [Test]
        public void VerifyClone()
        {
            ((IAbsence) target).SetId(Guid.NewGuid());

            IAbsence absenceClone = (IAbsence) target.Clone();
            Assert.IsNull(absenceClone.Id);
            Assert.AreEqual(target.Confidential, absenceClone.Confidential);
            Assert.AreEqual(target.Description, absenceClone.Description);
            Assert.AreEqual(target.DisplayColor, absenceClone.DisplayColor);
            Assert.AreEqual(target.GroupingAbsence, absenceClone.GroupingAbsence);
            Assert.AreEqual(target.InPaidTime, absenceClone.InPaidTime);
            Assert.AreEqual(target.InContractTime, absenceClone.InContractTime);
            Assert.AreEqual(target.BusinessUnit, absenceClone.BusinessUnit);
            Assert.AreEqual(target.InWorkTime, absenceClone.InWorkTime);
            Assert.AreEqual(target.Name, absenceClone.Name);
            Assert.AreEqual(target.PayrollCode, absenceClone.PayrollCode);
            Assert.AreEqual(target.Priority, absenceClone.Priority);
            Assert.AreEqual(target.Requestable, absenceClone.Requestable);
            Assert.AreEqual(target.Tracker, absenceClone.Tracker);

            absenceClone = target.NoneEntityClone();
            Assert.IsNull(absenceClone.Id);
            Assert.AreEqual(target.Confidential, absenceClone.Confidential);
            Assert.AreEqual(target.Description, absenceClone.Description);
            Assert.AreEqual(target.DisplayColor, absenceClone.DisplayColor);
            Assert.AreEqual(target.GroupingAbsence, absenceClone.GroupingAbsence);
            Assert.AreEqual(target.InPaidTime, absenceClone.InPaidTime);
            Assert.AreEqual(target.InContractTime, absenceClone.InContractTime);
            Assert.AreEqual(target.BusinessUnit, absenceClone.BusinessUnit);
            Assert.AreEqual(target.InWorkTime, absenceClone.InWorkTime);
            Assert.AreEqual(target.Name, absenceClone.Name);
            Assert.AreEqual(target.PayrollCode, absenceClone.PayrollCode);
            Assert.AreEqual(target.Priority, absenceClone.Priority);
            Assert.AreEqual(target.Requestable, absenceClone.Requestable);
            Assert.AreEqual(target.Tracker, absenceClone.Tracker);

            absenceClone = target.EntityClone();
            Assert.AreEqual(target.Id, absenceClone.Id);
            Assert.AreEqual(target.Confidential, absenceClone.Confidential);
            Assert.AreEqual(target.Description, absenceClone.Description);
            Assert.AreEqual(target.DisplayColor, absenceClone.DisplayColor);
            Assert.AreEqual(target.GroupingAbsence, absenceClone.GroupingAbsence);
            Assert.AreEqual(target.InPaidTime, absenceClone.InPaidTime);
            Assert.AreEqual(target.InContractTime, absenceClone.InContractTime);
            Assert.AreEqual(target.BusinessUnit, absenceClone.BusinessUnit);
            Assert.AreEqual(target.InWorkTime, absenceClone.InWorkTime);
            Assert.AreEqual(target.Name, absenceClone.Name);
            Assert.AreEqual(target.PayrollCode, absenceClone.PayrollCode);
            Assert.AreEqual(target.Priority, absenceClone.Priority);
            Assert.AreEqual(target.Requestable, absenceClone.Requestable);
            Assert.AreEqual(target.Tracker, absenceClone.Tracker);

        }
    }
}