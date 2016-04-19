using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.SettingsForPersonPeriodChangedEventHandlers;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.SettingsForPersonPeriodChangedEventHandlers
{
    [TestFixture]
    public class UpdateFindPersonDataHandlerTest
    {
        private MockRepository _mocks;
        private UpdateFindPersonDataHandlerHangfire _target;
        private IPersonFinderReadOnlyRepository _finderReadOnlyRep;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _finderReadOnlyRep = _mocks.StrictMock<IPersonFinderReadOnlyRepository>();
            _target = new UpdateFindPersonDataHandlerHangfire(_finderReadOnlyRep);
        }

        [Test]
        public void ShouldDenormalizeNotPersons()
        {
            var skill = SkillFactory.CreateSkill("TestID");
            var tempGuid = Guid.NewGuid();
            skill.SetId(tempGuid);

            var ids = new[] {tempGuid};
            var @event = new SettingsForPersonPeriodChangedEvent();
			@event.SetIdCollection(ids);

            using (_mocks.Record())
            {
                Expect.Call(() => _finderReadOnlyRep.UpdateFindPersonData(ids));
            }
            using (_mocks.Playback())
            {
                _target.Handle(@event);
            }
        }
    }
}