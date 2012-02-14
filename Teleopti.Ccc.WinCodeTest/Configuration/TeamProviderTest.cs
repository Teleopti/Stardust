using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.WinCode.Common.Configuration;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.WinCodeTest.Configuration
{
    [TestFixture]
    public class TeamProviderTest
    {
        private MockRepository _mocks;
        private ITeamProvider _target;
        private ITeamRepository _teamRepository;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _teamRepository = _mocks.StrictMock<ITeamRepository>();
            _target = new TeamProvider(_teamRepository);
        }

        [Test]
        public void VerifyCanGetAllTeams()
        {
            var result = new List<ITeam> { _mocks.StrictMock<ITeam>() };
            using (_mocks.Record())
            {
                Expect.Call(_teamRepository.LoadAll()).Return(result);
            }
            Assert.AreEqual(result, _target.GetTeams());
        }


        [Test]
        public void VerifyUpdate()
        {
            var oldTeam = _mocks.StrictMock<ITeam>();
            var newTeam = _mocks.StrictMock<ITeam>();
            var teamList = new List<ITeam> { oldTeam };
            var teamId = Guid.NewGuid();
            using (_mocks.Record())
            {
                Expect.Call(_teamRepository.LoadAll()).Return(teamList);
                Expect.Call(_teamRepository.Get(teamId)).Return(newTeam);
                Expect.Call(oldTeam.Id).Return(teamId);
            }
            using (_mocks.Playback())
            {
                _target.HandleMessageBrokerEvent(teamId, DomainUpdateType.Update);
                Assert.AreEqual(1, _target.GetTeams().Count());
                Assert.IsTrue(_target.GetTeams().Contains(newTeam));
            }
        }

        [Test]
        public void VerifyDelete()
        {
            var oldTeam = _mocks.StrictMock<ITeam>();
            var teamList = new List<ITeam> { oldTeam };
            var teamId = Guid.NewGuid();
            using (_mocks.Record())
            {
                Expect.Call(_teamRepository.LoadAll()).Return(teamList);
                Expect.Call(oldTeam.Id).Return(teamId);
            }
            using (_mocks.Playback())
            {
                _target.HandleMessageBrokerEvent(teamId, DomainUpdateType.Delete);
                Assert.AreEqual(0, _target.GetTeams().Count());
                Assert.IsFalse(_target.GetTeams().Contains(oldTeam));
            }
        }

        [Test]
        public void VerifyInsert()
        {
            var newTeam = _mocks.StrictMock<ITeam>();
            var teamList = new List<ITeam>();
            var teamId = Guid.NewGuid();
            using (_mocks.Record())
            {
                Expect.Call(_teamRepository.LoadAll()).Return(teamList);
                Expect.Call(_teamRepository.Get(teamId)).Return(newTeam);
            }
            using (_mocks.Playback())
            {
                _target.HandleMessageBrokerEvent(teamId, DomainUpdateType.Insert);
                Assert.AreEqual(1, _target.GetTeams().Count());
                Assert.IsTrue(_target.GetTeams().Contains(newTeam));
            }
        }
    }
}
