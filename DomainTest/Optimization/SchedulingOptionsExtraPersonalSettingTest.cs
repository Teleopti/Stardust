﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;
using Rhino.Mocks;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class SchedulingOptionsExtraPersonalSettingTest
    {
        private SchedulingOptionsExtraPersonalSetting _target;
        private ISchedulingOptions _schedulingOptions;
        private MockRepository _mocks;
        private IList<IScheduleTag> _scheduleTags;
        private IList<IGroupPageLight> _groupPages;
        private IScheduleTag _scheduleTag;
        private IGroupPageLight _groupPageLight;
        private Percent _fairnessValue;
        private Guid _guid;
        private const int _resourceCalculateFrequency = 1;


        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _schedulingOptions = _mocks.StrictMock<ISchedulingOptions>();
            _scheduleTag = _mocks.StrictMock<IScheduleTag>();

            _groupPageLight = _mocks.StrictMock<IGroupPageLight>();
            _groupPages = new List<IGroupPageLight> { _groupPageLight };
            _target = new SchedulingOptionsExtraPersonalSetting();
            _guid = new Guid();
            _fairnessValue = new Percent(0);
            _schedulingOptions = _mocks.StrictMock<ISchedulingOptions>();
        }

        [Test]
        public void ShouldMap()
        {
            using (_mocks.Record())
            {
                Expect.Call(_schedulingOptions.TagToUseOnScheduling).Return(_scheduleTag);
                Expect.Call(_scheduleTag.Id).Return(_guid);
                MapFromExpectations();

                Expect.Call(_scheduleTag.Id).Return(_guid);
                Expect.Call(_schedulingOptions.TagToUseOnScheduling).Return(_scheduleTag);
                Expect.Call(() => _schedulingOptions.TagToUseOnScheduling = _scheduleTag);
                MapToExpectations();
            }

            using (_mocks.Playback())
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
                _scheduleTags = new List<IScheduleTag>();
                _target.MapFrom(_schedulingOptions);
                _target.MapTo(_schedulingOptions, _scheduleTags, _groupPages);
            }
        }

        private void MapFromExpectations()
        {
            Expect.Call(_schedulingOptions.UseBlockScheduling).Return(BlockFinderType.BetweenDayOff);
            Expect.Call(_schedulingOptions.UseGroupScheduling).Return(true);
            Expect.Call(_groupPageLight.Key).Return("groupPageKey");
            Expect.Call(_schedulingOptions.GroupOnGroupPage).Return(_groupPageLight);
            Expect.Call(_schedulingOptions.Fairness).Return(_fairnessValue);
            Expect.Call(_schedulingOptions.GroupPageForShiftCategoryFairness).Return(_groupPageLight);
            Expect.Call(_schedulingOptions.ResourceCalculateFrequency).Return(_resourceCalculateFrequency);
            Expect.Call(_schedulingOptions.RefreshRate).Return(1);
            Expect.Call(_schedulingOptions.UseGroupSchedulingCommonStart).Return(true);
            Expect.Call(_schedulingOptions.UseGroupSchedulingCommonEnd).Return(true);
            Expect.Call(_schedulingOptions.UseGroupSchedulingCommonCategory).Return(false);
        }


        private void MapToExpectations()
        {
            Expect.Call(() => _schedulingOptions.UseBlockScheduling = BlockFinderType.BetweenDayOff);
            Expect.Call(() => _schedulingOptions.UseGroupScheduling = true);
            Expect.Call(_groupPageLight.Key).Return("groupPageKey").Repeat.AtLeastOnce();
            Expect.Call(() => _schedulingOptions.GroupOnGroupPage = _groupPageLight);
            Expect.Call(() => _schedulingOptions.GroupPageForShiftCategoryFairness = _groupPageLight);
            Expect.Call(() => _schedulingOptions.ResourceCalculateFrequency = _resourceCalculateFrequency);
            Expect.Call(_schedulingOptions.RefreshRate = 1);

            Expect.Call(() => _schedulingOptions.UseGroupSchedulingCommonStart = true);
            Expect.Call(() => _schedulingOptions.UseGroupSchedulingCommonEnd = true);
            Expect.Call(() => _schedulingOptions.UseGroupSchedulingCommonCategory = false);
            Expect.Call(() => _schedulingOptions.Fairness = _fairnessValue);
        }
    }
}
