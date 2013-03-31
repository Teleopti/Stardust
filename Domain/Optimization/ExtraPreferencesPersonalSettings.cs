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
        private string _groupPageOnTeamLevelingPerKey;

		private BlockFinderType _blockFinderTypeValue;

		//private bool _useBlockScheduling;
		private bool _useTeams;
		private bool _useSameDaysOffForTeams;
       	private double _fairnessValue;

	    private bool _useGroupSchedulingCommonStart;
	    private bool _useGroupSchedulingCommonEnd;
	    private bool _useGroupSchedulingCommonCategory;

        private bool _useLevellingSameEndTime;
        private bool _useLevellingSameShiftCategory;
        private bool _useLevellingSameStartTime;
        private bool _useLevellingSameShift;
        private bool _useLevellingOption;

	    private BlockFinderType _blockFinderTypeForAdvanceOptimization;
	    

	    public ExtraPreferencesPersonalSettings()
		{
			SetDefaultValues();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public void MapTo(IExtraPreferences target, IList<IGroupPageLight> groupPages, IList<IGroupPageLight> groupPagesForLevelingPer)
		{
		    if (groupPagesForLevelingPer == null) throw new ArgumentNullException("groupPagesForLevelingPer");
		    InParameter.NotNull("groupPages", groupPages);

			foreach (var groupPage in groupPages)
			{
				if (_groupPageOnTeamKey == groupPage.Key)
					target.GroupPageOnTeam = groupPage;
				if (_groupPageOnCompareWithKey == groupPage.Key)
					target.GroupPageOnCompareWith = groupPage;
			}

            foreach (var groupPage in groupPagesForLevelingPer)
            {
                if (_groupPageOnTeamLevelingPerKey == groupPage.Key)
                    target.GroupPageOnTeamLevelingPer = groupPage;
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

		    target.UseLevellingOption = _useLevellingOption;
		    target.UseLevellingSameEndTime = _useLevellingSameEndTime;
		    target.UseLevellingSameShift = _useLevellingSameShift;
		    target.UseLevellingSameShiftCategory = _useLevellingSameShiftCategory;
		    target.UseLevellingSameStartTime = _useLevellingSameStartTime;
		}

		public void MapFrom(IExtraPreferences source)
		{
			if (source.GroupPageOnTeam != null)
				_groupPageOnTeamKey = source.GroupPageOnTeam.Key;
			if (source.GroupPageOnCompareWith != null)
				_groupPageOnCompareWithKey = source.GroupPageOnCompareWith.Key;
            if (source.GroupPageOnTeamLevelingPer != null)
                _groupPageOnTeamLevelingPerKey = source.GroupPageOnTeamLevelingPer.Key;

			_blockFinderTypeValue = source.BlockFinderTypeValue;
			//_useBlockScheduling = source.UseBlockScheduling;
			_useTeams = source.UseTeams;
			_useSameDaysOffForTeams = source.KeepSameDaysOffInTeam;

            _fairnessValue = source.FairnessValue;

		    _useGroupSchedulingCommonCategory = source.UseGroupSchedulingCommonCategory;
		    _useGroupSchedulingCommonEnd = source.UseGroupSchedulingCommonEnd;
		    _useGroupSchedulingCommonStart = source.UseGroupSchedulingCommonStart;

		    _blockFinderTypeForAdvanceOptimization = source.BlockFinderTypeForAdvanceOptimization;

		    _useLevellingOption = source.UseLevellingOption;
		    _useLevellingSameEndTime = source.UseLevellingSameEndTime;
		    _useLevellingSameShift = source.UseLevellingSameShift;
		    _useLevellingSameShiftCategory = source.UseLevellingSameShiftCategory;
		    _useLevellingSameStartTime = source.UseLevellingSameStartTime;
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
        /// Sets the group page on cleveling per.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <remarks>Used in tests only</remarks>
        public void SetGroupPageOnTeamLevelingPerKey(string key)
        {
            _groupPageOnTeamLevelingPerKey = key;
        }

		private void SetDefaultValues()
		{
			_blockFinderTypeValue = BlockFinderType.BetweenDayOff;
			
		}

	}
}
