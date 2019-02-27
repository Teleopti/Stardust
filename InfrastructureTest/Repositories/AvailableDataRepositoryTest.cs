using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    /// <summary>
    /// Tests for AvailableData repository
    /// </summary>
    [TestFixture]
    [Category("BucketB")]
    public class AvailableDataRepositoryTest : RepositoryTest<IAvailableData>
    {
        private AvailableDataRepository _target;

        /// <summary>
        /// Runs every test. Implemented by repository's concrete implementation.
        /// </summary>
        protected override void ConcreteSetup()
        {
            //
        }

        /// <summary>
        /// Creates an aggregate using the Bu of logged in user.
        /// Should be a "full detailed" aggregate
        /// </summary>
        /// <returns></returns>
        protected override IAvailableData CreateAggregateWithCorrectBusinessUnit()
        {
            return CreatePersistableAvailableData();
        }

        /// <summary>
        /// Verifies the aggregate graph properties.
        /// </summary>
        /// <param name="loadedAggregateFromDatabase">The loaded aggregate from database.</param>
        protected override void VerifyAggregateGraphProperties(IAvailableData loadedAggregateFromDatabase)
        {
            Assert.IsNotNull(loadedAggregateFromDatabase);
            Assert.AreEqual(AvailableDataRangeOption.MySite, loadedAggregateFromDatabase.AvailableDataRange);
            Assert.IsNotNull(loadedAggregateFromDatabase.ApplicationRole);
        }

        protected override Repository<IAvailableData> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return AvailableDataRepository.DONT_USE_CTOR(currentUnitOfWork);
        }

        [Test]
        public void VerifyAvailableTeams()
        {

            IAvailableData availableData = CreatePersistableAvailableData();

            ISite site = SiteFactory.CreateSimpleSite("Site");

            PersistAndRemoveFromUnitOfWork(site);

            ITeam team1 = TeamFactory.CreateSimpleTeam("Team1");
            ITeam team2 = TeamFactory.CreateSimpleTeam("Team2");
            ITeam team3 = TeamFactory.CreateSimpleTeam("Team3");

            site.AddTeam(team1);
            site.AddTeam(team2);
            site.AddTeam(team3);

            PersistAndRemoveFromUnitOfWork(team1);
            PersistAndRemoveFromUnitOfWork(team2);
            PersistAndRemoveFromUnitOfWork(team3);

            availableData.AddAvailableTeam(team1);
            availableData.AddAvailableTeam(team2);
            availableData.AddAvailableTeam(team3);

            PersistAndRemoveFromUnitOfWork(availableData);

            _target = AvailableDataRepository.DONT_USE_CTOR(UnitOfWork);

            IAvailableData loadedAvailableData = _target.Load(availableData.Id.Value);
            Assert.AreEqual(availableData.AvailableTeams.Count, loadedAvailableData.AvailableTeams.Count);

        }

        [Test]
        public void VerifyAvailableSites()
        {

            IAvailableData availableData = CreatePersistableAvailableData();

            ISite site1 = SiteFactory.CreateSimpleSite("Site1");
            ISite site2 = SiteFactory.CreateSimpleSite("Site2");
            ISite site3 = SiteFactory.CreateSimpleSite("Site3");

            PersistAndRemoveFromUnitOfWork(site1);
            PersistAndRemoveFromUnitOfWork(site2);
            PersistAndRemoveFromUnitOfWork(site3);

            availableData.AddAvailableSite(site1);
            availableData.AddAvailableSite(site2);
            availableData.AddAvailableSite(site3);

            PersistAndRemoveFromUnitOfWork(availableData);
            _target = AvailableDataRepository.DONT_USE_CTOR(UnitOfWork);
            IAvailableData loadedAvailableData = _target.Load(availableData.Id.Value);
            Assert.AreEqual(availableData.AvailableSites.Count, loadedAvailableData.AvailableSites.Count);

        }

        [Test]
        public void VerifyAvailableBusinessUnits()
        {

            IAvailableData availableData = CreatePersistableAvailableData();

            IBusinessUnit unit1 = BusinessUnitFactory.CreateSimpleBusinessUnit("Unit1");
            IBusinessUnit unit2 = BusinessUnitFactory.CreateSimpleBusinessUnit("Unit2");
            IBusinessUnit unit3 = BusinessUnitFactory.CreateSimpleBusinessUnit("Unit3");

            PersistAndRemoveFromUnitOfWork(unit1);
            PersistAndRemoveFromUnitOfWork(unit2);
            PersistAndRemoveFromUnitOfWork(unit3);

            availableData.AddAvailableBusinessUnit(unit1);
            availableData.AddAvailableBusinessUnit(unit2);
            availableData.AddAvailableBusinessUnit(unit3);

            PersistAndRemoveFromUnitOfWork(availableData);

            _target = AvailableDataRepository.DONT_USE_CTOR(UnitOfWork);

            IAvailableData loadedAvailableData = _target.Load(availableData.Id.Value);

            Assert.AreEqual(availableData.AvailableBusinessUnits.Count, loadedAvailableData.AvailableBusinessUnits.Count);

        }

        [Test]
        public void VerifyDifferentTypesOfAvailableObject()
        {
            IAvailableData availableData = CreatePersistableAvailableData();

            ISite site1 = SiteFactory.CreateSimpleSite("Site");

            PersistAndRemoveFromUnitOfWork(site1);

            availableData.AddAvailableSite(site1);

            ITeam team1 = TeamFactory.CreateSimpleTeam("Team1");

            site1.AddTeam(team1);

            PersistAndRemoveFromUnitOfWork(team1);

            availableData.AddAvailableTeam(team1);

            BusinessUnit unit1 = new BusinessUnit("Unit1");

            PersistAndRemoveFromUnitOfWork(unit1);

            availableData.AddAvailableBusinessUnit(unit1);

            PersistAndRemoveFromUnitOfWork(availableData);

            _target = AvailableDataRepository.DONT_USE_CTOR(UnitOfWork);

            IAvailableData loadedAvailableData = _target.Load(availableData.Id.Value);

            Assert.AreEqual(availableData.AvailableTeams.Count, loadedAvailableData.AvailableTeams.Count);
            Assert.AreEqual(availableData.AvailableSites.Count, loadedAvailableData.AvailableSites.Count);
            Assert.AreEqual(availableData.AvailableBusinessUnits.Count, loadedAvailableData.AvailableBusinessUnits.Count);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "SCUD"), Test]
        public void VerifyMultipleSCUD()
        {
            ISite site1 = SiteFactory.CreateSimpleSite("Site1");
            ISite site2 = SiteFactory.CreateSimpleSite("Site2");

            PersistAndRemoveFromUnitOfWork(site1);
            PersistAndRemoveFromUnitOfWork(site2);

            ITeam team1 = TeamFactory.CreateSimpleTeam("Team1");
            ITeam team2 = TeamFactory.CreateSimpleTeam("Team2");
            site1.AddTeam(team1);
            site1.AddTeam(team2);

            PersistAndRemoveFromUnitOfWork(team1);
            PersistAndRemoveFromUnitOfWork(team2);

            IAvailableData availableData = CreatePersistableAvailableData();

            availableData.AddAvailableTeam(team1);
            availableData.AddAvailableSite(site1);

            PersistAndRemoveFromUnitOfWork(availableData);

            _target = AvailableDataRepository.DONT_USE_CTOR(UnitOfWork);

            IAvailableData loadedAvailableData = _target.Load(availableData.Id.Value);

            // Select
            Assert.AreEqual(1, loadedAvailableData.AvailableTeams.Count);
            Assert.AreEqual(1, loadedAvailableData.AvailableSites.Count);

            // Update
            loadedAvailableData.AddAvailableTeam(team2);
            loadedAvailableData.AddAvailableSite(site2);

            PersistAndRemoveFromUnitOfWork(loadedAvailableData);
            _target = AvailableDataRepository.DONT_USE_CTOR(UnitOfWork);
            loadedAvailableData = _target.Load(availableData.Id.Value);

            Assert.AreEqual(2, loadedAvailableData.AvailableTeams.Count);
            Assert.AreEqual(2, loadedAvailableData.AvailableSites.Count);

            loadedAvailableData.DeleteAvailableTeam(team2);
            loadedAvailableData.DeleteAvailableSite(site2);

            PersistAndRemoveFromUnitOfWork(loadedAvailableData);
            _target = AvailableDataRepository.DONT_USE_CTOR(UnitOfWork);
            loadedAvailableData = _target.Load(availableData.Id.Value);

            Assert.AreEqual(1, loadedAvailableData.AvailableTeams.Count);
            Assert.AreEqual(1, loadedAvailableData.AvailableSites.Count);

        }

        [Test]
        public void VerifyLoadAll()
        {
            IAvailableData availableData1 = CreatePersistableAvailableData();
            IAvailableData availableData2 = CreatePersistableAvailableData();

            PersistAndRemoveFromUnitOfWork(availableData1);
            PersistAndRemoveFromUnitOfWork(availableData2);

            _target = AvailableDataRepository.DONT_USE_CTOR(UnitOfWork);
            IList<IAvailableData> resultList = _target.LoadAllAvailableData();

            Assert.AreEqual(2, resultList.Count);
        }

        /// <summary>
        /// Creates a persistable available data.
        /// </summary>
        /// <returns></returns>
        private IAvailableData CreatePersistableAvailableData()
        {
            IApplicationRole applicationRole = ApplicationRoleFactory.CreateRole("Role", "Role");
            PersistAndRemoveFromUnitOfWork(applicationRole);
            IAvailableData availableData = new AvailableData();
            availableData.ApplicationRole = applicationRole;
            availableData.AvailableDataRange = AvailableDataRangeOption.MySite;
            return availableData;
        }
    }
}