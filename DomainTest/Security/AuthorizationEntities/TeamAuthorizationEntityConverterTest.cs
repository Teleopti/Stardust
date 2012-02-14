using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.AuthorizationEntities
{
    /// <summary>
    /// Test cases for TeamAuthorizationEntityConverter class
    /// </summary>
    [TestFixture]
    public class TeamAuthorizationEntityConverterTest
    {
        private TeamAuthorizationEntityConverter _target;
        private Team _team;
        private ApplicationRole _applicationRole;
        
        [SetUp]
        public void Setup()
        {
            _team = TeamFactory.CreateTeam("Team1", "Site1");
            _team.Description = new Description("Team1", "T1");
            _applicationRole = ApplicationRoleFactory.CreateRole("Role", "Role");
            _target = new TeamAuthorizationEntityConverter(_team, _applicationRole);
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyConstructor1()
        {
            _target = new TeamAuthorizationEntityConverter();
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreSame(_team, _target.ContainedEntity);
            Assert.AreEqual(_target.AuthorizationKey, _team.Id.ToString());
            Assert.AreEqual(_target.AuthorizationName, _team.Description.ToString());
            Assert.AreEqual(_target.AuthorizationDescription, _team.Description.ToString() +  " Team");
            Assert.AreEqual(_target.AuthorizationValue, _applicationRole.Name);
        }
    }
}
