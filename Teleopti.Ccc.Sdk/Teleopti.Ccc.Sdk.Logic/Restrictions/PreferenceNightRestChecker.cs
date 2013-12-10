using System;
using System.Collections.Generic;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.Logic.Restrictions
{
	public interface IPreferenceNightRestChecker
	{
		void CheckNightlyRest(IList<ValidatedSchedulePartDto> validatedSchedulePartDtos);
	}

	public class PreferenceNightRestChecker : IPreferenceNightRestChecker
	{
		private readonly INightlyRestFromPersonOnDayExtractor _nightlyRestFromPersonOnDayExtractor;

		public PreferenceNightRestChecker(INightlyRestFromPersonOnDayExtractor nightlyRestFromPersonOnDayExtractor)
		{
			_nightlyRestFromPersonOnDayExtractor = nightlyRestFromPersonOnDayExtractor;
		}

		public void CheckNightlyRest(IList<ValidatedSchedulePartDto> validatedSchedulePartDtos)
		{
			if(validatedSchedulePartDtos != null)
			{
				var lastIndex = validatedSchedulePartDtos.Count - 1;
				foreach (var validatedSchedulePartDto in validatedSchedulePartDtos)
				{
					var thisIndex = validatedSchedulePartDtos.IndexOf(validatedSchedulePartDto);
					if (thisIndex.Equals(lastIndex)) return;
					var nightlyRest = _nightlyRestFromPersonOnDayExtractor.NightlyRestOnDay(validatedSchedulePartDto.DateOnly);
					checkNightlyRest(validatedSchedulePartDto, validatedSchedulePartDtos[thisIndex + 1], nightlyRest);
				}
			}
		}

		private static void checkNightlyRest(ValidatedSchedulePartDto part1, ValidatedSchedulePartDto part2, TimeSpan nightlyRest)
		{
			if (part1 == null || part2 == null) return;
			if(part1.HasDayOff || part2.HasDayOff) return;
			if(part1.HasAbsence || part2.HasAbsence) return;
			if (part1.PreferenceRestriction != null && part1.PreferenceRestriction.DayOff != null) return;
			if(part2.PreferenceRestriction != null && part2.PreferenceRestriction.DayOff != null) return;
            if(part1.PreferenceRestriction != null && part1.PreferenceRestriction.Absence != null) return;
            if (part2.PreferenceRestriction != null && part2.PreferenceRestriction.Absence != null) return;
			
			var date1 = part1.DateOnly.DateTime.AddMinutes(part1.MinEndTimeMinute);
			var date2 = part2.DateOnly.DateTime.AddMinutes(part2.MaxStartTimeMinute);
			if (isUnavailableDay(part2)) return;
			
			if (date2 - date1 < nightlyRest)
			{
				part1.ViolatesNightlyRest = true;
				part2.ViolatesNightlyRest = true;
			}
		}

		private static bool isUnavailableDay(ValidatedSchedulePartDto partDto)
		{
			return partDto.MinStartTimeMinute == 0 && partDto.MaxStartTimeMinute == 0 && partDto.MinEndTimeMinute == 0 &&
			       partDto.MaxEndTimeMinute == 0 && partDto.MinWorkTimeInMinutes == 0 && partDto.MaxWorkTimeInMinutes == 0;
		}
	}
}