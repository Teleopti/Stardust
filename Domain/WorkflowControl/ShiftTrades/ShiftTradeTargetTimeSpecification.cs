using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades
{
    public class ShiftTradeTargetTimeSpecification : ShiftTradeSpecification
    {
		private readonly IMatrixListFactory _scheduleMatrixListCreator;
	    private readonly ISchedulePeriodTargetTimeCalculator _targetTimeTimeCalculator;
        
        public override string DenyReason
        {
            get { return "ShiftTradeTargetTimeDenyReason"; }
        }

		public ShiftTradeTargetTimeSpecification(IMatrixListFactory scheduleMatrixListCreator, ISchedulePeriodTargetTimeCalculator targetTimeTimeCalculator)
        {
			_scheduleMatrixListCreator = scheduleMatrixListCreator;
			_targetTimeTimeCalculator = targetTimeTimeCalculator;
        }

		  public override bool IsSatisfiedBy(IEnumerable<IShiftTradeSwapDetail> obj)
        {
            IList<IScheduleDay> scheduleDays = new List<IScheduleDay>(obj.Count()*2);

            // Här vill jag bara få en lista på alla inblandade scheduleDays så jag kan få fram min lista av schemaperioder (matrixes), gör det innan motbyten är tillagda
			//Here I just want to have a list of all scheduleDays, so I can get my list of scheduled periods, do this before the other trade is added.
            foreach (var shiftTradeDetail in obj)
            {
                scheduleDays.Add(shiftTradeDetail.SchedulePartFrom);
                scheduleDays.Add(shiftTradeDetail.SchedulePartTo);
            }

            // skapa och fyll på med "motbytet" för alla change requests
			  // Create and add the other trade for all change requests
            obj = createSwapDetails(obj);

            IList<IScheduleMatrixPro> matrixes = _scheduleMatrixListCreator.CreateMatrixListForSelection(scheduleDays);
            // Nu har jag alla inblandade schamaperioder. Om du och jag ska byta en vecka och din vecka är i en och samma period 
            /// och min vecka är i två olika perioder så får vi tre matriser
            /// Now I have all relevant scheduled periods. If you and me are supposed to trade one week and your week is the same schedulePeriod and my week is in two different shedulePeriods, then we get three matrixes.
            foreach (var matrix in matrixes)
            {
                TimeSpan flexibility = matrix.SchedulePeriod.Person.WorkflowControlSet.ShiftTradeTargetTimeFlexibility;
                ITargetTimeFlexibilityValidator matrixValidator = new TargetTimeFlexibilityValidator(matrix, _targetTimeTimeCalculator);

                // Här ska jag göra en lista på föreslagna ändringar för varje matris, 
                // det är helt ok att föreslå ändringar som ligger utanför matrisen
                // det hanteras senare
				// Here I'm supposed to make a list of suggested changes for each matrix,
				// It's OK to suggest changes outside of matrix
				// this will be handled later.
                IList<IScheduleDay> suggestedChanges = new List<IScheduleDay>();
                foreach (var shiftTradeDetail in obj)
                {
                    // Om detaljens "byt till" inte påverkar dig så gå på nästa, du är inte med som motagare i detta change request
					// If the detail 'trade with' don't affect you, go to next, you're not a receiver of this change request
                    if (!shiftTradeDetail.SchedulePartTo.Person.Equals(matrix.Person))
                        continue;

                    // Kolla så att både "det jag har" och "byt till" är skift eller DO
					//Check so that "What I have" and "trade with" is a shift or DO
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
							/*Want "What I have" should be become "trade with"
							 that means, put his main shift on my schedule day(to make sure that short term absence can be handled)*/
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

        private static IEnumerable<IShiftTradeSwapDetail> createSwapDetails(IEnumerable<IShiftTradeSwapDetail> originalList)
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
