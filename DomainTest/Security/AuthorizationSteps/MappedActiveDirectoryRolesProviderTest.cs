using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.AuthorizationSteps;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.AuthorizationSteps
{
    [TestFixture]
    public class MappedActiveDirectoryRolesProviderTest
    {
        private MappedActiveDirectoryRolesProvider _target;
        private ISystemRoleApplicationRoleMapperRepository _rep;
        private MockRepository _mocks;
        private string _systemName;
        private IList<SystemRole> _inputList;
        private IList<IApplicationRole> _expectedList;
        private IList<SystemRoleApplicationRoleMapper> _repositoryList;

        [SetUp]
        public void Setup()
        {
            _systemName = "ActiveDirectory";
            _mocks = new MockRepository();
            _rep = _mocks.StrictMock<ISystemRoleApplicationRoleMapperRepository>();
            _target = new MappedActiveDirectoryRolesProvider(_rep, _systemName);

            _inputList = SystemRoleFactory.CreateRolesForSystemRoleApplicationRoleMapperTest();
            _expectedList = ApplicationRoleFactory.CreateRolesForSystemRoleApplicationRoleMapperTest();
            _repositoryList =
                new List<SystemRoleApplicationRoleMapper>(SystemRoleApplicationRoleMapperFactory.CreateSystemRoleApplicationRoleMapperListForTest(_inputList, _expectedList));          

            _inputList = SystemRoleFactory.AddNotMappedSystemRoleToSystemRoleList(_inputList);

            _target.InputEntityList = new List<IAuthorizationEntity>(_inputList.OfType<IAuthorizationEntity>());

        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyResultEntityList()
        {

            _mocks.Record();

            Expect.Call(_rep.FindAllBySystemName(_systemName)).Return(_repositoryList).Repeat.AtLeastOnce();
            
            _mocks.ReplayAll();

            IList<IApplicationRole> resultList = _target.ResultEntityList;

            _mocks.VerifyAll();

            for (int counter = 0; counter < _expectedList.Count; counter++)
            {
                Assert.AreSame(_expectedList[counter], resultList[counter]);
            }
        }

    }
}
