using System.Collections.Generic;
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

		public ScheduleDayDifferenceSaver(ISkillCombinationResourceRepository skillCombinationResourceRepository, CompareProjection compareProjection)
		{
			_skillCombinationResourceRepository = skillCombinationResourceRepository;
			_compareProjection = compareProjection;
		}

		public void SaveDifferences(IScheduleDictionary dic, IPerson person, DateOnlyPeriod dateOnlyPeriod)
		{
			var snapshot = ((ScheduleRange) dic[person]).Snapshot;
			var scheduleDaysAfter = dic[person];
			var skillCombinationResourceDeltas = new List<SkillCombinationResource>();
			foreach (var snapShotDay in snapshot.ScheduledDayCollection(dateOnlyPeriod))
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