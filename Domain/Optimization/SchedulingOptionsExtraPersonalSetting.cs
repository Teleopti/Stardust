using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.SystemSetting;

namespace Teleopti.Ccc.Domain.Optimization
{
    [Serializable]
    public class SchedulingOptionsExtraPersonalSetting : SettingValue
    {

        private bool _useGroupScheduling; //take away

        private string _groupSchedlingForTeamBlockPerKey;
        private bool _useTeamBlockSameEndTime;
	    private bool _useTeamBlockSameShiftCategory = true;
        private bool _useTeamBlockSameStartTime;
        private bool _useTeamBlockSameShift;
        private bool _useTeamBlockPerOption;

		private BlockFinderType _blockFinderTypeForAdvanceScheduling = BlockFinderType.SingleDay;
        private bool _useGroupSchedulingCommonStart;
        private bool _useGroupSchedulingCommonEnd;
	    private bool _useGroupSchedulingCommonCategory = true;
        private bool _useCommmonActivity;
        private Guid?  _commonActivtyId;

		//Move elsewhere
		private Guid? _scheduleTagId;

	    public void MapTo(SchedulingOptions schedulingOptions, IEnumerable<IScheduleTag> scheduleTags,
		    IList<GroupPageLight> groupPages, IList<GroupPageLight> groupPagesForTeamBlockPer,
		    IEnumerable<IActivity> activityList)
	    {
		    foreach (var scheduleTag in scheduleTags)
		    {
			    if (_scheduleTagId == scheduleTag.Id)
				    schedulingOptions.TagToUseOnScheduling = scheduleTag;
		    }
		    if (schedulingOptions.TagToUseOnScheduling == null)
			    schedulingOptions.TagToUseOnScheduling = NullScheduleTag.Instance;

		    schedulingOptions.BlockFinderTypeForAdvanceScheduling = _blockFinderTypeForAdvanceScheduling;
		    schedulingOptions.UseTeam = _useGroupScheduling;

		    foreach (var groupPage in groupPagesForTeamBlockPer)
		    {
			    if (_groupSchedlingForTeamBlockPerKey == groupPage.Key)
				    schedulingOptions.GroupOnGroupPageForTeamBlockPer = groupPage;
		    }
		    schedulingOptions.BlockSameEndTime = _useTeamBlockSameEndTime;
		    schedulingOptions.BlockSameShift = _useTeamBlockSameShift;
		    schedulingOptions.BlockSameShiftCategory = _useTeamBlockSameShiftCategory;
		    schedulingOptions.BlockSameStartTime = _useTeamBlockSameStartTime;
		    schedulingOptions.UseBlock = _useTeamBlockPerOption;

		    schedulingOptions.TeamSameStartTime = _useGroupSchedulingCommonStart;
		    schedulingOptions.TeamSameEndTime = _useGroupSchedulingCommonEnd;
		    schedulingOptions.TeamSameShiftCategory = _useGroupSchedulingCommonCategory;
		    schedulingOptions.TeamSameActivity = _useCommmonActivity;
		    if (activityList != null & _commonActivtyId.HasValue)
			    schedulingOptions.CommonActivity = activityList.FirstOrDefault(x => x.Id == _commonActivtyId);
	    }

	    public void MapFrom(SchedulingOptions schedulingOptions)
	    {
		    _scheduleTagId = schedulingOptions.TagToUseOnScheduling.Id;

		    _blockFinderTypeForAdvanceScheduling = schedulingOptions.BlockFinderTypeForAdvanceScheduling;
		    _useGroupScheduling = schedulingOptions.UseTeam;

		    _useGroupSchedulingCommonStart = schedulingOptions.TeamSameStartTime;
		    _useGroupSchedulingCommonEnd = schedulingOptions.TeamSameEndTime;
		    _useGroupSchedulingCommonCategory = schedulingOptions.TeamSameShiftCategory;
		    _useCommmonActivity = schedulingOptions.TeamSameActivity;
		    _commonActivtyId = schedulingOptions.CommonActivity != null ? schedulingOptions.CommonActivity.Id : null;

		    _groupSchedlingForTeamBlockPerKey = schedulingOptions.GroupOnGroupPageForTeamBlockPer.Key;
		    _useTeamBlockSameEndTime = schedulingOptions.BlockSameEndTime;
		    _useTeamBlockSameShift = schedulingOptions.BlockSameShift;
		    _useTeamBlockSameShiftCategory = schedulingOptions.BlockSameShiftCategory;
		    _useTeamBlockSameStartTime = schedulingOptions.BlockSameStartTime;
		    _useTeamBlockPerOption = schedulingOptions.UseBlock;
	    }
    }
}