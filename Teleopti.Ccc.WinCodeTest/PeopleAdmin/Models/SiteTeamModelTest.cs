using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin.Models
{
    [TestFixture]
    public class SiteTeamModelTest
    {
        private ITeam _team;
        private SiteTeamModel _target;
        private string _teamName = "Team Rotation";
        private string _siteName = "London";
        private string _seperator = "/";

        [SetUp]
        public void Setup()
        {

            _team = TeamFactory.CreateTeam(_teamName, _siteName);
            _target = EntityConverter.ConvertToOther<ITeam, SiteTeamModel>(_team);
        }

        [Test]
        public void VerifyPropertiesNotNull()
        {
            Assert.IsNotNull(_target.Team);
            Assert.AreEqual(_team, _target.Team);
            Assert.IsNotNull(_target.Site);
        }

        [Test]
        public void VerifyDescription()
        {
            Assert.IsNotEmpty(_target.Description);
            Assert.AreEqual(_siteName + _seperator + _teamName, _target.Description);
        }

        [Test]
        public void ShouldHandleTeamChange()
        {
            _target.Team = TeamFactory.CreateTeam("new name", _siteName);
            Assert.AreEqual(_siteName + _seperator + "new name", _target.Description);
        }
    }
}
