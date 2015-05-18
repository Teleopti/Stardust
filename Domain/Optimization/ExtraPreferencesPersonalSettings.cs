﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	[Serializable]
	public class ExtraPreferencesPersonalSettings : SettingValue
	{
		private string _groupPageOnTeamKey;
		private bool _useTeams;
		private bool _useSameDaysOffForTeams;

	    private bool _useGroupSchedulingCommonStart;
	    private bool _useGroupSchedulingCommonEnd;
	    private bool _useGroupSchedulingCommonCategory;

        private bool _useTeamBlockSameEndTime;
        private bool _useTeamBlockSameShiftCategory;
		private bool _useTeamBlockSameActivity;
        private bool _useTeamBlockSameStartTime;
        private bool _useTeamBlockSameShift;
        private bool _useTeamBlockOption;

	    private BlockFinderType _blockFinderTypeForAdvanceOptimization;
	    

	   [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public void MapTo(IExtraPreferences target, IList<GroupPageLight> groupPages, IList<GroupPageLight> groupPagesForTeamBlockPer)
		{
		    if (groupPagesForTeamBlockPer == null) throw new ArgumentNullException("groupPagesForTeamBlockPer");
		    InParameter.NotNull("groupPages", groupPages);

			foreach (var groupPage in groupPages)
			{
				if (_groupPageOnTeamKey == groupPage.Key)
					target.TeamGroupPage = groupPage;
			}

			target.UseTeams = _useTeams;
			target.UseTeamSameDaysOff = _useSameDaysOffForTeams;

		    target.UseTeamSameShiftCategory = _useGroupSchedulingCommonCategory;
		    target.UseTeamSameEndTime = _useGroupSchedulingCommonEnd;
		    target.UseTeamSameStartTime = _useGroupSchedulingCommonStart;

		    target.BlockTypeValue = _blockFinderTypeForAdvanceOptimization;

		    target.UseTeamBlockOption = _useTeamBlockOption;
		    target.UseBlockSameEndTime = _useTeamBlockSameEndTime;
		    target.UseBlockSameShift = _useTeamBlockSameShift;
		    target.UseBlockSameShiftCategory = _useTeamBlockSameShiftCategory;
		    target.UseBlockSameStartTime = _useTeamBlockSameStartTime;
			target.UseTeamSameActivity = _useTeamBlockSameActivity;
		}

		public void MapFrom(IExtraPreferences source)
		{
			_groupPageOnTeamKey = source.TeamGroupPage.Key;

			_useTeams = source.UseTeams;
			_useSameDaysOffForTeams = source.UseTeamSameDaysOff;

		    _useGroupSchedulingCommonCategory = source.UseTeamSameShiftCategory;
		    _useGroupSchedulingCommonEnd = source.UseTeamSameEndTime;
		    _useGroupSchedulingCommonStart = source.UseTeamSameStartTime;

		    _blockFinderTypeForAdvanceOptimization = source.BlockTypeValue;

		    _useTeamBlockOption = source.UseTeamBlockOption;
		    _useTeamBlockSameEndTime = source.UseBlockSameEndTime;
		    _useTeamBlockSameShift = source.UseBlockSameShift;
		    _useTeamBlockSameShiftCategory = source.UseBlockSameShiftCategory;
		    _useTeamBlockSameStartTime = source.UseBlockSameStartTime;
			_useTeamBlockSameActivity = source.UseTeamSameActivity;
		}
	}
}
