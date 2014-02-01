using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
    public interface ISeniorityCalculatorForTeamBlock
    {
        IDictionary<ITeamBlockInfo, double> CreateWeekDayValueDictionary(IList<ITeamBlockInfo> teamBlocksToWorkWith, IDictionary<DayOfWeek, int> weekDayPoints);
    }

    public class SeniorityCalculatorForTeamBlock : ISeniorityCalculatorForTeamBlock
    {
        private readonly IWeekDayPointCalculatorForTeamBlock _weekDayPointCalculatorForTeamBlock;

        public SeniorityCalculatorForTeamBlock(IWeekDayPointCalculatorForTeamBlock weekDayPointCalculatorForTeamBlock)
        {
            _weekDayPointCalculatorForTeamBlock = weekDayPointCalculatorForTeamBlock;
        }

        public  IDictionary<ITeamBlockInfo, double> CreateWeekDayValueDictionary(IList<ITeamBlockInfo> teamBlocksToWorkWith, IDictionary<DayOfWeek, int> weekDayPoints)
        {
            var retDic = new Dictionary<ITeamBlockInfo, double>();
            foreach (var teamBlockInfo in teamBlocksToWorkWith)
            {
                var senorityValue = _weekDayPointCalculatorForTeamBlock.CalculateDaysOffSeniorityValue(teamBlockInfo,weekDayPoints);
                retDic.Add(teamBlockInfo, senorityValue);
            }

            return retDic;
        }
    }
}
