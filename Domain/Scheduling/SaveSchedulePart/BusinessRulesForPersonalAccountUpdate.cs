using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Interfaces.Domain;

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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public INewBusinessRuleCollection FromScheduleRange(IScheduleRange scheduleRange)
		{
			_schedulingResultStateHolder.Schedules = scheduleRange.Owner;
			_schedulingResultStateHolder.PersonsInOrganization = new Collection<IPerson> { scheduleRange.Person };
			_schedulingResultStateHolder.AllPersonAccounts =
				_personAbsenceAccountRepository.FindByUsers(_schedulingResultStateHolder.PersonsInOrganization);

			var rules = NewBusinessRuleCollection.MinimumAndPersonAccount(_schedulingResultStateHolder);
			rules.Remove(typeof(NewPersonAccountRule));
			//Stop this rule from hindering a save... This will make sure that we update personal accounts.

			((IValidateScheduleRange)scheduleRange).ValidateBusinessRules(rules);
			return rules;
		}
	}

	
}
