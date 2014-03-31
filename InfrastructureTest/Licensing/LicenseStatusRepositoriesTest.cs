using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Secrets.Licensing;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Licensing
{
    [TestFixture]
    public class LicenseStatusRepositoriesTest
    {
        private MockRepository _mocks;
        private LicenseStatusRepositories _target;
        private IRepositoryFactory _repositoryFactory;
        private IUnitOfWorkFactory _unitOfWorkFactory;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
            _repositoryFactory = _mocks.StrictMock<IRepositoryFactory>();
            _target = new LicenseStatusRepositories(_unitOfWorkFactory, _repositoryFactory);
        }

        [Test]
        public void ShouldFetchNumberOfAgentsFromPersonRep()
        {
            var uow = _mocks.StrictMock<IUnitOfWork>();
            var personRep = _mocks.StrictMock<IPersonRepository>();
            Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
            Expect.Call(_repositoryFactory.CreatePersonRepository(uow)).Return(personRep);
            Expect.Call(personRep.NumberOfActiveAgents()).Return(500);
            Expect.Call(uow.Dispose);
            _mocks.ReplayAll();
            var agents = _target.NumberOfActiveAgents();
            Assert.That(agents,Is.EqualTo(500));
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldFetchStatusFromRepository()
        {
            var statusXml = new LicenseStatusXml();
            var status = new LicenseStatus();
            status.SetId(Guid.NewGuid());
            status.XmlString = statusXml.GetNewStatusDocument().OuterXml;

            var uow = _mocks.StrictMock<IUnitOfWork>();
            var licenseStatusRep = _mocks.StrictMock<ILicenseStatusRepository>();
            Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
            Expect.Call(_repositoryFactory.CreateLicenseStatusRepository(uow)).Return(licenseStatusRep);
            Expect.Call(licenseStatusRep.LoadAll()).Return(new List<ILicenseStatus>{status});
            Expect.Call(uow.Dispose);
            _mocks.ReplayAll();
            var licenseStatus = _target.LicenseStatus;
            Assert.That(licenseStatus, Is.Not.Null);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldAddStatusToRepository()
        {
            var uow = _mocks.StrictMock<IUnitOfWork>();
            var licenseStatusRep = _mocks.StrictMock<ILicenseStatusRepository>();
            Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
            Expect.Call(_repositoryFactory.CreateLicenseStatusRepository(uow)).Return(licenseStatusRep);
            Expect.Call(() => licenseStatusRep.Add(null)).IgnoreArguments();
            Expect.Call(uow.PersistAll());
            Expect.Call(uow.Dispose);
            _mocks.ReplayAll();
            _target.SaveLicenseStatus("");
            
            _mocks.VerifyAll();
        }

        [Test, ExpectedException(typeof(LicenseMissingException))]
        public void ShouldThrowIfNoLicense()
        {
            var uow = _mocks.StrictMock<IUnitOfWork>();
            var licenseRep = _mocks.StrictMock<ILicenseRepository>();
            Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
            Expect.Call(_repositoryFactory.CreateLicenseRepository(uow)).Return(licenseRep);
            Expect.Call(licenseRep.LoadAll()).Return(new List<ILicense>());
			Expect.Call(uow.Dispose);
            _mocks.ReplayAll();
            var serv = _target.XmlLicenseService(0);
            Assert.That(serv, Is.Not.Null);
            _mocks.VerifyAll();
        }
    }
    
}