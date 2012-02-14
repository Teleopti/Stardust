using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.Security.Authentication
{
    [TestFixture]
    public class FindUserDetailTest
    {
        private MockRepository mocks;
        private ICheckUserDetail checkUserDetail;
        private IFindUserDetail target;
        private IUnitOfWork unitOfWork;
        private IRepositoryFactory repositoryFactory;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            checkUserDetail = mocks.StrictMock<ICheckUserDetail>();
            unitOfWork = mocks.StrictMock<IUnitOfWork>();
            repositoryFactory = mocks.StrictMock<IRepositoryFactory>();
            target = new FindUserDetail(checkUserDetail, repositoryFactory);
        }

        [Test]
        public void VerifyCanCheckLogOn()
        {
            const string password = "pass";
            IPerson person = mocks.StrictMock<IPerson>();
            IUserDetail userDetail = mocks.StrictMock<IUserDetail>();
            IUserDetailRepository userDetailRepository = mocks.StrictMock<IUserDetailRepository>();
            var authenticationResult = new AuthenticationResult();
            using (mocks.Record())
            {
                Expect.Call(checkUserDetail.CheckLogOn(userDetail, password)).Return(authenticationResult);
                Expect.Call(repositoryFactory.CreateUserDetailRepository(unitOfWork)).Return(userDetailRepository);
                Expect.Call(userDetailRepository.FindByUser(person)).Return(userDetail);
            }
            using(mocks.Playback())
            {
                AuthenticationResult result = target.CheckLogOn(unitOfWork, person, password);
                Assert.AreEqual(authenticationResult, result);
            }
        }

    }
}
