using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    [Serializable]
    public class SchedulingOptionsExtraPersonalSetting : SettingValue
    {
        private BlockFinderType _blockFinderType = BlockFinderType.None;
        private bool _useGroupScheduling;
        private string _groupSchedulingGroupPageKey;
        private Guid? _scheduleTagId;
        private bool _useGroupSchedulingCommonStart;
        private bool _useGroupSchedulingCommonEnd;
        private bool _useGroupSchedulingCommonCategory;

        private double _fairnessValue;
        private string _fairnessGroupPageKey;
        private int _resourceCalculateFrequency = 1;
        private int _screenRefreshRate = 10;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void MapTo(ISchedulingOptions schedulingOptions, IList<IScheduleTag> scheduleTags, IList<IGroupPage> groupPages)
        {
            foreach (var scheduleTag in scheduleTags)
            {
                if (_scheduleTagId == scheduleTag.Id)
                    schedulingOptions.TagToUseOnScheduling = scheduleTag;
            }
            if (schedulingOptions.TagToUseOnScheduling == null)
                schedulingOptions.TagToUseOnScheduling = NullScheduleTag.Instance;

           schedulingOptions.UseBlockScheduling = _blockFinderType;
            schedulingOptions.UseGroupScheduling = _useGroupScheduling;

            foreach (var groupPage in groupPages)
            {
                if (_groupSchedulingGroupPageKey == groupPage.Key)
                    schedulingOptions.GroupOnGroupPage = groupPage;
            }


            schedulingOptions.UseGroupSchedulingCommonStart = _useGroupSchedulingCommonStart;
            schedulingOptions.UseGroupSchedulingCommonEnd = _useGroupSchedulingCommonEnd;
            schedulingOptions.UseGroupSchedulingCommonCategory = _useGroupSchedulingCommonCategory;

            schedulingOptions.Fairness = new Percent(_fairnessValue);

            foreach (var groupPage in groupPages)
            {
                if (_fairnessGroupPageKey == groupPage.Key)
                    schedulingOptions.GroupPageForShiftCategoryFairness = groupPage;
            }

            if (_resourceCalculateFrequency < 1 || _resourceCalculateFrequency > 10)
                _resourceCalculateFrequency = 1;
            schedulingOptions.ResourceCalculateFrequency = _resourceCalculateFrequency;

            if (_screenRefreshRate < 1 || _screenRefreshRate > 999)
                _screenRefreshRate = 10;
            schedulingOptions.RefreshRate = _screenRefreshRate;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void MapFrom(ISchedulingOptions schedulingOptions)
        {
            _scheduleTagId = schedulingOptions.TagToUseOnScheduling.Id;
             _blockFinderType = schedulingOptions.UseBlockScheduling;
            _useGroupScheduling = schedulingOptions.UseGroupScheduling;
            _groupSchedulingGroupPageKey = schedulingOptions.GroupOnGroupPage.Key;
           _useGroupSchedulingCommonStart = schedulingOptions.UseGroupSchedulingCommonStart;
            _useGroupSchedulingCommonEnd = schedulingOptions.UseGroupSchedulingCommonEnd;
            _useGroupSchedulingCommonCategory = schedulingOptions.UseGroupSchedulingCommonCategory;

            _fairnessValue = schedulingOptions.Fairness.Value;
            _fairnessGroupPageKey = schedulingOptions.GroupPageForShiftCategoryFairness.Key;
            _resourceCalculateFrequency = schedulingOptions.ResourceCalculateFrequency;
            _screenRefreshRate = schedulingOptions.RefreshRate;

        }

    }
}