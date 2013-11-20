using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization
{
    public interface ITeamBlockPriorityDefinitionInfo
    {
        IEnumerable<int> HighToLowAgentPriorityList { get; }
        IEnumerable<int> HighToLowShiftCategoryPriorityList { get; }
        IEnumerable<int> LowToHighShiftCategoryPriorityList { get; }
        IEnumerable<int> LowToHighAgentPriorityList { get; }
        void Clear();
        ITeamBlockInfo BlockOnAgentPriority(int priority);
        int GetShiftCategoryPriorityOfBlock(ITeamBlockInfo teamBlockInfo);
    }

    public class TeamBlockPriorityDefinitionInfo : ITeamBlockPriorityDefinitionInfo
    {
        private readonly IDictionary<ITeamBlockInfo, PriorityDefinition> _tbPriorityDefinition;

        public TeamBlockPriorityDefinitionInfo(IDictionary<ITeamBlockInfo, PriorityDefinition> tbPriorityDefinition )
        {
            _tbPriorityDefinition = tbPriorityDefinition;
        }

        public IEnumerable<int> HighToLowAgentPriorityList
        {
            get { return (_tbPriorityDefinition.Values.Select(s => s.AgentPriority)).ToList().OrderByDescending(s => s); }
        }

        public IEnumerable<int> HighToLowShiftCategoryPriorityList
        {
            get
            {
                return
                    (_tbPriorityDefinition.Values.Select(s => s.ShiftCategoryPriority)).ToList()
                                                                                       .OrderByDescending(s => s);
            }
        }

        public IEnumerable<int> LowToHighShiftCategoryPriorityList
        {
            get { return (_tbPriorityDefinition.Values.Select(s => s.ShiftCategoryPriority)).ToList().OrderBy(s => s); }
        }

        public IEnumerable<int> LowToHighAgentPriorityList
        {
            get { return (_tbPriorityDefinition.Values.Select(s => s.AgentPriority)).ToList().OrderBy(s => s); }
        }

        public void Clear()
        {
            _tbPriorityDefinition.Clear();
        }

        public ITeamBlockInfo BlockOnAgentPriority(int priority)
        {
            return _tbPriorityDefinition.FirstOrDefault(s => s.Value.AgentPriority == priority).Key;
        }

        public int GetShiftCategoryPriorityOfBlock(ITeamBlockInfo teamBlockInfo)
        {
            return _tbPriorityDefinition[teamBlockInfo].ShiftCategoryPriority;
        }
    }
}
