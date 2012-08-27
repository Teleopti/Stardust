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

		private BlockFinderType _blockFinderTypeValue;

		private bool _useBlockScheduling;
		private bool _useTeams;
		private bool _useSameDaysOffForTeams;
       	private double _fairnessValue;

	    private bool _useGroupSchedulingCommonStart;
	    private bool _useGroupSchedulingCommonEnd;
	    private bool _useGroupSchedulingCommonCategory;

		public ExtraPreferencesPersonalSettings()
		{
			SetDefaultValues();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public void MapTo(IExtraPreferences target, IList<IGroupPageLight> groupPages)
		{
			InParameter.NotNull("groupPages", groupPages);

			foreach (var groupPage in groupPages)
			{
				if (_groupPageOnTeamKey == groupPage.Key)
					target.GroupPageOnTeam = groupPage;
				if (_groupPageOnCompareWithKey == groupPage.Key)
					target.GroupPageOnCompareWith = groupPage;
			}

			target.BlockFinderTypeValue = _blockFinderTypeValue;
			target.UseBlockScheduling = _useBlockScheduling;
			target.UseTeams = _useTeams;
			target.KeepSameDaysOffInTeam = _useSameDaysOffForTeams;

           
			target.FairnessValue = _fairnessValue;

		    target.UseGroupSchedulingCommonCategory = _useGroupSchedulingCommonCategory;
		    target.UseGroupSchedulingCommonEnd = _useGroupSchedulingCommonEnd;
		    target.UseGroupSchedulingCommonStart = _useGroupSchedulingCommonStart;
		}

		public void MapFrom(IExtraPreferences source)
		{
			if (source.GroupPageOnTeam != null)
				_groupPageOnTeamKey = source.GroupPageOnTeam.Key;
			if (source.GroupPageOnCompareWith != null)
				_groupPageOnCompareWithKey = source.GroupPageOnCompareWith.Key;

			_blockFinderTypeValue = source.BlockFinderTypeValue;
			_useBlockScheduling = source.UseBlockScheduling;
			_useTeams = source.UseTeams;
			_useSameDaysOffForTeams = source.KeepSameDaysOffInTeam;

            _fairnessValue = source.FairnessValue;

		    _useGroupSchedulingCommonCategory = source.UseGroupSchedulingCommonCategory;
		    _useGroupSchedulingCommonEnd = source.UseGroupSchedulingCommonEnd;
		    _useGroupSchedulingCommonStart = source.UseGroupSchedulingCommonStart;
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

		private void SetDefaultValues()
		{
			_blockFinderTypeValue = BlockFinderType.BetweenDayOff;
			
		}

	}
}
