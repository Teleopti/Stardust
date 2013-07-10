using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Overtime
{
    [TestFixture]
    public class OvertimePrefernecesGeneralPersonalSettingTest
    {
        
        private OvertimePreferencesGeneralPersonalSetting _target;
        private IOvertimePreferences _overtimePrefernces ;
        private MockRepository _mocks;
        private IList<IScheduleTag> _scheduleTags;
        private IScheduleTag _scheduleTag;
        private Guid _guid;
        private IActivity _activity;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _overtimePrefernces = _mocks.StrictMock<IOvertimePreferences>();
            //_overtimePrefernces = new OvertimePreferences();
            _scheduleTag = _mocks.StrictMock<IScheduleTag>();
            _activity = new Activity("test");
            _target = new OvertimePreferencesGeneralPersonalSetting() ;
            _guid = new Guid();
            //_schedulingOptions = _mocks.StrictMock<ISchedulingOptions>();
        }

        [Test]
        public void ShouldMap()
        {
            using (_mocks.Record())
            {
                Expect.Call(_overtimePrefernces.ScheduleTag).Return(_scheduleTag);
                Expect.Call(_scheduleTag.Id).Return(_guid);
                mapFromBoolExpectCalls();
                Expect.Call(_overtimePrefernces.OvertimeFrom).Return(TimeSpan.FromHours(1));
                Expect.Call(_overtimePrefernces.OvertimeTo).Return(TimeSpan.FromHours(2));
                Expect.Call(_overtimePrefernces.SkillActivity).Return(_activity);

                Expect.Call(_scheduleTag.Id).Return(_guid);
                Expect.Call(_overtimePrefernces.ScheduleTag).Return(_scheduleTag);
                Expect.Call(() => _overtimePrefernces.ScheduleTag = _scheduleTag);
                Expect.Call(_overtimePrefernces.SkillActivity).Return(_activity);
                mapToBoolExpectCalls();
            }

             using(_mocks.Playback())
             {
                 _scheduleTags = new List<IScheduleTag> { _scheduleTag };
                 _target.MapFrom(_overtimePrefernces );
                 _target.MapTo(_overtimePrefernces, _scheduleTags,new List<IActivity>());
             } 
        }

        [Test]
        public void ShouldMapWithScheduleTag()
        {
            _overtimePrefernces.ScheduleTag = _scheduleTag;

            using (_mocks.Playback())
            {
                _scheduleTags = new List<IScheduleTag> { _scheduleTag };
                _target.MapFrom(_overtimePrefernces);
                _target.MapTo(_overtimePrefernces, _scheduleTags, new List<IActivity>());
            }
        }

        private void mapFromBoolExpectCalls()
        {
            Expect.Call(_overtimePrefernces.DoNotBreakMaxWorkPerWeek).Return(true);
            Expect.Call(_overtimePrefernces.DoNotBreakNightlyRest).Return(true);
            Expect.Call(_overtimePrefernces.DoNotBreakWeeklyRest).Return(true);
            Expect.Call(_overtimePrefernces.ExtendExistingShift).Return(false);
            Expect.Call(_overtimePrefernces.OvertimeType).Return(_guid);
        }

        private void mapToBoolExpectCalls()
        {
            Expect.Call(_overtimePrefernces.DoNotBreakMaxWorkPerWeek).Return(true);
            Expect.Call(_overtimePrefernces.DoNotBreakNightlyRest).Return(true);
            Expect.Call(_overtimePrefernces.DoNotBreakWeeklyRest).Return(true);
            Expect.Call(_overtimePrefernces.ExtendExistingShift).Return(false);
            Expect.Call(_overtimePrefernces.OvertimeType).Return(_guid);
        }

        //    using(_mocks.Playback())
        //    {
        //        _scheduleTags = new List<IScheduleTag> { _scheduleTag };
        //        _target.MapFrom(_schedulingOptions);
        //        _target.MapTo(_schedulingOptions, _scheduleTags);    
        //    }   
        //}

        //[Test]
        //public void ShouldSetTagToNullScheduleInstanceWhenNoTag()
        //{
        //    using (_mocks.Record())
        //    {
        //        Expect.Call(_schedulingOptions.TagToUseOnScheduling).Return(_scheduleTag);
        //        Expect.Call(_scheduleTag.Id).Return(null);
        //        MapFromExpectations();

        //        Expect.Call(_schedulingOptions.TagToUseOnScheduling).Return(null);
        //        Expect.Call(() => _schedulingOptions.TagToUseOnScheduling = NullScheduleTag.Instance);
        //        MapToExpectations();
        //    }

        //    using (_mocks.Playback())
        //    {
        //        _scheduleTags = new List<IScheduleTag> ();
        //        _target.MapFrom(_schedulingOptions);
        //        _target.MapTo(_schedulingOptions, _scheduleTags);
        //    }    
        //}

        //private void MapFromExpectations()
        //{
        //    Expect.Call(_schedulingOptions.UseRotations).Return(true);
        //    Expect.Call(_schedulingOptions.RotationDaysOnly).Return(true);
        //    Expect.Call(_schedulingOptions.UseAvailability).Return(true);
        //    Expect.Call(_schedulingOptions.AvailabilityDaysOnly).Return(true);
        //    Expect.Call(_schedulingOptions.UseStudentAvailability).Return(true);
        //    Expect.Call(_schedulingOptions.UsePreferences).Return(true);
        //    Expect.Call(_schedulingOptions.PreferencesDaysOnly).Return(true);
        //    Expect.Call(_schedulingOptions.UsePreferencesMustHaveOnly).Return(true);
        //    Expect.Call(_schedulingOptions.UseShiftCategoryLimitations).Return(true);
        //    Expect.Call(_schedulingOptions.ShowTroubleshot).Return(true);
        //}

       
        //private void MapToExpectations()
        //{
        //    Expect.Call(() => _schedulingOptions.UseRotations = true);
        //    Expect.Call(() => _schedulingOptions.RotationDaysOnly = true);
        //    Expect.Call(() => _schedulingOptions.UseAvailability = true);
        //    Expect.Call(() => _schedulingOptions.AvailabilityDaysOnly = true);
        //    Expect.Call(() => _schedulingOptions.UseStudentAvailability = true);
        //    Expect.Call(() => _schedulingOptions.UsePreferences = true);
        //    Expect.Call(() => _schedulingOptions.PreferencesDaysOnly = true);
        //    Expect.Call(() => _schedulingOptions.UsePreferencesMustHaveOnly = true);
        //    Expect.Call(() => _schedulingOptions.UseShiftCategoryLimitations = true);
        //    Expect.Call(() => _schedulingOptions.ShowTroubleshot = true);        
                    
        //}
    
    }
}
