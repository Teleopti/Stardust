using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;

namespace Teleopti.Ccc.Domain.Scheduling.SaveSchedulePart
{
	public class BusinessRulesForPersonalAccountUpdate : IBusinessRulesForPersonalAccountUpdate
	{
		private readonly IPersonAbsenceAccountRepository _personAbsenceAccountRepository;
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;

		public BusinessRulesForPersonalAccountUpdate(IPersonAbsenceAccountRepository personAbsenceAccountRepository, ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			_personAbsenceAccountRepository = personAbsenceAccountRepository;
			_schedulingResultStateHolder = schedulingResultStateHolder;
		}

		public INewBusinessRuleCollection FromScheduleRange(IScheduleRange scheduleRange)
		{
			var personAccounts = _personAbsenceAccountRepository.FindByUsers(new Collection<IPerson> {scheduleRange.Person});
			var rules = NewBusinessRuleCollection.MinimumAndPersonAccount(_schedulingResultStateHolder, personAccounts);
			rules.DoNotHaltModify(typeof(NewPersonAccountRule));
			((IValidateScheduleRange) scheduleRange).ValidateBusinessRules(rules);
			return rules;
		}
		
		public INewBusinessRuleCollection FromScheduleDictionary(IDictionary<IPerson, IPersonAccountCollection> personAbsenceAccounts, IScheduleDictionary scheduleDictionary)
		{
			var rules = NewBusinessRuleCollection.MinimumAndPersonAccount(scheduleDictionary, personAbsenceAccounts);
			rules.DoNotHaltModify(typeof(NewPersonAccountRule));

			foreach (var scheduleRange in scheduleDictionary.Values)
				((IValidateScheduleRange) scheduleRange).ValidateBusinessRules(rules);
			
			return rules;
		}
	}
}
