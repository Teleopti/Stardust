using System;
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
		public void Save(IScheduleDay scheduleDay, INewBusinessRuleCollection newBusinessRuleCollection, IScheduleTag scheduleTag)
		{
			performSave(scheduleDay, newBusinessRuleCollection, scheduleTag);
		}

		private void performSave (IScheduleDay scheduleDay, INewBusinessRuleCollection newBusinessRuleCollection, IScheduleTag scheduleTag)
		{
			var dic = (IReadOnlyScheduleDictionary) scheduleDay.Owner;
			dic.MakeEditable();

			throwExceptionIfBrokenBusinessRule (scheduleDay, newBusinessRuleCollection, scheduleTag, dic);
			_scheduleDictionarySaver.SaveChanges (dic.DifferenceSinceSnapshot(), (IUnvalidatedScheduleRangeUpdate) dic[scheduleDay.Person]);
			_personAbsenceAccountRepository.AddRange (dic.ModifiedPersonAccounts);
		}

		private static void throwExceptionIfBrokenBusinessRule (IScheduleDay scheduleDay, INewBusinessRuleCollection newBusinessRuleCollection, IScheduleTag scheduleTag, IReadOnlyScheduleDictionary dic)
		{
			var invalidList = dic.Modify (ScheduleModifier.Scheduler, scheduleDay, newBusinessRuleCollection, new ResourceCalculationOnlyScheduleDayChangeCallback(), new ScheduleTagSetter (scheduleTag)).ToList();
			if (invalidList != null && invalidList.Any (l => !l.Overridden))
			{
				throw new BusinessRuleValidationException(
					string.Format (System.Globalization.CultureInfo.InvariantCulture,
						"At least one business rule was broken. Messages are: {0}{1}", Environment.NewLine,
						string.Join (Environment.NewLine,
							invalidList.Select (i => i.Message).Distinct().ToArray())));
			}
		}
	}
}