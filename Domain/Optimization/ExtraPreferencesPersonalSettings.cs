using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class ExtraPreferencesPersonalSettings : SettingValue, IPersonalSettings<IExtraPreferences>
	{
		private IList<IGroupPage> GroupPages { get; set; }

		private Guid? GroupPageOnTeamId { get; set; }
		private Guid? GroupPageOnCompareWithId { get; set; }

		private BlockFinderType BlockFinderTypeValue { get; set; }

		private bool UseBlockScheduling { get; set; }
		private bool UseTeams { get; set; }

        private bool KeepShiftCategories { get; set; }
        private bool KeepStartAndEndTimes { get; set; }
        private bool KeepShifts { get; set; }

        private double KeepShiftsValue { get; set; }
		private double FairnessValue { get; set; }

		public ExtraPreferencesPersonalSettings(IList<IGroupPage> groupPages)
		{
			GroupPages = groupPages;
			//BlockFinderTypeValue = BlockFinderType.BetweenDayOff;
		}

		public void MapTo(IExtraPreferences target)
		{
			foreach (var groupPage in GroupPages)
			{
				if (GroupPageOnTeamId == groupPage.Id)
					target.GroupPageOnTeam = groupPage;
				if (GroupPageOnCompareWithId == groupPage.Id)
					target.GroupPageOnCompareWith = groupPage;
			}

			target.BlockFinderTypeValue = BlockFinderTypeValue;
			target.UseBlockScheduling = UseBlockScheduling;
			target.UseTeams = UseTeams;

			target.KeepShiftCategories = KeepShiftCategories;
			target.KeepStartAndEndTimes = KeepStartAndEndTimes;
			target.KeepShifts = KeepShifts;

			target.KeepShiftsValue = KeepShiftsValue;
			target.FairnessValue = FairnessValue;
		}

		public void MapFrom(IExtraPreferences source)
		{
			if(source.GroupPageOnTeam != null)
				GroupPageOnTeamId = source.GroupPageOnTeam.Id;
			if (source.GroupPageOnCompareWith != null)
				GroupPageOnCompareWithId = source.GroupPageOnCompareWith.Id;

			BlockFinderTypeValue = source.BlockFinderTypeValue;
			UseBlockScheduling = source.UseBlockScheduling;
			UseTeams = source.UseTeams;

			KeepShiftCategories = source.KeepShiftCategories;
			KeepStartAndEndTimes = source.KeepStartAndEndTimes;
			KeepShifts = source.KeepShifts;

			KeepShiftsValue = source.KeepShiftsValue;
			FairnessValue = source.FairnessValue;
		}

		/// <summary>
		/// Sets the group page on team id.
		/// </summary>
		/// <param name="guid">The GUID.</param>
		/// <remarks>Used in tests only</remarks>
		public void SetGroupPageOnTeamId(Guid guid)
		{
			GroupPageOnTeamId = guid;
		}

		/// <summary>
		/// Sets the group page on compare with id.
		/// </summary>
		/// <param name="guid">The GUID.</param>
		/// <remarks>Used in tests only</remarks>
		public void SetGroupPageOnCompareWithId(Guid guid)
		{
			GroupPageOnCompareWithId = guid;
		}

	}
}
