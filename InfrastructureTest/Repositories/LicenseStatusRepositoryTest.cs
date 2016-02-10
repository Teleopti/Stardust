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
    public class LicenseStatusRepositoryTest : RepositoryTest<ILicenseStatus>
    {
        private readonly string _toVerify = Guid.NewGuid().ToString();

        protected override void ConcreteSetup()
        {
        }

        protected override ILicenseStatus CreateAggregateWithCorrectBusinessUnit()
        {
            var lic = new LicenseStatus {XmlString = _toVerify};
            return lic;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected override void VerifyAggregateGraphProperties(ILicenseStatus loadedAggregateFromDatabase)
        {
            Assert.IsNotNull(loadedAggregateFromDatabase);
            Assert.AreEqual(_toVerify, loadedAggregateFromDatabase.XmlString);
        }

        protected override Repository<ILicenseStatus> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return new FakeRepositoryThatAllowsOldAddRange(currentUnitOfWork);
        }

        private class FakeRepositoryThatAllowsOldAddRange : LicenseStatusRepository
        {
            public FakeRepositoryThatAllowsOldAddRange(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork) { }

            public override void AddRange(IEnumerable<ILicenseStatus> entityCollection)
            {
                entityCollection.ForEach(Add);
            }
        }

        [Test]
        public void VerifyMultipleLicensesNotAllowed()
        {
            ILicenseStatusRepository licenseRepository = new LicenseStatusRepository(UnitOfWork);
            ILicenseStatus license = new LicenseStatus { XmlString = "<foo></foo>" };
            licenseRepository.Add(license);
            Session.Flush();

            ILicenseStatus oneLicenseTooMany = new LicenseStatus { XmlString = "<overflow></overflow>" };
            licenseRepository.Add(oneLicenseTooMany);
            Session.Flush();
            Assert.AreEqual(1, licenseRepository.LoadAll().Count);
        }

        [Test]
        public void VerifyMultipleLicensesNotAllowedWithAddRange()
        {
            ILicenseStatusRepository licenseRepository = new LicenseStatusRepository(UnitOfWork);
            var license = new LicenseStatus { XmlString = "<foo></foo>" };
            licenseRepository.Add(license);
            Session.Flush();

            var oneLicenseTooMany = new LicenseStatus { XmlString = "<overflow></overflow>" };
            licenseRepository.AddRange(new Collection<ILicenseStatus> {oneLicenseTooMany} );
            Session.Flush();
            Assert.AreEqual(1, licenseRepository.LoadAll().Count);
        }

        [Test]
        [ExpectedException(typeof(DataSourceException))]
        public void VerifyMultipleLicensesNotAllowedWithAddRange2()
        {
            var licenseRepository = new LicenseStatusRepository(UnitOfWork);
            var license = new LicenseStatus { XmlString = "<foo></foo>" };
            var oneLicenseTooMany = new LicenseStatus { XmlString = "<overflow></overflow>" };
            licenseRepository.AddRange(new Collection<ILicenseStatus> { license, oneLicenseTooMany });
            Session.Flush();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void ShouldCreateInstanceWithUnitOfWorkFactory()
        {
            var repository = new LicenseRepository(CurrUnitOfWork);
            Assert.IsNotNull(repository);
        }

    }
}
