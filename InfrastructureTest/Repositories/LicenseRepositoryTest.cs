using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    [TestFixture]
    [Category("LongRunning")]
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
            return new FakeRepositoryThatAllowsOldAddRange(currentUnitOfWork);
        }

        private class FakeRepositoryThatAllowsOldAddRange : LicenseRepository
        {
            public FakeRepositoryThatAllowsOldAddRange(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork) {}

            public override void AddRange(IEnumerable<ILicense> entityCollection)
            {
                entityCollection.ForEach(Add);
            }
        }

        #endregion

        #region disallow more than one license
        [Test]
        public void VerifyMultipleLicensesNotAllowed()
        {
            ILicenseRepository licenseRepository = new LicenseRepository(UnitOfWork);
            ILicense license = new License { XmlString = "<foo></foo>" };
            licenseRepository.Add(license);
            Session.Flush();

            ILicense oneLicenseTooMany = new License { XmlString = "<overflow></overflow>" };
            licenseRepository.Add(oneLicenseTooMany);
            Session.Flush();
            Assert.AreEqual(1, licenseRepository.CountAllEntities());
        }

        [Test]
        public void VerifyMultipleLicensesNotAllowedWithAddRange()
        {
            ILicenseRepository licenseRepository = new LicenseRepository(UnitOfWork);
            ILicense license = new License { XmlString = "<foo></foo>" };
            licenseRepository.Add(license);
            Session.Flush();

            ILicense oneLicenseTooMany = new License { XmlString = "<overflow></overflow>" };
            licenseRepository.AddRange(new Collection<ILicense> {oneLicenseTooMany} );
            Session.Flush();
            Assert.AreEqual(1, licenseRepository.CountAllEntities());
        }

        [Test]
        [ExpectedException(typeof(DataSourceException))]
        public void VerifyMultipleLicensesNotAllowedWithAddRange2()
        {
            ILicenseRepository licenseRepository = new LicenseRepository(UnitOfWork);
            ILicense license = new License { XmlString = "<foo></foo>" };
            ILicense oneLicenseTooMany = new License { XmlString = "<overflow></overflow>" };
            licenseRepository.AddRange(new Collection<ILicense> { license, oneLicenseTooMany });
            Session.Flush();
        }

        [Test]
        public void ShouldCreateInstanceWithUnitOfWorkFactory()
        {
            var repository = new LicenseRepository(UnitOfWorkFactory.Current);
            Assert.IsNotNull(repository);
        }
        #endregion

		[Test]
		public void ShouldLoadActiveAgents()
		{
			ILicenseRepository licenseRepository = new LicenseRepository(UnitOfWork);
			var agents = licenseRepository.GetActiveAgents();
		}
    }
}
