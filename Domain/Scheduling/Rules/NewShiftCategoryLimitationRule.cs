using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
    public class NewShiftCategoryLimitationRule : INewBusinessRule
    {
        private readonly IShiftCategoryLimitationChecker _limitationChecker;
        private readonly IVirtualSchedulePeriodExtractor _virtualSchedulePeriodExtractor;
        private bool _haltModify = true;

        public NewShiftCategoryLimitationRule(IShiftCategoryLimitationChecker limitationChecker, 
            IVirtualSchedulePeriodExtractor virtualSchedulePeriodExtractor)
        {
            _limitationChecker = limitationChecker;
            _virtualSchedulePeriodExtractor = virtualSchedulePeriodExtractor;
        }

        public string ErrorMessage { get { return ""; } }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public bool IsMandatory
        {
            get { return false; }
        }

        public bool HaltModify
        {
            get { return _haltModify; }
            set { _haltModify = value; }
        }

        public bool ForDelete { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Domain.Scheduling.Rules.NewShiftCategoryLimitationRule.createResponse(Teleopti.Interfaces.Domain.IPerson,Teleopti.Interfaces.Domain.DateOnly,System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public IEnumerable<IBusinessRuleResponse> Validate(IDictionary<IPerson, IScheduleRange> rangeClones, IEnumerable<IScheduleDay> scheduleDays)
        {
            var responseList = new HashSet<IBusinessRuleResponse>();

            var virtualSchedulePeriods =
                _virtualSchedulePeriodExtractor.CreateVirtualSchedulePeriodsFromScheduleDays(scheduleDays);

            foreach (IVirtualSchedulePeriod schedulePeriod in virtualSchedulePeriods)
            {
                if (schedulePeriod.IsValid)
                {
                    DateOnlyPeriod period = schedulePeriod.DateOnlyPeriod;
                    var person = schedulePeriod.Person;
                    IScheduleRange currentSchedules = rangeClones[person];
                    var oldResponses = currentSchedules.BusinessRuleResponseInternalCollection;
                    var oldResponseCount = oldResponses.Count();
                    foreach (var shiftCategoryLimitation in schedulePeriod.ShiftCategoryLimitationCollection())
                    {
                        if (!shiftCategoryLimitation.Weekly)
                        {
                            foreach (DateOnly day in period.DayCollection())
                            {
                                oldResponses.Remove(createResponse(person, day,"remove"));
                            }
                            IList<DateOnly> datesWithCategory;
                            if (_limitationChecker.IsShiftCategoryOverPeriodLimit(shiftCategoryLimitation, period, currentSchedules, out datesWithCategory))
                            {
                                string message = string.Format(TeleoptiPrincipal.CurrentPrincipal.Regional.Culture,
                                                    UserTexts.Resources.BusinessRuleShiftCategoryLimitationErrorMessage2,
                                                    shiftCategoryLimitation.ShiftCategory.Description.Name);
                                foreach (DateOnly dateOnly in datesWithCategory)
                                {
                                    if (!ForDelete)
                                        responseList.Add(createResponse(schedulePeriod.Person, dateOnly, message));
                                    oldResponses.Add(createResponse(schedulePeriod.Person, dateOnly, message));
                                }
                                
                            }
                        }
                    }
                    var newResponseCount = responseList.Count();
                    if (newResponseCount <= oldResponseCount)
                        responseList = new HashSet<IBusinessRuleResponse>();
                }
            }

            return responseList;
        }

        private IBusinessRuleResponse createResponse(IPerson person, DateOnly dateOnly, string message)
        {
            var dop = new DateOnlyPeriod(dateOnly, dateOnly);
            DateTimePeriod period = dop.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());
            IBusinessRuleResponse response = new BusinessRuleResponse(typeof(NewShiftCategoryLimitationRule), message, _haltModify, IsMandatory, period, person, dop)
                                                 {Overridden = !_haltModify};
            return response;
        }
    }
}
