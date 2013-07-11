using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Overtime
{
    [TestFixture]
    public class OvertimePrefernecesGeneralPersonalSettingTest
    {
        
        private OvertimePreferencesGeneralPersonalSetting _target;
        private IOvertimePreferences _overtimePreferncesMock ;
        private MockRepository _mocks;
        private IList<IScheduleTag> _scheduleTags;
        private IScheduleTag _scheduleTagMock;
        private IScheduleTag _scheduleTag;
        private Guid _guid;
        private IActivity _activity;
        private IOvertimePreferences _overtimePrefernces;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _overtimePreferncesMock = _mocks.StrictMock<IOvertimePreferences>();
            _overtimePrefernces = new OvertimePreferences();
            _scheduleTagMock = _mocks.StrictMock<IScheduleTag>();
            _activity = new Activity("test");
            _target = new OvertimePreferencesGeneralPersonalSetting() ;
            _scheduleTag = new ScheduleTag();
            _guid = new Guid();
        }

        [Test]
        public void ShouldMapWithMock()
        {
            using (_mocks.Record())
            {
                Expect.Call(_overtimePreferncesMock.ScheduleTag).Return(_scheduleTagMock);
                Expect.Call(_scheduleTagMock.Id).Return(_guid);
                mapFromBoolExpectCalls();
                Expect.Call(_overtimePreferncesMock.OvertimeFrom).Return(TimeSpan.FromHours(1));
                Expect.Call(_overtimePreferncesMock.OvertimeTo).Return(TimeSpan.FromHours(2));
                Expect.Call(_overtimePreferncesMock.SkillActivity).Return(_activity);

                Expect.Call(_scheduleTagMock.Id).Return(_guid).Repeat.AtLeastOnce();
                Expect.Call(_overtimePreferncesMock.ScheduleTag).Return(_scheduleTagMock).Repeat.AtLeastOnce() ;
                Expect.Call(() => _overtimePreferncesMock.ScheduleTag = _scheduleTagMock);
                Expect.Call(_overtimePreferncesMock.SkillActivity).Return(_activity).Repeat.AtLeastOnce();
                mapToBoolExpectCalls();
            }

             using(_mocks.Playback())
             {
                 _scheduleTags = new List<IScheduleTag> { _scheduleTagMock };
                 _target.MapFrom(_overtimePreferncesMock );
                 _target.MapTo(_overtimePreferncesMock, _scheduleTags, new List<IActivity> { _activity });
             } 
        }

        [Test]
        public void ShouldMapAllTheProperties()
        {
            IOvertimePreferences overtimePrefernces = new OvertimePreferences();
           _scheduleTags = new List<IScheduleTag> { _scheduleTag };

           _overtimePrefernces.ScheduleTag = _scheduleTag;
           _overtimePrefernces.SkillActivity = _activity;
           _overtimePrefernces.DoNotBreakMaxWorkPerWeek = true;
            _overtimePrefernces.DoNotBreakNightlyRest = true;
            _overtimePrefernces.DoNotBreakWeeklyRest = false;
            _overtimePrefernces.ExtendExistingShift = true;
            _overtimePrefernces.OvertimeFrom = TimeSpan.FromHours(10);
            _overtimePrefernces.OvertimeTo = TimeSpan.FromHours(12);
            
            _target.MapFrom(_overtimePrefernces);
            _target.MapTo(overtimePrefernces, _scheduleTags, new List<IActivity>{_activity});

            Assert.AreEqual(overtimePrefernces.ScheduleTag, _scheduleTag);
            Assert.AreEqual(overtimePrefernces.SkillActivity, _activity);
            Assert.IsTrue(overtimePrefernces.DoNotBreakMaxWorkPerWeek);
            Assert.IsTrue(overtimePrefernces.DoNotBreakNightlyRest );
            Assert.IsFalse(overtimePrefernces.DoNotBreakWeeklyRest  );
            Assert.IsTrue( overtimePrefernces.ExtendExistingShift  );
            Assert.AreEqual(overtimePrefernces .OvertimeFrom , TimeSpan.FromHours(10));
            Assert.AreEqual(overtimePrefernces .OvertimeTo , TimeSpan.FromHours(12));

        }

        private void mapFromBoolExpectCalls()
        {
            Expect.Call(_overtimePreferncesMock.DoNotBreakMaxWorkPerWeek).Return(true);
            Expect.Call(_overtimePreferncesMock.DoNotBreakNightlyRest).Return(true);
            Expect.Call(_overtimePreferncesMock.DoNotBreakWeeklyRest).Return(true);
            Expect.Call(_overtimePreferncesMock.ExtendExistingShift).Return(false);
            Expect.Call(_overtimePreferncesMock.OvertimeType).Return(_guid);
        }

        private void mapToBoolExpectCalls()
        {
            Expect.Call(() => _overtimePreferncesMock.SkillActivity   = _activity);
            Expect.Call(() => _overtimePreferncesMock.ExtendExistingShift = false);
            Expect.Call(() => _overtimePreferncesMock.DoNotBreakMaxWorkPerWeek = true);
            Expect.Call(() => _overtimePreferncesMock.DoNotBreakNightlyRest  = true);
            Expect.Call(() => _overtimePreferncesMock.DoNotBreakWeeklyRest  = true);
            Expect.Call(() => _overtimePreferncesMock.OvertimeFrom = TimeSpan.FromHours(1));
            Expect.Call(() => _overtimePreferncesMock.OvertimeTo = TimeSpan.FromHours(2));
        }

    
    }
}
