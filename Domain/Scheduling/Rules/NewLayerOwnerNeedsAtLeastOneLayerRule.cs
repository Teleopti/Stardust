using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
    public class NewLayerOwnerNeedsAtLeastOneLayerRule : INewBusinessRule
    {
        public string ErrorMessage
        {
            get { return ""; }
        }

        public bool IsMandatory
        {
            get { return true; }
        }

        public bool HaltModify
        {
            get { return true; }
            set { }
        }

        public bool ForDelete { get; set; }

        public IEnumerable<IBusinessRuleResponse> Validate(IDictionary<IPerson, IScheduleRange> rangeClones, IEnumerable<IScheduleDay> scheduleDays)
        {
            var responseList = new HashSet<IBusinessRuleResponse>();
            foreach (IScheduleDay day in scheduleDays)
            {
                var person = day.Person;
                foreach (var ass in day.PersonAssignmentCollection())
                {
                    if (!isMainShiftOk(ass.MainShift, responseList, person) ||
                            !isPersonalShiftsOk(ass.PersonalShiftCollection, responseList, person) ||
                            !isOvertimeShiftsOk(ass.OvertimeShiftCollection, responseList, person))
                        return responseList;
                }
            }
            return responseList;
        }

        private bool isOvertimeShiftsOk(IEnumerable<IOvertimeShift> overtimeShifts, HashSet<IBusinessRuleResponse> responseList, IPerson person)
        {
            foreach (var overtimeShift in overtimeShifts)
            {
                if (!hasLayerOwnerAtLeastOneLayer(overtimeShift, responseList, person)) return false;
            }
            return true;
        }

        private bool isPersonalShiftsOk(IEnumerable<IPersonalShift> personalShifts, HashSet<IBusinessRuleResponse> responseList, IPerson person)
        {
            foreach (var personalShift in personalShifts)
            {
                if (!hasLayerOwnerAtLeastOneLayer(personalShift, responseList, person)) return false;
            }
            return true;
        }

        private bool isMainShiftOk(IMainShift mainShift, HashSet<IBusinessRuleResponse> responseList, IPerson person)
        {
            return mainShift == null || hasLayerOwnerAtLeastOneLayer(mainShift, responseList, person);
        }

        private bool hasLayerOwnerAtLeastOneLayer(ILayerCollectionOwner<IActivity> layerOwner, HashSet<IBusinessRuleResponse> responseList, IPerson person)
        {
            if (layerOwner.LayerCollection.Count == 0)
            {
                var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(), new DateOnly());
                responseList.Add(new BusinessRuleResponse(typeof(NewLayerOwnerNeedsAtLeastOneLayerRule), string.Format(CultureInfo.CurrentUICulture, Resources.LayerOwnerHasNoLayers, layerOwner), HaltModify, IsMandatory, new DateTimePeriod(), person, dateOnlyPeriod) { Overridden = !HaltModify });
                return false;
            }
            return true;
        }
    }
}
