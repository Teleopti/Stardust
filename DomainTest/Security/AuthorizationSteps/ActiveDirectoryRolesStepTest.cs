using System.Collections.Generic;
using System.Runtime.InteropServices;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Security.ActiveDirectory;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.AuthorizationSteps;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest.Security.AuthorizationSteps
{
    [TestFixture]
    public class ActiveDirectoryRolesStepTest
    {
        private ActiveDirectoryRolesStep _target;
        private string _stepName;
        private IActiveDirectoryUserRepository _repository;
        private MockRepository _mocks;
        private readonly string _userName = "UserName";

        [SetUp]
        public void Setup()
        {
            _stepName = "ActiveDirectoryRolesStep";
            _mocks = new MockRepository();
            _repository = _mocks.StrictMock<IActiveDirectoryUserRepository>();
            _target = new ActiveDirectoryRolesStep(_stepName, _userName, _repository, "Active Directory Roles step description");
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target);
            Assert.AreEqual(_stepName, _target.PanelName);
        }

        [Test]
        public void VerifyRefreshList()
        {

            ActiveDirectoryUser user = ActiveDirectoryUserFactory.CreateActiveDirectoryUserWithTwoGroups();
           
            _mocks.Record();

            Expect.Call(_repository.FindUser(ActiveDirectoryUserMapper.SAMACCOUNTNAME, _userName)).Return(user).Repeat.AtLeastOnce();
            
            _mocks.ReplayAll();

            _target.RefreshList();
            IList<SystemRole> resultList = _target.ProvidedList<SystemRole>();

            _mocks.VerifyAll();
            
            Assert.AreEqual(resultList.Count, 2);
            Assert.AreEqual(resultList[0].Name, user.TokenGroups[0].CommonName);
            Assert.AreEqual(resultList[0].DescriptionText, user.TokenGroups[0].DistinguishedName);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        [Test]
        public void VerifyHandleFalseDomainUserName()
        {

            _mocks.Record();

            Expect.Call(_repository.FindUser(ActiveDirectoryUserMapper.SAMACCOUNTNAME, _userName))
                .Return(null);

            _mocks.ReplayAll();

            _target.RefreshList();

            IList<SystemRole> resultList = _target.ProvidedList<SystemRole>();

            // Expectations
            Assert.IsNotNull(resultList);
            Assert.AreEqual(0, resultList.Count);
            Assert.IsTrue(_target.Enabled);
            Assert.IsNull(_target.InnerException);
            Assert.IsNotNullOrEmpty(_target.WarningMessage);

            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyHandleComExceptionWhenFindingDomainUserName()
        {

            _mocks.Record();

            Expect.Call(_repository.FindUser(ActiveDirectoryUserMapper.SAMACCOUNTNAME, _userName)).Throw(new COMException()).Repeat.AtLeastOnce();

            _mocks.ReplayAll();

            _target.RefreshList();

            IList<SystemRole> resultList = _target.ProvidedList<SystemRole>();

            // Expectations
            Assert.IsNotNull(resultList);
            Assert.AreEqual(0, resultList.Count);
            Assert.IsTrue(_target.Enabled);
            Assert.IsNotNull(_target.InnerException);

            _mocks.VerifyAll();
        }
    }
}
