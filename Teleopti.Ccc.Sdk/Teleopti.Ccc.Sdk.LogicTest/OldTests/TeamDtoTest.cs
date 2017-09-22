using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;


namespace Teleopti.Ccc.Sdk.LogicTest.OldTests
{
    [TestFixture]
    public class TeamDtoTest
    {
        private TeamDto         _target;
        private Description     _description;
        private Guid teamId = Guid.NewGuid();

        [SetUp]
        public void Setup()
        {
            MockRepository mocks = new MockRepository();
            ITeam team = mocks.StrictMock<ITeam>();
            _description = new Description("a", "b");

            using (mocks.Record())
            {
                Expect.On(team)
                    .Call(team.Description)
                    .Return(_description);

                Expect.On(team)
                    .Call(team.Id)
                    .Return(teamId);

                Expect.On(team)
                    .Call(team.SiteAndTeam)
                    .Return("Site/Team");
            }

			_target = new TeamDto { Description = team.Description.Name, Id = team .Id};
        }
       
        [Test]
        public void VerifyConstructor()
        {
            Assert.AreEqual(_description.Name , _target.Description);
            Assert.AreEqual(teamId, _target.Id);
        }

        [Test]
        public void VerifyCanSetProperties()
        {
            var id = teamId;
            _target.Description = "test";
            Assert.AreEqual("test", _target.Description);
            _target.Id = id;
	        _target.SiteAndTeam = "ngt";
            Assert.AreEqual(id, _target.Id);
            Assert.IsFalse(string.IsNullOrEmpty(_target.SiteAndTeam));
        }
    }
}