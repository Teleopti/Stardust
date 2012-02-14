using System.Collections.Generic;
using System.Collections.Specialized;
using System.DirectoryServices;
using System.Reflection;
using System.Runtime.InteropServices;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Security.ActiveDirectory;
using Teleopti.Ccc.Infrastructure.ActiveDirectory;


namespace Teleopti.Ccc.InfrastructureTest.ActiveDirectory
{

    [TestFixture]
    [Category("LongRunning")]
    public class ActiveDirectoryUserRepositoryTest
    {

        ActiveDirectoryUserRepositoryTestClass _target;

        [SetUp]
        public void Setup()
        {
            _target = new ActiveDirectoryUserRepositoryTestClass();
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyFindUsersListOverload1()
        {
            ActiveDirectoryUserRepositoryTestClass userRepository = new ActiveDirectoryUserRepositoryTestClass();
            IList<ActiveDirectoryUser> allUsers = userRepository.FindUsers(null, null, 0, null);

            Assert.IsNotNull(allUsers);
            Assert.Greater(allUsers.Count, 0);
           
        }

        [Test]
        public void VerifyFindUsersListOverload2()
        {
            IList<ActiveDirectoryUser> allUsers = _target.FindUsers(ActiveDirectoryUserMapper.SAMACCOUNTNAME, "*");

            Assert.IsNotNull(allUsers);
            Assert.Greater(allUsers.Count, 0);

        }

        [Test]
        public void VerifyFindUser()
        {
            IList<ActiveDirectoryUser> allUsers = _target.FindUsers(null, null, 10, null);

            string inListUserName = allUsers[0].SAMAccountName;

            ActiveDirectoryUser foundUser = _target.FindUser(ActiveDirectoryUserMapper.SAMACCOUNTNAME, inListUserName);

            Assert.IsNotNull(foundUser);
            Assert.AreEqual(foundUser.SAMAccountName, inListUserName);

        }

        [Test]
        public void VerifyFindUserWithNonexistentUser()
        {
            string inListUserName = "nonExistenceUser";

            ActiveDirectoryUser foundUser = _target.FindUser(ActiveDirectoryUserMapper.SAMACCOUNTNAME, inListUserName);

            Assert.IsNull(foundUser);

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes"), Test]
        [ExpectedException(typeof(COMException))]
        public void VerifyFindUserWithNoDomainPresent()
        {
            string inListUserName = "nonExistenceUser";

            using(var searcher = new DirectorySearcherChannelStub())
            {
                Expect.Call(searcher.FindOne()).IgnoreArguments().Throw(new COMException("The specified domain either does not exist or could not to be contacted.")).Repeat.Once();

                _target.SetDirectorySearcher(searcher);                
            }


            ActiveDirectoryUser foundUser = _target.FindUser(ActiveDirectoryUserMapper.SAMACCOUNTNAME, inListUserName);

            Assert.IsNull(foundUser);

        }
    }
}
