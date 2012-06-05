using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;
using Rhino.Mocks;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class SchedulingOptionsGeneralPersonalSettingTest
    {
        private SchedulingOptionsGeneralPersonalSetting _target;
        private ISchedulingOptions _schedulingOptions;
        private MockRepository _mocks;
        private IList<IScheduleTag> _scheduleTags;
        private IList<IGroupPageLight> _groupPages;
        private IScheduleTag _scheduleTag;
        private IGroupPageLight _groupPage;
        private Guid _guid;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _schedulingOptions = _mocks.StrictMock<ISchedulingOptions>();
            _scheduleTag = _mocks.StrictMock<IScheduleTag>();
            
            _groupPage = _mocks.StrictMock<IGroupPageLight>();
            _groupPages = new List<IGroupPageLight>{_groupPage};
            _target = new SchedulingOptionsGeneralPersonalSetting();
            _guid = new Guid();
            _schedulingOptions = _mocks.StrictMock<ISchedulingOptions>();
        }

        [Test]
        public void ShouldMap()
        {
            using(_mocks.Record())
            {
                Expect.Call(_schedulingOptions.TagToUseOnScheduling).Return(_scheduleTag);
                Expect.Call(_scheduleTag.Id).Return(_guid);
                MapFromExpectations();

                Expect.Call(_scheduleTag.Id).Return(_guid);
                Expect.Call(_schedulingOptions.TagToUseOnScheduling).Return(_scheduleTag);
                Expect.Call(() => _schedulingOptions.TagToUseOnScheduling = _scheduleTag);
                MapToExpectations();
            }

            using(_mocks.Playback())
            {
                _scheduleTags = new List<IScheduleTag> { _scheduleTag };
                _target.MapFrom(_schedulingOptions);
                _target.MapTo(_schedulingOptions, _scheduleTags, _groupPages);    
            }   
        }

        [Test]
        public void ShouldSetTagToNullScheduleInstanceWhenNoTag()
        {
            using (_mocks.Record())
            {
                Expect.Call(_schedulingOptions.TagToUseOnScheduling).Return(_scheduleTag);
                Expect.Call(_scheduleTag.Id).Return(null);
                MapFromExpectations();

                Expect.Call(_schedulingOptions.TagToUseOnScheduling).Return(null);
                Expect.Call(() => _schedulingOptions.TagToUseOnScheduling = NullScheduleTag.Instance);
                MapToExpectations();
            }

            using (_mocks.Playback())
            {
                _scheduleTags = new List<IScheduleTag> ();
                _target.MapFrom(_schedulingOptions);
                _target.MapTo(_schedulingOptions, _scheduleTags, _groupPages);
            }    
        }

        private void MapFromExpectations()
        {
            Expect.Call(_schedulingOptions.UseRotations).Return(true);
            Expect.Call(_schedulingOptions.RotationDaysOnly).Return(true);
            Expect.Call(_schedulingOptions.UseAvailability).Return(true);
            Expect.Call(_schedulingOptions.AvailabilityDaysOnly).Return(true);
            Expect.Call(_schedulingOptions.UseBlockScheduling).Return(BlockFinderType.BetweenDayOff);
            Expect.Call(_schedulingOptions.UseGroupScheduling).Return(true);
            Expect.Call(_schedulingOptions.GroupOnGroupPage).Return(_groupPage);
            Expect.Call(_groupPage.Key).Return("groupPageKey");
            Expect.Call(_schedulingOptions.UseStudentAvailability).Return(true);
            Expect.Call(_schedulingOptions.UsePreferences).Return(true);
            Expect.Call(_schedulingOptions.PreferencesDaysOnly).Return(true);
            Expect.Call(_schedulingOptions.UsePreferencesMustHaveOnly).Return(true);
            Expect.Call(_schedulingOptions.UseShiftCategoryLimitations).Return(true);
            Expect.Call(_schedulingOptions.ShowTroubleshot).Return(true);
        }

       
        private void MapToExpectations()
        { 
            Expect.Call(() => _schedulingOptions.UseRotations = true);
            Expect.Call(() => _schedulingOptions.RotationDaysOnly = true);
            Expect.Call(() => _schedulingOptions.UseAvailability = true);
            Expect.Call(() => _schedulingOptions.AvailabilityDaysOnly = true);
            Expect.Call(() => _schedulingOptions.UseBlockScheduling = BlockFinderType.BetweenDayOff);
            Expect.Call(() => _schedulingOptions.UseGroupScheduling = true);
            Expect.Call(_groupPage.Key).Return("groupPageKey");
            Expect.Call(() => _schedulingOptions.GroupOnGroupPage = _groupPage);
            Expect.Call(() => _schedulingOptions.UseStudentAvailability = true);
            Expect.Call(() => _schedulingOptions.UsePreferences = true);
            Expect.Call(() => _schedulingOptions.PreferencesDaysOnly = true);
            Expect.Call(() => _schedulingOptions.UsePreferencesMustHaveOnly = true);
            Expect.Call(() => _schedulingOptions.UseShiftCategoryLimitations = true);
            Expect.Call(() => _schedulingOptions.ShowTroubleshot = true);        
        }
    }
}
