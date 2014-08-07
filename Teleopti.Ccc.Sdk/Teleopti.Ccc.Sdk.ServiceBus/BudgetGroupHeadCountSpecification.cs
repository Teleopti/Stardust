using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;
using log4net;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "HeadCount")]
    public class BudgetGroupHeadCountSpecification : PersonRequestSpecification<IAbsenceRequest>, IBudgetGroupHeadCountSpecification
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (BudgetGroupHeadCountSpecification));

        private readonly IScenarioRepository _scenarioRepository;
        private readonly IBudgetDayRepository _budgetDayRepository;
        private readonly IScheduleProjectionReadOnlyRepository _scheduleProjectionReadOnlyRepository;

        public BudgetGroupHeadCountSpecification(IScenarioRepository scenarioRepository,
                                                 IBudgetDayRepository budgetDayRepository,
                                                 IScheduleProjectionReadOnlyRepository
                                                     scheduleProjectionReadOnlyRepository)
        {
            _scenarioRepository = scenarioRepository;
            _budgetDayRepository = budgetDayRepository;
            _scheduleProjectionReadOnlyRepository = scheduleProjectionReadOnlyRepository;
        }

        public override IValidatedRequest IsSatisfied(IAbsenceRequest absenceRequest)
        {
            var timeZone = absenceRequest.Person.PermissionInformation.DefaultTimeZone();
            var culture = absenceRequest.Person.PermissionInformation.Culture();
            var requestedPeriod = absenceRequest.Period.ToDateOnlyPeriod(timeZone);
            var personPeriod = absenceRequest.Person.PersonPeriods(requestedPeriod).FirstOrDefault();
            string invalidConfig;
            var invalidDays = string.Empty;
            var validatedRequest = new ValidatedRequest {IsValid = true, ValidationErrors = String.Empty};
            string notEnoughAllowance = UserTexts.Resources.ResourceManager.GetString("NotEnoughAllowance", culture);

            if (personPeriod == null || personPeriod.BudgetGroup == null)
            {
								Logger.DebugFormat("There is no budget group for you: {0}", absenceRequest.Person.Id);
								invalidConfig = UserTexts.Resources.ResourceManager.GetString("BudgetGroupMissing", culture); 
                validatedRequest.IsValid = false;
                validatedRequest.ValidationErrors = invalidConfig;
                return validatedRequest;
            }

            var budgetGroup = personPeriod.BudgetGroup;
            var defaultScenario = _scenarioRepository.LoadDefaultScenario();
            var budgetDays = _budgetDayRepository.Find(defaultScenario, budgetGroup, requestedPeriod);

            if (budgetDays == null)
            {
                Logger.DebugFormat("There is no budget for this period {0}.", requestedPeriod);
                invalidConfig = string.Format(culture, UserTexts.Resources.NoBudgetForThisPeriod, requestedPeriod);
                validatedRequest.IsValid = false;
                validatedRequest.ValidationErrors = invalidConfig;
                return validatedRequest;
            }

            if (budgetDays.Count != requestedPeriod.DayCollection().Count)
            {
                Logger.DebugFormat("One or more days during this requested period {0} has no budget.", requestedPeriod);
                invalidConfig = string.Format(culture, UserTexts.Resources.NoBudgetDefineForSomeRequestedDays, requestedPeriod);
                validatedRequest.IsValid = false;
                validatedRequest.ValidationErrors = invalidConfig;
                return validatedRequest;
            }

            invalidDays = getInvalidDaysIfExist(budgetDays, budgetGroup, invalidDays, culture);

            if (!string.IsNullOrEmpty(invalidDays))
            {
                invalidDays = invalidDays.Substring(0, invalidDays.Length - 1);
                invalidConfig = notEnoughAllowance + invalidDays;

                if (!String.IsNullOrEmpty(invalidConfig))
                    return new ValidatedRequest { IsValid = false, ValidationErrors = invalidConfig };
            }

            return validatedRequest; 
        }

        private string getInvalidDaysIfExist(IEnumerable<IBudgetDay> budgetDays, IBudgetGroup budgetGroup, string invalidDays, CultureInfo culture)
        {
            var count = 0;
            foreach (var budgetDay in budgetDays.OrderBy(x => x.Day))
            {
                var currentDay = budgetDay.Day;

                if (budgetDay.IsClosed)
                    continue;

                var allowance = budgetDay.Allowance;
                var alreadyUsedAllowance =
                    _scheduleProjectionReadOnlyRepository.GetNumberOfAbsencesPerDayAndBudgetGroup(
                        budgetGroup.Id.GetValueOrDefault(), currentDay);

                if (Math.Floor(allowance) <= alreadyUsedAllowance)
                {
                    count++;
                    if (count > 5)
                        break;
                    Logger.DebugFormat("There is not enough allowance for day {0}.", budgetDay.Day);
                    invalidDays += budgetDay.Day.ToShortDateString(culture) + ",";
                }
            }
            return invalidDays;
        }
    }
}
