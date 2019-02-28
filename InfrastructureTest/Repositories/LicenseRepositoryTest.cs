using System;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    [TestFixture]
    [Category("BucketB")]
    public class LicenseRepositoryTest : RepositoryTest<ILicense>
    {
        private readonly string toVerify = Guid.NewGuid().ToString();

        #region Overrides of RepositoryTest<License>

        /// <summary>
        /// Runs every test. Implemented by repository's concrete implementation.
        /// </summary>
        protected override void ConcreteSetup()
        {
				
		}

        /// <summary>
        /// Creates an aggregate using the Bu of logged in user.
        /// Should be a "full detailed" aggregate
        /// </summary>
        /// <returns></returns>
        protected override ILicense CreateAggregateWithCorrectBusinessUnit()
        {
            License lic = new License {XmlString = toVerify};
            return lic;
        }

        /// <summary>
        /// Verifies the aggregate graph properties.
        /// </summary>
        /// <param name="loadedAggregateFromDatabase">The loaded aggregate from database.</param>
        protected override void VerifyAggregateGraphProperties(ILicense loadedAggregateFromDatabase)
        {
            Assert.IsNotNull(loadedAggregateFromDatabase);
            Assert.AreEqual(toVerify, loadedAggregateFromDatabase.XmlString);
        }

        protected override Repository<ILicense> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return LicenseRepository.DONT_USE_CTOR(currentUnitOfWork);
        }

        #endregion

        #region disallow more than one license
        [Test]
        public void VerifyMultipleLicensesNotAllowed()
        {
            ILicenseRepository licenseRepository = LicenseRepository.DONT_USE_CTOR(UnitOfWork);
            ILicense license = new License { XmlString = "<foo></foo>" };
            licenseRepository.Add(license);
            Session.Flush();

            ILicense oneLicenseTooMany = new License { XmlString = "<overflow></overflow>" };
            licenseRepository.Add(oneLicenseTooMany);
            Session.Flush();
            Assert.AreEqual(1, licenseRepository.LoadAll().Count());
        }

        [Test]
        public void VerifyMultipleLicensesNotAllowedWithAddRange()
        {
            ILicenseRepository licenseRepository = LicenseRepository.DONT_USE_CTOR(UnitOfWork);
            ILicense license = new License { XmlString = "<foo></foo>" };
            licenseRepository.Add(license);
            Session.Flush();

            ILicense oneLicenseTooMany = new License { XmlString = "<overflow></overflow>" };
            licenseRepository.AddRange(new Collection<ILicense> {oneLicenseTooMany} );
            Session.Flush();
            Assert.AreEqual(1, licenseRepository.LoadAll().Count());
        }

        [Test]
        public void ShouldCreateInstanceWithUnitOfWorkFactory()
        {
            var repository = LicenseRepository.DONT_USE_CTOR(CurrUnitOfWork);
            Assert.IsNotNull(repository);
        }
        #endregion

		[TestCase(false, ExpectedResult = 1)]
		[TestCase(true, ExpectedResult = 0)]
		public int ShouldGetActiveAgents(bool deletedBusinessUnit)
		{
			var licenseRepository = LicenseRepository.DONT_USE_CTOR(UnitOfWork);
			var person = PersonFactory.CreatePerson();
			var team = TeamFactory.CreateSimpleTeam("_");
			var site = SiteFactory.CreateSimpleSite();
			site.AddTeam(team);
			var personContract = PersonContractFactory.CreatePersonContract();
			var personPeriod = new PersonPeriod(new DateOnly(2000, 1, 1), personContract, team);
			var scenario = new Scenario { DefaultScenario = true };
			var activity = new Activity("_");
			var shiftCategory = new ShiftCategory();
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, new DateTimePeriod(2000, 1, 1, 2000, 1, 2), shiftCategory);
			if (deletedBusinessUnit)
			{
				((IDeleteTag)site.BusinessUnit).SetDeleted();
			}
			PersistAndRemoveFromUnitOfWork(site.BusinessUnit);
			PersistAndRemoveFromUnitOfWork(personContract.Contract);
			PersistAndRemoveFromUnitOfWork(personContract.ContractSchedule);
			PersistAndRemoveFromUnitOfWork(personContract.PartTimePercentage);
			person.AddPersonPeriod(personPeriod);
			PersistAndRemoveFromUnitOfWork(site);
			PersistAndRemoveFromUnitOfWork(team);
			PersistAndRemoveFromUnitOfWork(person);
			PersistAndRemoveFromUnitOfWork(shiftCategory);
			PersistAndRemoveFromUnitOfWork(scenario);
			PersistAndRemoveFromUnitOfWork(activity);
			PersistAndRemoveFromUnitOfWork(assignment);

			var agents = licenseRepository.GetActiveAgents();

			return agents.Count;
		}
	}
}
