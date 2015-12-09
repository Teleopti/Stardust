using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public IList<string> Save(IScheduleDay scheduleDay, INewBusinessRuleCollection newBusinessRuleCollection, IScheduleTag scheduleTag, bool isNeedGetResult = false)
		{
			var dic = (IReadOnlyScheduleDictionary)scheduleDay.Owner;
			dic.MakeEditable();

			var checkResult = checkRules(dic, scheduleDay, newBusinessRuleCollection, scheduleTag, isNeedGetResult);
			if (checkResult != null)
			{
				return checkResult;
			}

			performSave(dic, scheduleDay);
			return null;
		}

		private IList<string> checkRules(IReadOnlyScheduleDictionary dic, IScheduleDay scheduleDay, INewBusinessRuleCollection newBusinessRuleCollection, IScheduleTag scheduleTag, bool isNeedGetResult)
		{
			var invalidList = dic.Modify(ScheduleModifier.Scheduler, scheduleDay, newBusinessRuleCollection, new ResourceCalculationOnlyScheduleDayChangeCallback(), new ScheduleTagSetter(scheduleTag)).ToList();
			if (invalidList != null && invalidList.Any(l => !l.Overridden))
			{
				if (isNeedGetResult) return invalidList.Select(i => i.Message).Distinct().ToArray();
				throwExceptionIfBrokenBusinessRule(invalidList);
			}

			return null;
		}

		private void performSave (IReadOnlyScheduleDictionary dic, IScheduleDay scheduleDay)
		{
			_scheduleDictionarySaver.SaveChanges (dic.DifferenceSinceSnapshot(), (IUnvalidatedScheduleRangeUpdate) dic[scheduleDay.Person]);
			_personAbsenceAccountRepository.AddRange (dic.ModifiedPersonAccounts);
		}

		private static void throwExceptionIfBrokenBusinessRule(IEnumerable<IBusinessRuleResponse> invalidList)
		{
			throw new BusinessRuleValidationException(
				string.Format(System.Globalization.CultureInfo.InvariantCulture,
					"At least one business rule was broken. Messages are: {0}{1}", Environment.NewLine,
					string.Join(Environment.NewLine,
						invalidList.Select(i => i.Message).Distinct().ToArray())));
		}
	}
}