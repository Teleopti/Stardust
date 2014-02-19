using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
{
    public interface IDayOffStep1
    {
        void PerformStep1(ISchedulingOptions schedulingOptions, IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons);
    }

    public class DayOffStep1 : IDayOffStep1
    {
        private readonly IConstructTeamBlock _constructTeamBlock;
        private readonly IFilterForTeamBlockInSelection _filterForTeamBlockInSelection;
        private readonly IFilterForTeamBlockInSelection _filterForFullyScheduledBlocks;
        private readonly IDetermineTeamBlockWeekDayPriority _determineTeamBlockWeekDayPriority;
        private readonly IFilterForMultipleMatrixPerAgentInTeamBlock _filterForMultipleMatrixPerAgentInTeamBlock;
        private ISeniorityExtractor  _seniorityExtractor;
        private IFilterForSameTeamBlock  _filterForSameTeamBlock;
        private IWeekDayPointExtractor  _weekDayPointExtractor;

        public DayOffStep1( IConstructTeamBlock constructTeamBlock, IFilterForTeamBlockInSelection filterForTeamBlockInSelection, IFilterForTeamBlockInSelection filterForFullyScheduledBlocks, IDetermineTeamBlockWeekDayPriority determineTeamBlockWeekDayPriority, IFilterForMultipleMatrixPerAgentInTeamBlock filterForMultipleMatrixPerAgentInTeamBlock)
        {
            _constructTeamBlock = constructTeamBlock;
            _filterForTeamBlockInSelection = filterForTeamBlockInSelection;
            _filterForFullyScheduledBlocks = filterForFullyScheduledBlocks;
            _determineTeamBlockWeekDayPriority = determineTeamBlockWeekDayPriority;
            _filterForMultipleMatrixPerAgentInTeamBlock = filterForMultipleMatrixPerAgentInTeamBlock;
        }

        public void PerformStep1(ISchedulingOptions schedulingOptions, IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons)
        {
            var listOfAllTeamBlock = stepAConstructTeamBlock(schedulingOptions, allPersonMatrixList, selectedPeriod, selectedPersons);
            listOfAllTeamBlock = stepBFilterOutUnwantedBlocks(listOfAllTeamBlock,selectedPersons,selectedPeriod);

            var calcualtedTeamBlocks =  setpCCalcaulateScneriotyAndPoints(listOfAllTeamBlock);

            setpDAnalyzeTeamBlockForPossibleSwap(calcualtedTeamBlocks);

            //return the new result in the form of matrixes or?
        }

        public void PerformStep1SecondWay(ISchedulingOptions schedulingOptions, IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons)
        {
            var listOfAllTeamBlock = stepAConstructTeamBlock(schedulingOptions, allPersonMatrixList, selectedPeriod, selectedPersons);
            listOfAllTeamBlock = stepBFilterOutUnwantedBlocks(listOfAllTeamBlock, selectedPersons, selectedPeriod);

            var teamBlockPoints = _seniorityExtractor.ExtractSeniority(listOfAllTeamBlock).ToList();
            foreach (var teamBlockPoint in teamBlockPoints.OrderByDescending(s => s.Points))
            {
                var roleModelTeamBlock = teamBlockPoint.TeamBlockInfo;
                var roleModelWeekDayPoints = _weekDayPointExtractor.ExtractWeekDayPointForTeamBlock(roleModelTeamBlock);
                var filteredTeamBlockList = _filterForSameTeamBlock.Filter(roleModelTeamBlock, teamBlockPoints);
                var weekDayPoints = _weekDayPointExtractor.ExtractWeekDayInfos(filteredTeamBlockList.Select(s=>s.TeamBlockInfo ));
                foreach (var targetWeekDayPoint in weekDayPoints.OrderByDescending(s => s.Points))
                {
                    var targetBlock = targetWeekDayPoint.TeamBlockInfo;
                    if (targetWeekDayPoint.Points <= roleModelWeekDayPoints)
                        continue;
                    //do the swap and move to the next least block
                }
            }
            
            var calcualtedTeamBlocks = setpCCalcaulateScneriotyAndPoints(listOfAllTeamBlock);

            setpDAnalyzeTeamBlockForPossibleSwap(calcualtedTeamBlocks);

            //return the new result in the form of matrixes or?
        }

        private void setpDAnalyzeTeamBlockForPossibleSwap(ITeamBlockWeekDaySeniorityCalculator calcualtedTeamBlocks)
        {
            foreach (var teamBlock in calcualtedTeamBlocks.HighToLowSeniorityListBlockInfo)
            {
                //if (!_teamBlockMatrixValidator.Validate(teamBlock)) continue;
                foreach (var targetTeamBlock in calcualtedTeamBlocks.ExtractAppropiateTeamBlock(teamBlock))
                {
                    //	if (_cancelMe) break;
                    //	if (_teamBlockSwapValidator.ValidateCanSwap(teamBlock, targetTeamBlock))
                    //	{
                    //		//perform swap
                    //		break;
                    //	}

                }
                //if (_cancelMe) break;

                //var message = Resources.FairnessOptimizationOn + " " + Resources.Seniority + ": " + new Percent(analyzedTeamBlocks.Count / totalBlockCount);
                //OnReportProgress(message);
                //analyzedTeamBlocks.Add(teamBlockInfoHighSeniority);

            }
        }

        private ITeamBlockWeekDaySeniorityCalculator setpCCalcaulateScneriotyAndPoints(IList<ITeamBlockInfo> listOfAllTeamBlock)
        {
            return _determineTeamBlockWeekDayPriority.PerformTeamBlockCalculation(listOfAllTeamBlock);
        }

        private IList<ITeamBlockInfo> stepBFilterOutUnwantedBlocks(IList<ITeamBlockInfo> listOfAllTeamBlock, IList<IPerson> selectedPersons, DateOnlyPeriod selectedPeriod)
        {
            var filteredList =  _filterForTeamBlockInSelection.Filter(listOfAllTeamBlock, selectedPersons, selectedPeriod);
            filteredList = _filterForFullyScheduledBlocks.Filter(filteredList, selectedPersons, selectedPeriod);
            filteredList = _filterForMultipleMatrixPerAgentInTeamBlock.Filter(filteredList);
            return filteredList;
        }

        private IList<ITeamBlockInfo> stepAConstructTeamBlock(ISchedulingOptions schedulingOptions, IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons)
        {

            return _constructTeamBlock.Construct(allPersonMatrixList, selectedPeriod, selectedPersons, true,
			                                                  BlockFinderType.SchedulePeriod,
			                                                  schedulingOptions.GroupOnGroupPageForTeamBlockPer);

        }

    }
}
