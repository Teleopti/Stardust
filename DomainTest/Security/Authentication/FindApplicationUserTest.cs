using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.Security.Authentication
{
    [TestFixture]
    public class FindApplicationUserTest
    {
        private MockRepository mocks;
        private ICheckNullUser checkNullUser;
        private IFindApplicationUser target;
        private IUnitOfWork unitOfWork;
        private IRepositoryFactory repositoryFactory;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            checkNullUser = mocks.StrictMock<ICheckNullUser>();
            unitOfWork = mocks.StrictMock<IUnitOfWork>();
            repositoryFactory = mocks.StrictMock<IRepositoryFactory>();
            target = new FindApplicationUser(checkNullUser,repositoryFactory);
        }

        [Test]
        public void VerifyCanCheckLogOn()
        {
            const string password = "pass";
            const string user = "UserName";
            IPerson person = mocks.StrictMock<IPerson>();
            IPersonRepository personRepository = mocks.StrictMock<IPersonRepository>();
            var authenticationResult = new AuthenticationResult();
            using (mocks.Record())
            {
                Expect.Call(checkNullUser.CheckLogOn(unitOfWork, person, password)).Return(authenticationResult);
                Expect.Call(repositoryFactory.CreatePersonRepository(unitOfWork)).Return(personRepository);
                Expect.Call(personRepository.TryFindBasicAuthenticatedPerson(user)).Return(person);
            }
            using(mocks.Playback())
            {
                AuthenticationResult result = target.CheckLogOn(unitOfWork, user, password);
                Assert.AreEqual(authenticationResult, result);
            }
        }
    }
}
