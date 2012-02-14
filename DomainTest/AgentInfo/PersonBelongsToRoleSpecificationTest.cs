using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Rhino.Mocks;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.AgentInfo
{
    /// <summary>
    /// Tests for PersonBelongsToTeamSpecification
    /// </summary>
    [TestFixture]
    public class PersonBelongsToRoleSpecificationTest
    {
        private IApplicationRole _role;
        private PersonBelongsToRoleSpecification _target;

        [Test]
        public void VerifyPersonBelongsToRoleSpecificationHandleOkPerson()
        {
            IPerson okPerson = PersonFactory.CreatePersonWithApplicationRolesAndFunctions();
            _role = okPerson.PermissionInformation.ApplicationRoleCollection[0];

            _target = new PersonBelongsToRoleSpecification(_role);

            Assert.IsTrue(_target.IsSatisfiedBy(okPerson));
        }

        [Test]
        public void VerifyPersonBelongsToRoleSpecificationHandleNotOkPerson()
        {
            IPerson notOkPerson = PersonFactory.CreatePersonWithApplicationRolesAndFunctions();
            _role = new ApplicationRole();
            _role.DescriptionText = "NOTHAVETHISROLE";

            _target = new PersonBelongsToRoleSpecification(_role);

            Assert.IsFalse(_target.IsSatisfiedBy(notOkPerson));
        }


        [Test]
        public void VerifyPersonWithNoApplicationRole()
        {
            IPerson notOkPerson = PersonFactory.CreatePersonWithBasicPermissionInfo("LOG", "PASSWORD");
            _role = new ApplicationRole();
            _role.DescriptionText = "NOTHAVETHISROLE";

            _target = new PersonBelongsToRoleSpecification(_role);

            Assert.IsFalse(_target.IsSatisfiedBy(notOkPerson));
        }

        [Test]
        public void VerifyPersonWithNoApplicationRolesAtAll()
        {
            MockRepository mocks = new MockRepository();
            IPerson notOkPerson = mocks.StrictMock<IPerson>();

            mocks.Record();
            Expect.On(notOkPerson).Call(notOkPerson.PermissionInformation).Return(null).Repeat.Once();
            mocks.ReplayAll();

            _role = new ApplicationRole();
            _role.DescriptionText = "NOTHAVETHISROLE";

            _target = new PersonBelongsToRoleSpecification(_role);

            Assert.IsFalse(_target.IsSatisfiedBy(notOkPerson));

            mocks.VerifyAll();
        }
    }
}