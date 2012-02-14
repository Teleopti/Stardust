using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
    public class NewLegalStateRule : INewBusinessRule
    {
        private readonly IWorkShiftMinMaxCalculator _workShiftMinMaxCalculator;
        private readonly IScheduleMatrixListCreator _scheduleMatrixListCreator;
        private bool _haltModify = true;
        public bool ForDelete { get; set; }

        public NewLegalStateRule(IScheduleMatrixListCreator scheduleMatrixListCreator, 
            IWorkShiftMinMaxCalculator workShiftMinMaxCalculator)
        {
            _scheduleMatrixListCreator = scheduleMatrixListCreator;
            _workShiftMinMaxCalculator = workShiftMinMaxCalculator;
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public IEnumerable<IBusinessRuleResponse> Validate(IDictionary<IPerson, IScheduleRange> rangeClones, IEnumerable<IScheduleDay> scheduleDays)
        {
            var responseList = new HashSet<IBusinessRuleResponse>();
            var matrixList =
                _scheduleMatrixListCreator.CreateMatrixListFromSchedulePartsAndAlternativeScheduleRanges(rangeClones,
                                                                                                         scheduleDays);
            foreach (IScheduleMatrixPro matrix in matrixList)
            {
                IScheduleRange currentSchedules = rangeClones[matrix.Person];
                var oldResponses = currentSchedules.BusinessRuleResponseInternalCollection;
                //var oldResponseFound = false;
                foreach (var scheduleDayPro in matrix.EffectivePeriodDays)
                {
                    var response = createResponse(matrix.Person, scheduleDayPro.Day, string.Empty,
                                                  typeof(NewLegalStateRule));

                    oldResponses.Remove(response);
                }

                if (!_workShiftMinMaxCalculator.IsPeriodInLegalState(matrix))
                {
                    foreach (var scheduleDayPro in matrix.EffectivePeriodDays)
                    {
                        var response = createResponse(matrix.Person, scheduleDayPro.Day,
                                                      UserTexts.Resources.PeriodNotInLegalState,
                                                      typeof (NewLegalStateRule));
                        //Always override automatically
                        response.Overridden = true;

                        if(!ForDelete)
                            responseList.Add(response);

                        oldResponses.Add(response);
                    }
                }
            }

            return responseList;
        }

        private IBusinessRuleResponse createResponse(IPerson person, DateOnly dateOnly, string message, Type type)
        {
            var dop = new DateOnlyPeriod(dateOnly, dateOnly);
            var period = dop.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());
            var dateOnlyPeriod = new DateOnlyPeriod(dateOnly, dateOnly);
            IBusinessRuleResponse response = new BusinessRuleResponse(type, message, _haltModify, IsMandatory, period, person, dateOnlyPeriod) { Overridden = !_haltModify };
            return response;
        }
    }
}
