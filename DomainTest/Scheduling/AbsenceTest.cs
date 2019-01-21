using System;
using System.Drawing;
using System.IdentityModel.Claims;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling
{
    /// <summary>
    /// Test class for Absence
    /// </summary>
    [TestFixture]
	[TestWithStaticDependenciesDONOTUSE]
	public class AbsenceTest
    {
        private Absence target;

        /// <summary>
        /// Runs once for every test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            target = new Absence();
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
            var loggedOn = TestWithStaticDependenciesDONOTUSEAttribute.loggedOnPerson;
            Assert.AreEqual(target.DisplayColor, target.ConfidentialDisplayColor(loggedOn));
            Assert.AreEqual(target.Description, target.ConfidentialDescription(loggedOn));
        }

        /// <summary>
        /// Verifies that properties can be set.
        /// </summary>
        [Test]
        public void CanSetProperties()
        {
            target.DisplayColor = Color.DarkSalmon;
            target.Description = new Description("Sjuk","SJ");
            target.Priority = 37;
            target.InWorkTime = true;
            target.InPaidTime = true;
            target.PayrollCode = "aabbcc";

            Assert.AreEqual(Color.DarkSalmon.ToArgb(), target.DisplayColor.ToArgb());
            Assert.AreEqual("Sjuk", target.Description.Name);
            Assert.AreEqual("SJ", target.Description.ShortName);
            Assert.AreEqual(BusinessUnitFactory.BusinessUnitUsedInTest, target.BusinessUnit);
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
            using(CurrentAuthorization.ThreadlyUse(new NoPermission()))
            {
                Assert.AreEqual(ConfidentialPayloadValues.Description, target.ConfidentialDescription(null));
                Assert.AreEqual(ConfidentialPayloadValues.DisplayColor, target.ConfidentialDisplayColor(null));
            }
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

		[Test]
		public void ShouldShowRealDisplayValuesToAllowedOnBusinessUnitWhenAgentIsTerminatedAndAbsenceIsConfidential()
		{
			var personAccountUpdater = new PersonAccountUpdaterDummy();
			var queryingPerson = PersonFactory.CreatePerson();
			var site = SiteFactory.CreateSimpleSite();
			var team = TeamFactory.CreateSimpleTeam();
			site.AddTeam(team);

			var targetPerson = PersonFactory.CreatePersonWithId();
			queryingPerson.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today.AddDays(-10), team));
			targetPerson.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today.AddDays(-10), team));
			targetPerson.TerminatePerson(DateOnly.Today.AddDays(-5), personAccountUpdater);

			var principal = new TeleoptiPrincipalForLegacy(new TeleoptiIdentity("test", null, null, null, null, null), queryingPerson);
			var claimType = string.Concat(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace, "/", DefinedRaptorApplicationFunctionPaths.ViewConfidential);
			var dataClaimType = string.Concat(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace, "/AvailableData");
			var claimSet = new DefaultClaimSet(new[]
                                        {
                                            new Claim(claimType, null, Rights.PossessProperty),
                                            new Claim(dataClaimType, new AuthorizeMyBusinessUnit(), Rights.PossessProperty)
                                        });

			principal.AddClaimSet(claimSet);

			target.Confidential = true;
			using (CurrentAuthorization.ThreadlyUse(new PrincipalAuthorization(new FakeCurrentTeleoptiPrincipal(principal))))
			{
				Assert.AreEqual(target.Description, target.ConfidentialDescription(targetPerson));
				Assert.AreEqual(target.DisplayColor, target.ConfidentialDisplayColor(targetPerson));
			}		
		}

		[Test]
		public void ShouldHaveDeletedInDescription()
		{
			target.SetDeleted();
			target.Description.Name.Should().Contain(UserTexts.Resources.Deleted);
		}
    }
}