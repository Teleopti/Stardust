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
					target.TeamGroupPage = groupPage;
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
			target.UseTeamSameDaysOff = _useSameDaysOffForTeams;
            
           
			target.FairnessValue = _fairnessValue;

		    target.UseTeamSameShiftCategory = _useGroupSchedulingCommonCategory;
		    target.UseTeamSameEndTime = _useGroupSchedulingCommonEnd;
		    target.UseTeamSameStartTime = _useGroupSchedulingCommonStart;

		    target.BlockTypeValue = _blockFinderTypeForAdvanceOptimization;

		    target.UseTeamBlockOption = _useTeamBlockOption;
		    target.UseBlockSameEndTime = _useTeamBlockSameEndTime;
		    target.UseBlockSameShift = _useTeamBlockSameShift;
		    target.UseBlockSameShiftCategory = _useTeamBlockSameShiftCategory;
		    target.UseBlockSameStartTime = _useTeamBlockSameStartTime;
		}

		public void MapFrom(IExtraPreferences source)
		{
			if (source.TeamGroupPage != null)
				_groupPageOnTeamKey = source.TeamGroupPage.Key;
			if (source.GroupPageOnCompareWith != null)
				_groupPageOnCompareWithKey = source.GroupPageOnCompareWith.Key;
            if (source.GroupPageOnTeamBlockPer != null)
                _groupPageOnTeamBlockPerKey = source.GroupPageOnTeamBlockPer.Key;

			_blockFinderTypeValue = source.BlockFinderTypeValue;
			//_useBlockScheduling = source.UseBlockScheduling;
			_useTeams = source.UseTeams;
			_useSameDaysOffForTeams = source.UseTeamSameDaysOff;

            _fairnessValue = source.FairnessValue;

		    _useGroupSchedulingCommonCategory = source.UseTeamSameShiftCategory;
		    _useGroupSchedulingCommonEnd = source.UseTeamSameEndTime;
		    _useGroupSchedulingCommonStart = source.UseTeamSameStartTime;

		    _blockFinderTypeForAdvanceOptimization = source.BlockTypeValue;

		    _useTeamBlockOption = source.UseTeamBlockOption;
		    _useTeamBlockSameEndTime = source.UseBlockSameEndTime;
		    _useTeamBlockSameShift = source.UseBlockSameShift;
		    _useTeamBlockSameShiftCategory = source.UseBlockSameShiftCategory;
		    _useTeamBlockSameStartTime = source.UseBlockSameStartTime;
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
