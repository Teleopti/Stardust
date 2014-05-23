using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
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
        private IList<IMultiplicatorDefinitionSet> _multiplicatorDefinitionSets;
        private IMultiplicatorDefinitionSet _definitionSetMock;
        private IMultiplicatorDefinitionSet _definitionSet;

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
            _definitionSetMock = _mocks.StrictMock<IMultiplicatorDefinitionSet>();
            _guid = new Guid();
            _multiplicatorDefinitionSets = new List<IMultiplicatorDefinitionSet>();
            _multiplicatorDefinitionSets.Add(_definitionSetMock);
            _definitionSet = new MultiplicatorDefinitionSet("test",MultiplicatorType.Overtime );
        }

        [Test]
        public void ShouldMapWithMock()
        {
            using (_mocks.Record())
            {
                Expect.Call(_overtimePreferncesMock.ScheduleTag).Return(_scheduleTagMock);
                Expect.Call(_scheduleTagMock.Id).Return(_guid);
                mapFromBoolExpectCalls();
                Expect.Call(_overtimePreferncesMock.SelectedTimePeriod).Return(new TimePeriod(TimeSpan.FromHours(1),TimeSpan.FromHours(2)));
                Expect.Call(_overtimePreferncesMock.SkillActivity).Return(_activity);
                Expect.Call(_overtimePreferncesMock.OvertimeType).Return(_multiplicatorDefinitionSets[0]).Repeat.Twice();
                Expect.Call(_multiplicatorDefinitionSets[0].Id).Return(_guid);

                Expect.Call(_scheduleTagMock.Id).Return(_guid).Repeat.AtLeastOnce();
                Expect.Call(_overtimePreferncesMock.ScheduleTag).Return(_scheduleTagMock).Repeat.AtLeastOnce() ;
                Expect.Call(() => _overtimePreferncesMock.ScheduleTag = _scheduleTagMock);
                Expect.Call(() => _overtimePreferncesMock.OvertimeType = _definitionSetMock);
                Expect.Call(_definitionSetMock.Id).Return(_guid);

                mapToBoolExpectCalls();
            }

             using(_mocks.Playback())
             {
                 _scheduleTags = new List<IScheduleTag> { _scheduleTagMock };
                 _target.MapFrom(_overtimePreferncesMock );
                 _target.MapTo(_overtimePreferncesMock, _scheduleTags, new List<IActivity> { _activity },_multiplicatorDefinitionSets);
             } 
        }

        [Test]
        public void ShouldMapAllTheProperties()
        {
            IOvertimePreferences overtimePrefernces = new OvertimePreferences();
           _scheduleTags = new List<IScheduleTag> { _scheduleTag };

           _overtimePrefernces.ScheduleTag = _scheduleTag;
           _overtimePrefernces.SkillActivity = _activity;
           _overtimePrefernces.AllowBreakMaxWorkPerWeek = true;
            _overtimePrefernces.AllowBreakNightlyRest = true;
            _overtimePrefernces.AllowBreakWeeklyRest = false;
            _overtimePrefernces.AvailableAgentsOnly = true;
            _overtimePrefernces.ExtendExistingShift = true;
            _overtimePrefernces.SelectedTimePeriod  = new TimePeriod(TimeSpan.FromHours(10),TimeSpan.FromHours(12));
            _overtimePrefernces.OvertimeType = _definitionSet;
            
            _target.MapFrom(_overtimePrefernces);
            _target.MapTo(overtimePrefernces, _scheduleTags, new List<IActivity> { _activity }, new List<IMultiplicatorDefinitionSet>{_definitionSet });

            Assert.AreEqual(overtimePrefernces.ScheduleTag, _scheduleTag);
            Assert.AreEqual(overtimePrefernces.SkillActivity, _activity);
            Assert.IsTrue(overtimePrefernces.AllowBreakMaxWorkPerWeek);
            Assert.IsTrue(overtimePrefernces.AllowBreakNightlyRest );
            Assert.IsFalse(overtimePrefernces.AllowBreakWeeklyRest  );
            Assert.IsTrue( overtimePrefernces.ExtendExistingShift  );
            Assert.IsTrue(overtimePrefernces.AvailableAgentsOnly );
            Assert.AreEqual(overtimePrefernces.SelectedTimePeriod , new TimePeriod(TimeSpan.FromHours(10), TimeSpan.FromHours(12)));
            Assert.AreEqual(overtimePrefernces.OvertimeType ,_definitionSet );

        }

		[Test]
		public void ShouldMapToDefaultIfNotInPersonalSetting()
		{
			IOvertimePreferences preferences = new OvertimePreferences();
			_scheduleTags = new List<IScheduleTag> { _scheduleTagMock };
			_target.MapTo(preferences, _scheduleTags, new List<IActivity> { _activity }, new List<IMultiplicatorDefinitionSet> { _definitionSet });
			
			Assert.AreEqual(_activity, preferences.SkillActivity );
			Assert.AreEqual(_definitionSet, preferences.OvertimeType);
			Assert.AreEqual(new TimePeriod(TimeSpan.FromHours(1), TimeSpan.FromHours(1)), preferences.SelectedTimePeriod);
		}

        private void mapFromBoolExpectCalls()
        {
            Expect.Call(_overtimePreferncesMock.AllowBreakMaxWorkPerWeek).Return(true);
            Expect.Call(_overtimePreferncesMock.AllowBreakNightlyRest).Return(true);
            Expect.Call(_overtimePreferncesMock.AllowBreakWeeklyRest).Return(true);
            Expect.Call(_overtimePreferncesMock.ExtendExistingShift).Return(false);
            Expect.Call(_overtimePreferncesMock.AvailableAgentsOnly).Return(true);
            
        }

        private void mapToBoolExpectCalls()
        {
            Expect.Call(() => _overtimePreferncesMock.SkillActivity   = _activity);
            Expect.Call(() => _overtimePreferncesMock.ExtendExistingShift = false);
            Expect.Call(() => _overtimePreferncesMock.AllowBreakMaxWorkPerWeek = true);
            Expect.Call(() => _overtimePreferncesMock.AllowBreakNightlyRest  = true);
            Expect.Call(() => _overtimePreferncesMock.AllowBreakWeeklyRest  = true);
            Expect.Call(() => _overtimePreferncesMock.AvailableAgentsOnly   = true);
            Expect.Call(() => _overtimePreferncesMock.SelectedTimePeriod = new TimePeriod(TimeSpan.FromHours(1), TimeSpan.FromHours(2)));
        }

		
    }
}
