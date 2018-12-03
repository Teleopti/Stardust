using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Util;

namespace Teleopti.Ccc.Domain.Scheduling.SaveSchedulePart
{
	public class SaveSchedulePartService : ISaveSchedulePartService
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(SaveSchedulePartService));

		private readonly IScheduleDifferenceSaver _scheduleDictionarySaver;
		private readonly IPersonAbsenceAccountRepository _personAbsenceAccountRepository;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
		
		public SaveSchedulePartService(IScheduleDifferenceSaver scheduleDictionarySaver,
			IPersonAbsenceAccountRepository personAbsenceAccountRepository, IScheduleDayChangeCallback scheduleDayChangeCallback)
		{
			_scheduleDictionarySaver = scheduleDictionarySaver;
			_personAbsenceAccountRepository = personAbsenceAccountRepository;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
		}

		public IList<string> Save(IScheduleDay scheduleDay, INewBusinessRuleCollection newBusinessRuleCollection,
			IScheduleTag scheduleTag)
		{
			return Save(new[] {scheduleDay}, newBusinessRuleCollection, scheduleTag);
		}

		public IList<string> Save(IEnumerable<IScheduleDay> scheduleDays, INewBusinessRuleCollection newBusinessRuleCollection,
			IScheduleTag scheduleTag)
		{
			var scheduleDay = scheduleDays.FirstOrDefault();
			openForEditsWhenReadOnlyDictionary(scheduleDay);
			var dic = scheduleDay.Owner;

			var checkResult = checkRules(dic, scheduleDays, newBusinessRuleCollection, scheduleTag);

			if (checkResult != null)
			{
				logger.Error($"Only logged for cancelling absence request used to reproduce bug#79030 PersonAbsenceRemover SaveSchedulePartService:{string.Join(",",scheduleDays.Select(a=>a.DateOnlyAsPeriod.DateOnly.ToString()).ToArray())}|{string.Join(",",checkResult.ToArray())}");
				return checkResult;
			}
			
			performSave(dic, scheduleDay);
			return null;
		}

		private static void openForEditsWhenReadOnlyDictionary(IScheduleDay scheduleDay)
		{
			var dic = scheduleDay.Owner as IReadOnlyScheduleDictionary;
			dic?.MakeEditable();
		}

		private IList<string> checkRules(IScheduleDictionary dic, IEnumerable<IScheduleDay> scheduleDays, INewBusinessRuleCollection newBusinessRuleCollection, IScheduleTag scheduleTag)
		{
			var invalidList = dic.Modify(ScheduleModifier.Scheduler, scheduleDays, newBusinessRuleCollection, _scheduleDayChangeCallback, new ScheduleTagSetter(scheduleTag)).ToList();
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