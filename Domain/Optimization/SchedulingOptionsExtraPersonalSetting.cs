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
        //private BlockFinderType _blockFinderType = BlockFinderType.None;
        private bool _useGroupScheduling;
        private string _groupSchedulingGroupPageKey;

        private string _groupSchedlingForTeamBlockPerKey;
        private bool _useTeamBlockSameEndTime;
        private bool _useTeamBlockSameShiftCategory;
        private bool _useTeamBlockSameStartTime;
        private bool _useTeamBlockSameShift;
        private bool _useTeamBlockPerOption;
        
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
        public void MapTo(ISchedulingOptions schedulingOptions, IEnumerable<IScheduleTag> scheduleTags, IList<IGroupPageLight> groupPages,IList<IGroupPageLight> groupPagesForTeamBlockPer, IEnumerable<IActivity> activityList)
        {
            if (groupPagesForTeamBlockPer == null) throw new ArgumentNullException("groupPagesForTeamBlockPer");
            foreach (var scheduleTag in scheduleTags)
            {
                if (_scheduleTagId == scheduleTag.Id)
                    schedulingOptions.TagToUseOnScheduling = scheduleTag;
            }
            if (schedulingOptions.TagToUseOnScheduling == null)
                schedulingOptions.TagToUseOnScheduling = NullScheduleTag.Instance;

            //schedulingOptions.UseBlockScheduling = _blockFinderType;
            schedulingOptions.BlockFinderTypeForAdvanceScheduling = _blockFinderTypeForAdvanceScheduling;
            schedulingOptions.UseGroupScheduling = _useGroupScheduling;

            foreach (var groupPage in groupPages)
            {
                if (_groupSchedulingGroupPageKey == groupPage.Key)
                    schedulingOptions.GroupOnGroupPage = groupPage;
            }

            //for leve per
            foreach (var groupPage in groupPagesForTeamBlockPer)
            {
                if (_groupSchedlingForTeamBlockPerKey  == groupPage.Key)
                    schedulingOptions.GroupOnGroupPageForTeamBlockPer = groupPage;
            }
            schedulingOptions.UseTeamBlockSameEndTime = _useTeamBlockSameEndTime;
            schedulingOptions.UseTeamBlockSameShift = _useTeamBlockSameShift;
            schedulingOptions.UseTeamBlockSameShiftCategory = _useTeamBlockSameShiftCategory;
            schedulingOptions.UseTeamBlockSameStartTime = _useTeamBlockSameStartTime;
            schedulingOptions.UseTeamBlockPerOption = _useTeamBlockPerOption;

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
             //_blockFinderType = schedulingOptions.UseBlockScheduling;
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

            if (schedulingOptions.GroupOnGroupPageForTeamBlockPer!=null)
                _groupSchedlingForTeamBlockPerKey = schedulingOptions.GroupOnGroupPageForTeamBlockPer.Key;
            _useTeamBlockSameEndTime = schedulingOptions.UseTeamBlockSameEndTime;
            _useTeamBlockSameShift = schedulingOptions.UseTeamBlockSameShift;
            _useTeamBlockSameShiftCategory = schedulingOptions.UseTeamBlockSameShiftCategory;
            _useTeamBlockSameStartTime  = schedulingOptions.UseTeamBlockSameStartTime ;
            _useTeamBlockPerOption = schedulingOptions.UseTeamBlockPerOption;
        }
    }
}