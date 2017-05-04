using System.Collections.Generic;
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
		void SaveDifferences(IScheduleDictionary dic, IPerson person, DateOnlyPeriod dateOnlyPeriod);
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

		public void SaveDifferences(IScheduleDictionary dic, IPerson person, DateOnlyPeriod dateOnlyPeriod)
		{
			var snapshot = ((ScheduleRange) dic[person]).Snapshot;
			var scheduleDaysAfter = dic[person];
			var skillCombinationResourceDeltas = new List<SkillCombinationResource>();
			var readModelPeriod = new DateTimePeriod(_now.UtcDateTime().AddDays(-1).AddHours(-1), _now.UtcDateTime().AddDays(_staffingSettingsReader.GetIntSetting("StaffingReadModelNumberOfDays", 14)).AddHours(1));
			foreach (var snapShotDay in snapshot.ScheduledDayCollection(dateOnlyPeriod.Inflate(1)).Where(x => readModelPeriod.Contains(x.DateOnlyAsPeriod.DateOnly.Date)))  //inflate to handle midnight shift
			{
				skillCombinationResourceDeltas.AddRange(_compareProjection.Compare(snapShotDay, scheduleDaysAfter.ScheduledDay(snapShotDay.DateOnlyAsPeriod.DateOnly)));
			}
			_skillCombinationResourceRepository.PersistChanges(skillCombinationResourceDeltas);
		}
	}

	public class EmptyScheduleDayDifferenceSaver : IScheduleDayDifferenceSaver
	{
		public void SaveDifferences(IScheduleDictionary dic, IPerson person, DateOnlyPeriod dateOnlyPeriod)
		{
			//do nothing
		}
	}

}