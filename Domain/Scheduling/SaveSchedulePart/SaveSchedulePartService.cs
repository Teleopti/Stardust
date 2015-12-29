using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Scheduling.SaveSchedulePart
{
	public class SaveSchedulePartService : ISaveSchedulePartService
	{
		private readonly IScheduleDifferenceSaver _scheduleDictionarySaver;
		private readonly IPersonAbsenceAccountRepository _personAbsenceAccountRepository;

		public SaveSchedulePartService(IScheduleDifferenceSaver scheduleDictionarySaver, IPersonAbsenceAccountRepository personAbsenceAccountRepository)
		{
			_scheduleDictionarySaver = scheduleDictionarySaver;
			_personAbsenceAccountRepository = personAbsenceAccountRepository;
		}
		
		public IList<string> Save(IScheduleDay scheduleDay, INewBusinessRuleCollection newBusinessRuleCollection, IScheduleTag scheduleTag)
		{
			openForEditsWhenReadOnlyDictionary(scheduleDay);
			var dic = scheduleDay.Owner;

			var checkResult = checkRules(dic, scheduleDay, newBusinessRuleCollection, scheduleTag);
			if (checkResult != null)
			{
				return checkResult;
			}

			performSave(dic, scheduleDay);
			return null;
		}

		private static void openForEditsWhenReadOnlyDictionary(IScheduleDay scheduleDay)
		{
			var dic = scheduleDay.Owner as IReadOnlyScheduleDictionary;
			if (dic != null)
			{
				dic.MakeEditable();
			}
		}

		private IList<string> checkRules(IScheduleDictionary dic, IScheduleDay scheduleDay, INewBusinessRuleCollection newBusinessRuleCollection, IScheduleTag scheduleTag)
		{
			var invalidList = dic.Modify(ScheduleModifier.Scheduler, scheduleDay, newBusinessRuleCollection, new ResourceCalculationOnlyScheduleDayChangeCallback(), new ScheduleTagSetter(scheduleTag)).ToList();
			if (invalidList != null && invalidList.Any(l => !l.Overridden))
			{
				return invalidList.Select(i => i.Message).Distinct().ToArray();
			}

			return null;
		}

		private void performSave (IScheduleDictionary dic, IScheduleDay scheduleDay)
		{
			_scheduleDictionarySaver.SaveChanges (dic.DifferenceSinceSnapshot(), (IUnvalidatedScheduleRangeUpdate) dic[scheduleDay.Person]);
			_personAbsenceAccountRepository.AddRange (dic.ModifiedPersonAccounts);
		}
	}
}