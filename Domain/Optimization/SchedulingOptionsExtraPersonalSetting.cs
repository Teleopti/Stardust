using System;
using System.Collections.Generic;
using System.Linq;
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

        private string _groupSchedlingForLevelingPerKey;
        private bool _useLevellingSameEndTime;
        private bool _useLevellingSameShiftCategory;
        private bool _useLevellingSameStartTime;
        private bool _useLevellingSameShift;
        private bool _useLevellingPerOption;
        
        private Guid? _scheduleTagId;
        private bool _useGroupSchedulingCommonStart;
        private bool _useGroupSchedulingCommonEnd;
        private bool _useGroupSchedulingCommonCategory;
        private bool _useCommmonActivity;
        private Guid?  _commonActivtyId;
        private BlockFinderType _blockFinderTypeForAdvanceScheduling = BlockFinderType.None;

        private double _fairnessValue;
        private string _fairnessGroupPageKey;
        private int _resourceCalculateFrequency = 1;
        private int _screenRefreshRate = 10;
        

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void MapTo(ISchedulingOptions schedulingOptions, IList<IScheduleTag> scheduleTags, IList<IGroupPageLight> groupPages, IList<IActivity> activityList)
        {
            foreach (var scheduleTag in scheduleTags)
            {
                if (_scheduleTagId == scheduleTag.Id)
                    schedulingOptions.TagToUseOnScheduling = scheduleTag;
            }
            if (schedulingOptions.TagToUseOnScheduling == null)
                schedulingOptions.TagToUseOnScheduling = NullScheduleTag.Instance;

           schedulingOptions.UseBlockScheduling = _blockFinderType;
            schedulingOptions.BlockFinderTypeForAdvanceScheduling = _blockFinderTypeForAdvanceScheduling;
            schedulingOptions.UseGroupScheduling = _useGroupScheduling;

            foreach (var groupPage in groupPages)
            {
                if (_groupSchedulingGroupPageKey == groupPage.Key)
                    schedulingOptions.GroupOnGroupPage = groupPage;
            }

            //for leve per
            foreach (var groupPage in groupPages)
            {
                if (_groupSchedlingForLevelingPerKey  == groupPage.Key)
                    schedulingOptions.GroupOnGroupPageForLevelingPer = groupPage;
            }
            schedulingOptions.UseLevellingSameEndTime = _useLevellingSameEndTime;
            schedulingOptions.UseLevellingSameShift = _useLevellingSameShift;
            schedulingOptions.UseLevellingSameShiftCategory = _useLevellingSameShiftCategory;
            schedulingOptions.UseLevellingSameStartTime = _useLevellingSameStartTime;
            schedulingOptions.UseLevellingPerOption = _useLevellingPerOption;

            schedulingOptions.UseGroupSchedulingCommonStart = _useGroupSchedulingCommonStart;
            schedulingOptions.UseGroupSchedulingCommonEnd = _useGroupSchedulingCommonEnd;
            schedulingOptions.UseGroupSchedulingCommonCategory = _useGroupSchedulingCommonCategory;
            schedulingOptions.UseCommonActivity = _useCommmonActivity;
            if (activityList != null & _commonActivtyId.HasValue)
                schedulingOptions.CommonActivity = activityList.FirstOrDefault(x => x.Id == _commonActivtyId);

            schedulingOptions.Fairness = new Percent(_fairnessValue);

            foreach (var groupPage in groupPages)
            {
                if (_fairnessGroupPageKey == groupPage.Key)
                    schedulingOptions.GroupPageForShiftCategoryFairness = groupPage;
            }

            schedulingOptions.ResourceCalculateFrequency = _resourceCalculateFrequency;
            if (_resourceCalculateFrequency < 1 || _resourceCalculateFrequency > 10)
                schedulingOptions.ResourceCalculateFrequency = 1;

            schedulingOptions.RefreshRate = _screenRefreshRate;
            if (_screenRefreshRate < 1 || _screenRefreshRate > 999)
                schedulingOptions.RefreshRate = 10;
            
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void MapFrom(ISchedulingOptions schedulingOptions)
        {
            _scheduleTagId = schedulingOptions.TagToUseOnScheduling.Id;
             _blockFinderType = schedulingOptions.UseBlockScheduling;
            _blockFinderTypeForAdvanceScheduling = schedulingOptions.BlockFinderTypeForAdvanceScheduling;
            _useGroupScheduling = schedulingOptions.UseGroupScheduling;
            _groupSchedulingGroupPageKey = schedulingOptions.GroupOnGroupPage.Key;
           _useGroupSchedulingCommonStart = schedulingOptions.UseGroupSchedulingCommonStart;
            _useGroupSchedulingCommonEnd = schedulingOptions.UseGroupSchedulingCommonEnd;
            _useGroupSchedulingCommonCategory = schedulingOptions.UseGroupSchedulingCommonCategory;
            _useCommmonActivity = schedulingOptions.UseCommonActivity;

            _fairnessValue = schedulingOptions.Fairness.Value;
            _fairnessGroupPageKey = schedulingOptions.GroupPageForShiftCategoryFairness.Key;
            _resourceCalculateFrequency = schedulingOptions.ResourceCalculateFrequency;
            _screenRefreshRate = schedulingOptions.RefreshRate;
            _commonActivtyId = schedulingOptions.CommonActivity != null ? schedulingOptions.CommonActivity.Id : null;

            if (schedulingOptions.GroupOnGroupPageForLevelingPer!=null)
                _groupSchedlingForLevelingPerKey = schedulingOptions.GroupOnGroupPageForLevelingPer.Key;
            _useLevellingSameEndTime = schedulingOptions.UseLevellingSameEndTime;
            _useLevellingSameShift = schedulingOptions.UseLevellingSameShift;
            _useLevellingSameShiftCategory = schedulingOptions.UseLevellingSameShiftCategory;
            _useLevellingSameStartTime  = schedulingOptions.UseLevellingSameStartTime ;
            _useLevellingPerOption = schedulingOptions.UseLevellingPerOption;
        }
    }
}