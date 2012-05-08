using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	[Serializable]
	public class ExtraPreferencesPersonalSettings : SettingValue
	{
		private Guid? _groupPageOnTeamId;
		private Guid? _groupPageOnCompareWithId;

		private BlockFinderType _blockFinderTypeValue;

		private bool _useBlockScheduling;
		private bool _useTeams;
		private bool _keepShiftCategories;
		private bool _keepStartAndEndTimes;
		private bool _keepShifts;

		private double _keepShiftsValue;
		private double _fairnessValue;

		public ExtraPreferencesPersonalSettings()
		{
			SetDefaultValues();
		}

		public void MapTo(IExtraPreferences target, IList<IGroupPage> groupPages)
		{
			InParameter.NotNull("groupPages", groupPages);

			foreach (var groupPage in groupPages)
			{
				if (_groupPageOnTeamId == groupPage.Id)
					target.GroupPageOnTeam = groupPage;
				if (_groupPageOnCompareWithId == groupPage.Id)
					target.GroupPageOnCompareWith = groupPage;
			}

			target.BlockFinderTypeValue = _blockFinderTypeValue;
			target.UseBlockScheduling = _useBlockScheduling;
			target.UseTeams = _useTeams;

			target.KeepShiftCategories = _keepShiftCategories;
			target.KeepStartAndEndTimes = _keepStartAndEndTimes;
			target.KeepShifts = _keepShifts;

			target.KeepShiftsValue = _keepShiftsValue;
			target.FairnessValue = _fairnessValue;
		}

		public void MapFrom(IExtraPreferences source)
		{
			if (source.GroupPageOnTeam != null)
				_groupPageOnTeamId = source.GroupPageOnTeam.Id;
			if (source.GroupPageOnCompareWith != null)
				_groupPageOnCompareWithId = source.GroupPageOnCompareWith.Id;

			_blockFinderTypeValue = source.BlockFinderTypeValue;
			_useBlockScheduling = source.UseBlockScheduling;
			_useTeams = source.UseTeams;

			_keepShiftCategories = source.KeepShiftCategories;
			_keepStartAndEndTimes = source.KeepStartAndEndTimes;
			_keepShifts = source.KeepShifts;

			_keepShiftsValue = source.KeepShiftsValue;
			_fairnessValue = source.FairnessValue;
		}

		/// <summary>
		/// Sets the group page on team id.
		/// </summary>
		/// <param name="guid">The GUID.</param>
		/// <remarks>Used in tests only</remarks>
		public void SetGroupPageOnTeamId(Guid guid)
		{
			_groupPageOnTeamId = guid;
		}

		/// <summary>
		/// Sets the group page on compare with id.
		/// </summary>
		/// <param name="guid">The GUID.</param>
		/// <remarks>Used in tests only</remarks>
		public void SetGroupPageOnCompareWithId(Guid guid)
		{
			_groupPageOnCompareWithId = guid;
		}

		private void SetDefaultValues()
		{
			_blockFinderTypeValue = BlockFinderType.BetweenDayOff;
			_keepShiftsValue = 0.8d;
		}

	}
}
