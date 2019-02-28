using System;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    [TestFixture]
    [Category("BucketB")]
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
        }

        [Test]
        public void VerifyMultipleLicensesNotAllowed()
        {
            ILicenseStatusRepository licenseRepository = new LicenseStatusRepository(CurrUnitOfWork);
            ILicenseStatus license = new LicenseStatus { XmlString = "<foo></foo>" };
            licenseRepository.Add(license);
            Session.Flush();

            ILicenseStatus oneLicenseTooMany = new LicenseStatus { XmlString = "<overflow></overflow>" };
            licenseRepository.Add(oneLicenseTooMany);
            Session.Flush();
            Assert.AreEqual(1, licenseRepository.LoadAll().Count());
        }

        [Test]
        public void VerifyMultipleLicensesNotAllowedWithAddRange()
        {
            ILicenseStatusRepository licenseRepository = new LicenseStatusRepository(CurrUnitOfWork);
            var license = new LicenseStatus { XmlString = "<foo></foo>" };
            licenseRepository.Add(license);
            Session.Flush();

            var oneLicenseTooMany = new LicenseStatus { XmlString = "<overflow></overflow>" };
            licenseRepository.AddRange(new Collection<ILicenseStatus> {oneLicenseTooMany} );
            Session.Flush();
            Assert.AreEqual(1, licenseRepository.LoadAll().Count());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void ShouldCreateInstanceWithUnitOfWorkFactory()
        {
            var repository = LicenseRepository.DONT_USE_CTOR(CurrUnitOfWork);
            Assert.IsNotNull(repository);
        }

    }
}
