using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	[Serializable]
	public class ExtraPreferencesPersonalSettings : SettingValue
	{
		private string _groupPageOnTeamKey;
		private string _groupPageOnCompareWithKey;
        private string _groupPageOnTeamBlockPerKey;

		private BlockFinderType _blockFinderTypeValue;

		//private bool _useBlockScheduling;
		private bool _useTeams;
		private bool _useSameDaysOffForTeams;
       	private double _fairnessValue;

	    private bool _useGroupSchedulingCommonStart;
	    private bool _useGroupSchedulingCommonEnd;
	    private bool _useGroupSchedulingCommonCategory;

        private bool _useTeamBlockSameEndTime;
        private bool _useTeamBlockSameShiftCategory;
        private bool _useTeamBlockSameStartTime;
        private bool _useTeamBlockSameShift;
        private bool _useTeamBlockOption;

	    private BlockFinderType _blockFinderTypeForAdvanceOptimization;
	    

	    public ExtraPreferencesPersonalSettings()
		{
			SetDefaultValues();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public void MapTo(IExtraPreferences target, IList<IGroupPageLight> groupPages, IList<IGroupPageLight> groupPagesForTeamBlockPer)
		{
		    if (groupPagesForTeamBlockPer == null) throw new ArgumentNullException("groupPagesForTeamBlockPer");
		    InParameter.NotNull("groupPages", groupPages);

			foreach (var groupPage in groupPages)
			{
				if (_groupPageOnTeamKey == groupPage.Key)
					target.GroupPageOnTeam = groupPage;
				if (_groupPageOnCompareWithKey == groupPage.Key)
					target.GroupPageOnCompareWith = groupPage;
			}

            foreach (var groupPage in groupPagesForTeamBlockPer)
            {
                if (_groupPageOnTeamBlockPerKey == groupPage.Key)
                    target.GroupPageOnTeamBlockPer = groupPage;
            }

			target.BlockFinderTypeValue = _blockFinderTypeValue;
			//target.UseBlockScheduling = _useBlockScheduling;
			target.UseTeams = _useTeams;
			target.KeepSameDaysOffInTeam = _useSameDaysOffForTeams;
            
           
			target.FairnessValue = _fairnessValue;

		    target.UseGroupSchedulingCommonCategory = _useGroupSchedulingCommonCategory;
		    target.UseGroupSchedulingCommonEnd = _useGroupSchedulingCommonEnd;
		    target.UseGroupSchedulingCommonStart = _useGroupSchedulingCommonStart;

		    target.BlockFinderTypeForAdvanceOptimization = _blockFinderTypeForAdvanceOptimization;

		    target.UseTeamBlockOption = _useTeamBlockOption;
		    target.UseTeamBlockSameEndTime = _useTeamBlockSameEndTime;
		    target.UseTeamBlockSameShift = _useTeamBlockSameShift;
		    target.UseTeamBlockSameShiftCategory = _useTeamBlockSameShiftCategory;
		    target.UseTeamBlockSameStartTime = _useTeamBlockSameStartTime;
		}

		public void MapFrom(IExtraPreferences source)
		{
			if (source.GroupPageOnTeam != null)
				_groupPageOnTeamKey = source.GroupPageOnTeam.Key;
			if (source.GroupPageOnCompareWith != null)
				_groupPageOnCompareWithKey = source.GroupPageOnCompareWith.Key;
            if (source.GroupPageOnTeamBlockPer != null)
                _groupPageOnTeamBlockPerKey = source.GroupPageOnTeamBlockPer.Key;

			_blockFinderTypeValue = source.BlockFinderTypeValue;
			//_useBlockScheduling = source.UseBlockScheduling;
			_useTeams = source.UseTeams;
			_useSameDaysOffForTeams = source.KeepSameDaysOffInTeam;

            _fairnessValue = source.FairnessValue;

		    _useGroupSchedulingCommonCategory = source.UseGroupSchedulingCommonCategory;
		    _useGroupSchedulingCommonEnd = source.UseGroupSchedulingCommonEnd;
		    _useGroupSchedulingCommonStart = source.UseGroupSchedulingCommonStart;

		    _blockFinderTypeForAdvanceOptimization = source.BlockFinderTypeForAdvanceOptimization;

		    _useTeamBlockOption = source.UseTeamBlockOption;
		    _useTeamBlockSameEndTime = source.UseTeamBlockSameEndTime;
		    _useTeamBlockSameShift = source.UseTeamBlockSameShift;
		    _useTeamBlockSameShiftCategory = source.UseTeamBlockSameShiftCategory;
		    _useTeamBlockSameStartTime = source.UseTeamBlockSameStartTime;
		}

		/// <summary>
		/// Sets the group page on team key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <remarks>Used in tests only</remarks>
		public void SetGroupPageOnTeamKey(string key)
		{
			_groupPageOnTeamKey = key;
		}

		/// <summary>
		/// Sets the group page on compare with key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <remarks>Used in tests only</remarks>
		public void SetGroupPageOnCompareWithKey(string key)
		{
			_groupPageOnCompareWithKey = key;
		}

        /// <summary>
        /// Sets the group page on TeamBlock per.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <remarks>Used in tests only</remarks>
        public void SetGroupPageOnTeamBlockPerKey(string key)
        {
            _groupPageOnTeamBlockPerKey = key;
        }

		private void SetDefaultValues()
		{
			_blockFinderTypeValue = BlockFinderType.BetweenDayOff;
			
		}

	}
}
