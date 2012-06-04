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
		private bool _keepShiftCategories;
		private bool _keepStartAndEndTimes;
		private bool _keepShifts;

		private double _keepShiftsValue;
		private double _fairnessValue;

		public ExtraPreferencesPersonalSettings()
		{
			SetDefaultValues();
		}

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

			target.KeepShiftCategories = _keepShiftCategories;
			target.KeepStartAndEndTimes = _keepStartAndEndTimes;
			target.KeepShifts = _keepShifts;

			target.KeepShiftsValue = _keepShiftsValue;
			target.FairnessValue = _fairnessValue;
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

			_keepShiftCategories = source.KeepShiftCategories;
			_keepStartAndEndTimes = source.KeepStartAndEndTimes;
			_keepShifts = source.KeepShifts;

			_keepShiftsValue = source.KeepShiftsValue;
			_fairnessValue = source.FairnessValue;
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
			_keepShiftsValue = 0.8d;
		}

	}
}
