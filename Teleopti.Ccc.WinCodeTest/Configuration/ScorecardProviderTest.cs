using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration;

namespace Teleopti.Ccc.WinCodeTest.Configuration
{
    [TestFixture]
    public class ScorecardProviderTest
    {
        private MockRepository _mocks;
        private IScorecardProvider _target;
        private IRepository<IScorecard> _scorecardRepository;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _scorecardRepository = _mocks.StrictMock<IRepository<IScorecard>>();
            _target = new ScorecardProvider(_scorecardRepository, false);
        }

        [Test]
        public void VerifyCanGetAllScorecards()
        {
            var result = new List<IScorecard> { _mocks.StrictMock<IScorecard>() };
            using (_mocks.Record())
            {
                Expect.Call(_scorecardRepository.LoadAll()).Return(result);
            }
            Assert.AreEqual(result, _target.GetScorecards());
        }

        [Test]
        public void VerifyCanGetAllScorecardsWithNullScorecardIncluded()
        {
            _target = new ScorecardProvider(_scorecardRepository, true);
            var result = new List<IScorecard> { _mocks.StrictMock<IScorecard>() };
            using (_mocks.Record())
            {
                Expect.Call(_scorecardRepository.LoadAll()).Return(result);
            }
            using (_mocks.Playback())
            {
                Assert.IsTrue(_target.GetScorecards().Contains(ScorecardProvider.NullScorecard));
            }
        }

        [Test]
        public void VerifyUpdate()
        {
            var oldScorecard = _mocks.StrictMock<IScorecard>();
            var newScorecard = _mocks.StrictMock<IScorecard>();
            var scorecardList = new List<IScorecard> { oldScorecard };
            var scorecardId = Guid.NewGuid();
            using (_mocks.Record())
            {
                Expect.Call(_scorecardRepository.LoadAll()).Return(scorecardList);
                Expect.Call(_scorecardRepository.Get(scorecardId)).Return(newScorecard);
                Expect.Call(oldScorecard.Id).Return(scorecardId);
            }
            using (_mocks.Playback())
            {
                _target.HandleMessageBrokerEvent(scorecardId, DomainUpdateType.Update);
                Assert.AreEqual(1, _target.GetScorecards().Count());
                Assert.IsTrue(_target.GetScorecards().Contains(newScorecard));
            }
        }

        [Test]
        public void VerifyDelete()
        {
            var oldScorecard = _mocks.StrictMock<IScorecard>();
            var scorecardList = new List<IScorecard> { oldScorecard };
            var scorecardId = Guid.NewGuid();
            using (_mocks.Record())
            {
                Expect.Call(_scorecardRepository.LoadAll()).Return(scorecardList);
                Expect.Call(oldScorecard.Id).Return(scorecardId);
            }
            using (_mocks.Playback())
            {
                _target.HandleMessageBrokerEvent(scorecardId, DomainUpdateType.Delete);
                Assert.AreEqual(0, _target.GetScorecards().Count());
                Assert.IsFalse(_target.GetScorecards().Contains(oldScorecard));
            }
        }

        [Test]
        public void VerifyInsert()
        {
            var newScorecard = _mocks.StrictMock<IScorecard>();
            var scorecardList = new List<IScorecard>();
            var scorecardId = Guid.NewGuid();
            using (_mocks.Record())
            {
                Expect.Call(_scorecardRepository.LoadAll()).Return(scorecardList);
                Expect.Call(_scorecardRepository.Get(scorecardId)).Return(newScorecard);
            }
            using (_mocks.Playback())
            {
                _target.HandleMessageBrokerEvent(scorecardId, DomainUpdateType.Insert);
                Assert.AreEqual(1, _target.GetScorecards().Count());
                Assert.IsTrue(_target.GetScorecards().Contains(newScorecard));
            }
        }
    }
}
