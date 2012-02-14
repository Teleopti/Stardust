using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades
{

    public class ShiftTradeTargetTimeSpecification : ShiftTradeSpecification, IShiftTradeTargetTimeSpecification
    {
        private readonly ISchedulingResultStateHolder _stateHolder;
        private readonly ISchedulePeriodTargetTimeCalculator _targetTimeTimeCalculator;
        
        public override string DenyReason
        {
            get { return "ShiftTradeTargetTimeDenyReason"; }
        }

        public ShiftTradeTargetTimeSpecification(ISchedulingResultStateHolder stateHolder, ISchedulePeriodTargetTimeCalculator targetTimeTimeCalculator)
        {
            _stateHolder = stateHolder;
            _targetTimeTimeCalculator = targetTimeTimeCalculator;
        }

        public override bool IsSatisfiedBy(IList<IShiftTradeSwapDetail> obj)
        {
            IList<IScheduleDay> scheduleDays = new List<IScheduleDay>(obj.Count*2);

            // Här vill jag bara få en lista på alla inblandade scheduleDays så jag kan få fram min lista av schemaperioder (matrixes), gör det innan motbyten är tillagda
            foreach (var shiftTradeDetail in obj)
            {
                scheduleDays.Add(shiftTradeDetail.SchedulePartFrom);
                scheduleDays.Add(shiftTradeDetail.SchedulePartTo);
            }

            // skapa och fyll på med "motbytet" för alla change requests
            obj = createSwapDetails(obj);

            IList<IScheduleMatrixPro> matrixes = new ScheduleMatrixListCreator(_stateHolder).CreateMatrixListFromScheduleParts(scheduleDays);
            // Nu har jag alla inblandade schamaperioder. Om du och jag ska byta en vecka och din vecka är i en och samma period 
            /// och min vecka är i två olika perioder så får vi tre matriser
            foreach (var matrix in matrixes)
            {
                TimeSpan flexibility = matrix.SchedulePeriod.Person.WorkflowControlSet.ShiftTradeTargetTimeFlexibility;
                ITargetTimeFlexibilityValidator matrixValidator = new TargetTimeFlexibilityValidator(matrix, _targetTimeTimeCalculator);

                // Här ska jag göra en lista på föreslagna ändringar för varje matris, 
                // det är helt ok att föreslå ändringar som ligger utanför matrisen
                // det hanteras senare
                IList<IScheduleDay> suggestedChanges = new List<IScheduleDay>();
                foreach (var shiftTradeDetail in obj)
                {
                    // Om detaljens "byt till" inte påverkar dig så gå på nästa, du är inte med som motagare i detta change request
                    if (!shiftTradeDetail.SchedulePartTo.Person.Equals(matrix.Person))
                        continue;

                    // Kolla så att både "det jag har" och "byt till" är skift eller DO
                    if (shiftTradeDetail.SchedulePartFrom.SignificantPart() == SchedulePartView.MainShift ||
                        shiftTradeDetail.SchedulePartFrom.SignificantPart() == SchedulePartView.DayOff)
                    {
                        if (shiftTradeDetail.SchedulePartTo.SignificantPart() == SchedulePartView.MainShift ||
                            shiftTradeDetail.SchedulePartTo.SignificantPart() == SchedulePartView.DayOff)
                        {
                            // Vill att "det jag har" ska bli "byt till"
                            // alltså, sätt hans mainshift på min schemadag (för att en ev korttidsfrånvaro på mig ska kunna beaktas)
                            // Nu är det är sent och jag är virrig, kan bara hoppas detta blir rätt. Har man ett riktigt case så vet man och kan skriva
                            // ett vettigt test också
                            IScheduleDay changeFrom = (IScheduleDay)shiftTradeDetail.SchedulePartFrom.Clone();
                            IScheduleDay changeTo = (IScheduleDay)shiftTradeDetail.SchedulePartTo.Clone();

                            changeTo.Clear<IPersonAssignment>();
                            changeTo.Merge(changeFrom, false);

                            suggestedChanges.Add(changeTo);
                        }
                    }

                }
                bool result = matrixValidator.Validate(suggestedChanges, flexibility);
                if (!result)
                    return false;
            }

            return true;
        }

        private static IList<IShiftTradeSwapDetail> createSwapDetails(IEnumerable<IShiftTradeSwapDetail> originalList)
        {
            IList<IShiftTradeSwapDetail> ret = new List<IShiftTradeSwapDetail>();
            foreach (var currentDetail in originalList)
            {
                IShiftTradeSwapDetail newDetail = new ShiftTradeSwapDetail(currentDetail.PersonTo,
                                                                           currentDetail.PersonFrom,
                                                                           currentDetail.DateTo, 
                                                                           currentDetail.DateFrom);
                newDetail.SchedulePartFrom = (IScheduleDay) currentDetail.SchedulePartTo.Clone();
                newDetail.SchedulePartTo = (IScheduleDay) currentDetail.SchedulePartFrom.Clone();
                ret.Add(currentDetail);
                ret.Add(newDetail);
            }

            return ret;
        }
    }
}
