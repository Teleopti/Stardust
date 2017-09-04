﻿using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Staffing
{
	public interface IScheduleDayDifferenceSaver
	{
		IEnumerable<SkillCombinationResource> SaveDifferences(IScheduleRange scheduleRange);
		IEnumerable<SkillCombinationResource> GetDifferences(IScheduleRange scheduleRange);
	}

	public class ScheduleDayDifferenceSaver : IScheduleDayDifferenceSaver
	{
		private readonly ISkillCombinationResourceRepository _skillCombinationResourceRepository;
		private readonly CompareProjection _compareProjection;
		private readonly INow _now;
		private readonly IStaffingSettingsReader _staffingSettingsReader;

		public ScheduleDayDifferenceSaver(ISkillCombinationResourceRepository skillCombinationResourceRepository, CompareProjection compareProjection, 
			INow now, IStaffingSettingsReader staffingSettingsReader)
		{
			_skillCombinationResourceRepository = skillCombinationResourceRepository;
			_compareProjection = compareProjection;
			_now = now;
			_staffingSettingsReader = staffingSettingsReader;
		}

		public IEnumerable<SkillCombinationResource> SaveDifferences(IScheduleRange scheduleRange)
		{
			var skillCombinationResourceDeltas = GetDifferences(scheduleRange).ToList();
			_skillCombinationResourceRepository.PersistChanges(skillCombinationResourceDeltas);
			return skillCombinationResourceDeltas;
		}

		public IEnumerable<SkillCombinationResource> GetDifferences(IScheduleRange scheduleRange)
		{
			var snapshot = ((ScheduleRange)scheduleRange).Snapshot;
			var skillCombinationResourceDeltas = new List<SkillCombinationResource>();
			var staffingReadModelNumberOfDays = _staffingSettingsReader.GetIntSetting("StaffingReadModelNumberOfDays", 14);
			var staffingReadModelHistoricalHours = _staffingSettingsReader.GetIntSetting("StaffingReadModelHistoricalHours", 8*24);
			var readModelPeriod = new DateTimePeriod(_now.UtcDateTime().AddHours(-staffingReadModelHistoricalHours), _now.UtcDateTime().AddDays(staffingReadModelNumberOfDays).AddHours(1));
			foreach (var snapShotDay in snapshot.ScheduledDayCollection(scheduleRange.Period.ToDateOnlyPeriod(scheduleRange.Person.PermissionInformation.DefaultTimeZone()).Inflate(1)).Where(x => readModelPeriod.Contains(x.DateOnlyAsPeriod.DateOnly.Date)))  //inflate to handle midnight shift
			{
				skillCombinationResourceDeltas.AddRange(_compareProjection.Compare(snapShotDay, scheduleRange.ScheduledDay(snapShotDay.DateOnlyAsPeriod.DateOnly)));
			}
			return skillCombinationResourceDeltas;
		}
	}

	public class EmptyScheduleDayDifferenceSaver : IScheduleDayDifferenceSaver
	{
		public IEnumerable<SkillCombinationResource> SaveDifferences(IScheduleRange scheduleRange)
		{
			return new List<SkillCombinationResource>(); //do nothing
		}

		public IEnumerable<SkillCombinationResource> GetDifferences(IScheduleRange scheduleRange)
		{
			return new List<SkillCombinationResource>();
		}
	}
}