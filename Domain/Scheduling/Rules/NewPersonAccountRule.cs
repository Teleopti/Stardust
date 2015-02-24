using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
    public class NewPersonAccountRule : INewBusinessRule
    {
        private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
        private readonly IDictionary<IPerson, IPersonAccountCollection> _allAccounts;
        private bool _haltModify = true;
        private bool _forDelete;
		private static readonly object _modifiedAccountLock = new object();

        public NewPersonAccountRule(ISchedulingResultStateHolder schedulingResultStateHolder, IDictionary<IPerson, IPersonAccountCollection> allAccounts)
        {
            _schedulingResultStateHolder = schedulingResultStateHolder;
            _allAccounts = allAccounts;
        }

        public string ErrorMessage
        {
            get { return string.Empty; }
        }

        public bool IsMandatory
        {
            get { return false; }
        }

        public bool HaltModify
        {
            get { return _haltModify; }
            set { _haltModify = value; }
        }

        public bool ForDelete
        {
            get { return _forDelete; }
            set { _forDelete = value; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public IEnumerable<IBusinessRuleResponse> Validate(IDictionary<IPerson, IScheduleRange> rangeClones, IEnumerable<IScheduleDay> scheduleDays)
        {
            var responseList = new HashSet<IBusinessRuleResponse>();

            if (otherScenarioThanDefault())
            {
                return responseList;
            }

            foreach (var rangeCloneValueKey in rangeClones)
            {
                IPersonAccountCollection myAccounts;
                if (!_allAccounts.TryGetValue(rangeCloneValueKey.Key, out myAccounts))
                    continue;

                var affectedAccounts = new HashSet<IAccount>();
                foreach (var scheduleDay in scheduleDays)
                {
                    if(scheduleDay.Person == rangeCloneValueKey.Key)
                    {
                        DateOnly dateOnly = scheduleDay.DateOnlyAsPeriod.DateOnly;
                        IEnumerable<IAccount> accounts =  myAccounts.Find(dateOnly);
                        foreach (var account in accounts)
                        {
                            affectedAccounts.Add(account);
                        }
                    }
                }

	            var timeZone = rangeCloneValueKey.Value.Person.PermissionInformation.DefaultTimeZone();
                foreach (var affectedAccount in affectedAccounts)
                {
                    DateOnlyPeriod rangePeriod = rangeCloneValueKey.Value.Period.ToDateOnlyPeriod(timeZone);
                    DateOnlyPeriod? intersection = affectedAccount.Period().Intersection(rangePeriod);
                    if (!intersection.HasValue)
                        continue;

                	var lastRemaining = affectedAccount.Remaining;
                    IList<IScheduleDay> scheduleDaysForAccount = new List<IScheduleDay>(rangeCloneValueKey.Value.ScheduledDayCollection(intersection.Value));
                    affectedAccount.Owner.Absence.Tracker.Track(affectedAccount, affectedAccount.Owner.Absence,
                                                                              scheduleDaysForAccount);

					if (lastRemaining != affectedAccount.Remaining)
					{
						//Tell someone this account is dirty
						lock (_modifiedAccountLock)
							_schedulingResultStateHolder.Schedules.ModifiedPersonAccounts.Add((IPersonAbsenceAccount) affectedAccount.Root());
					}

                	var oldResponses = rangeCloneValueKey.Value.BusinessRuleResponseInternalCollection;
                    foreach (var scheduleDay in scheduleDaysForAccount)
                    {
                        oldResponses.Remove(createResponse(scheduleDay.Person, scheduleDay.DateOnlyAsPeriod, string.Empty, typeof(NewPersonAccountRule)));
                    }
                    if(affectedAccount.IsExceeded)
                    {
                        foreach (var scheduleDay in scheduleDaysForAccount)
                        {
                            string message = string.Format(TeleoptiPrincipal.Current.Regional.Culture,
                                                    UserTexts.Resources.BusinessRulePersonAccountError1,
                                                    affectedAccount.Owner.Absence.Description.Name, affectedAccount.Period().StartDate.ToShortDateString());

                            if (!ForDelete)
                                responseList.Add(createResponse(scheduleDay.Person, scheduleDay.DateOnlyAsPeriod, message, typeof(NewPersonAccountRule)));
                        }
                    }
                    
                }
                foreach (var businessRuleResponse in responseList)
                {
					if (rangeCloneValueKey.Key.Equals(businessRuleResponse.Person))
						rangeCloneValueKey.Value.BusinessRuleResponseInternalCollection.Add(businessRuleResponse);
                }
            }

            return responseList;
        }

        private bool otherScenarioThanDefault()
        {
            return !_schedulingResultStateHolder.Schedules.Scenario.DefaultScenario;
        }

        private IBusinessRuleResponse createResponse(IPerson person, IDateOnlyAsDateTimePeriod dateOnly, string message, Type type)
        {
            var dop = new DateOnlyPeriod(dateOnly.DateOnly, dateOnly.DateOnly);
            IBusinessRuleResponse response = new BusinessRuleResponse(type, message, _haltModify, IsMandatory, dateOnly.Period(), person, dop) { Overridden = !_haltModify };
            return response;
        }
    }
}