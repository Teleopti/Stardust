using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.Security;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.AuthorizationSteps
{
    [TestFixture]
    public class RaptorApplicationRolesProviderTest
    {
        private RaptorApplicationRolesProviderTestClass _target;
        private IPerson _person;
        private MockRepository _mocks;
        private IPersonRepository _repository;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _repository = _mocks.StrictMock<IPersonRepository>();
            _person = PersonFactory.CreatePersonWithApplicationRolesAndFunctions();
            _target = new RaptorApplicationRolesProviderTestClass(_person, _repository);

            _mocks.Record();

            Expect.Call(_repository.LoadPermissionData(_person)).Return(_person).Repeat.AtLeastOnce();

            _mocks.ReplayAll();

        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifySetInputEntityList()
        { 
            _target.InputEntityList = AuthorizationObjectFactory.CreateAuthorizationEntityList();
            IList<IAuthorizationEntity> expectedList = AuthorizationObjectFactory.CreateAuthorizationEntityList();
            _target.InputEntityList = expectedList;
            Assert.AreEqual(_target.GetInputEntityList().Count, expectedList.Count);
        }

        [Test]
        public void VerifyResultEntityList()
        {
            IList<IApplicationRole> resultList = _target.ResultEntityList;
            IList<IApplicationRole> expectedList = new List<IApplicationRole>(_person.PermissionInformation.ApplicationRoleCollection);
            for (int counter = 0; counter < expectedList.Count; counter++)
            {
                Assert.AreSame(expectedList[counter], resultList[counter]);
            }
        }

    }
}
