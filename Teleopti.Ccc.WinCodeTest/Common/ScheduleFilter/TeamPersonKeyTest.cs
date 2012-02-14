using NUnit.Framework;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Common.ScheduleFilter;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Common.ScheduleFilter
{
    [TestFixture]
    public class TeamPersonKeyTest
    {
        private TeamPersonKey _target;
        private ITeam _team;
        private IPerson _person;

        [SetUp]
        public void Setup()
        {
            _team = TeamFactory.CreateSimpleTeam();
            _person = PersonFactory.CreatePerson();
            _target = new TeamPersonKey(_team, _person);

        }

        [Test]
        public void Verify()
        {
            Assert.AreSame(_team, _target.Team);
            Assert.AreSame(_person, _target.Person);
            Assert.IsNotNull(_target.GetHashCode());
        }
    }
}