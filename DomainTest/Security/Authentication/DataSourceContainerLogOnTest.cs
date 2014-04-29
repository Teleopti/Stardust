using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using System.Collections.Generic;

namespace Teleopti.Ccc.DomainTest.Security.Authentication
{
    [TestFixture]
    public class DataSourceContainerLogOnTest
    {
        private MockRepository mocks;
        private IRepositoryFactory repositoryFactory;
        private IDataSource dataSource;
        private DataSourceContainer target;
        private IFindApplicationUser checkLogOn;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            repositoryFactory = mocks.StrictMock<IRepositoryFactory>();
            dataSource = mocks.StrictMock<IDataSource>();
            checkLogOn = mocks.StrictMock<IFindApplicationUser>();

            target = new DataSourceContainer(dataSource, repositoryFactory, checkLogOn, AuthenticationTypeOption.Application);
        }

        [Test]
        public void VerifyBusinessUnitsProvider()
        {
            Assert.IsNotNull(target.AvailableBusinessUnitProvider);
        }

        [Test]
        public void VerifyLogOn()
        {
            var unitOfWorkFactory = mocks.StrictMock<IUnitOfWorkFactory>();
            var unitOfWork = mocks.StrictMock<IUnitOfWork>();
            var person = mocks.StrictMock<IPerson>();
	        using(mocks.Record())
            {
                Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(dataSource.Application).Return(unitOfWorkFactory);
                Expect.Call(checkLogOn.CheckLogOn(unitOfWork, "robink", "topsecret")).Return(new AuthenticationResult{Person = person});
	            Expect.Call(unitOfWork.PersistAll()).Return(new List<IRootChangeInfo>());
                Expect.Call(unitOfWork.Dispose);
            }
            using (mocks.Playback())
            {
                var authenticationResult = target.LogOn("robink","topsecret");
                Assert.AreEqual(person,target.User);
                Assert.AreEqual(person, authenticationResult.Person);
            }
        }

        [Test]
        public void ShouldLogOnDespiteExceptionOnSave()
        {
            IUnitOfWorkFactory unitOfWorkFactory = mocks.StrictMock<IUnitOfWorkFactory>();
            IUnitOfWork unitOfWork = mocks.StrictMock<IUnitOfWork>();
            IPerson person = mocks.StrictMock<IPerson>();
			using (mocks.Record())
            {
                Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(dataSource.Application).Return(unitOfWorkFactory);
                Expect.Call(checkLogOn.CheckLogOn(unitOfWork, "robink", "topsecret")).Return(new AuthenticationResult { Person = person, Successful = true});
                Expect.Call(unitOfWork.PersistAll()).Throw(new DataSourceException());
                Expect.Call(unitOfWork.Dispose);
            }
            using (mocks.Playback())
            {
                var authenticationResult = target.LogOn("robink", "topsecret");
                authenticationResult.Successful.Should().Be.True();
            }
        }

        [Test]
        public void ShouldNotLogOnWindowsUserWithoutDomain()
        {
            IUnitOfWorkFactory unitOfWorkFactory = mocks.StrictMock<IUnitOfWorkFactory>();
            IUnitOfWork unitOfWork = mocks.StrictMock<IUnitOfWork>();
            using (mocks.Record())
            {
                Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(dataSource.Application).Return(unitOfWorkFactory);
                Expect.Call(unitOfWork.Dispose);
            }
            using (mocks.Playback())
            {
                var authenticationResult = target.LogOn("robink");
                target.User.Should().Be.Null();
                authenticationResult.Successful.Should().Be.False();
            }
        }

        [Test]
        public void ShouldLogOnWindowsUserWithDomain()
        {
            IUnitOfWorkFactory unitOfWorkFactory = mocks.StrictMock<IUnitOfWorkFactory>();
            IUnitOfWork unitOfWork = mocks.StrictMock<IUnitOfWork>();
            IPerson person = mocks.StrictMock<IPerson>();
            IPersonRepository repository = mocks.StrictMock<IPersonRepository>();
            using (mocks.Record())
            {
                Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(dataSource.Application).Return(unitOfWorkFactory);
                Expect.Call(repositoryFactory.CreatePersonRepository(unitOfWork)).Return(repository);
                Expect.Call(repository.TryFindIdentityAuthenticatedPerson("toptinet", out person)).Return(true).
                    OutRef(person);
                Expect.Call(unitOfWork.Dispose);
            }
            using (mocks.Playback())
            {
                var authenticationResult = target.LogOn(@"toptinet\robink");
                target.User.Should().Not.Be.Null();
                authenticationResult.Successful.Should().Be.True();
            }
        }
    }
}