using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.AuthorizationSteps;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.AuthorizationSteps
{
    [TestFixture]
    public class GrantedApplicationFunctionsProviderTest
    {
        private GrantedApplicationFunctionsProvider _target;
        private IPerson _person;

        [SetUp]
        public void Setup()
        {
            _person = PersonFactory.CreatePersonWithApplicationRolesAndFunctions();
            _target = new GrantedApplicationFunctionsProvider();
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifySetInputEntityList()
        {
            IList<IAuthorizationEntity> expectedList = new List<IAuthorizationEntity>(_person.PermissionInformation.ApplicationRoleCollection.OfType<IAuthorizationEntity>());
            _target.InputEntityList = expectedList;
            IList<IAuthorizationEntity> resultList = _target.InputEntityList;
            for (int counter = 0; counter < expectedList.Count; counter++)
            {
                Assert.AreSame(expectedList[counter], resultList[counter]);
            }

        }

        [Test]
        public void VerifyResultEntityList()
        {
            IList<IApplicationFunction> expectedList = new List<IApplicationFunction>(_person.PermissionInformation.ApplicationRoleCollection[_person.PermissionInformation.ApplicationRoleCollection.Count - 1].ApplicationFunctionCollection.OfType<IApplicationFunction>());
            IList<IAuthorizationEntity> inputList = new List<IAuthorizationEntity>(_person.PermissionInformation.ApplicationRoleCollection.OfType<IAuthorizationEntity>());
            _target.InputEntityList = inputList;
            IList<IApplicationFunction> resultList = _target.ResultEntityList;
            for (int counter = 0; counter < expectedList.Count; counter++)
            {
                Assert.AreSame(expectedList[counter], resultList[counter]);
            }
        }

    }
}
