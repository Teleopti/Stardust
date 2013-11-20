using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization
{
    public interface IDetermineTeamBlockPriority
    {
        IDictionary<ITeamBlockInfo, PriorityDefinition> CalculatePriority(IList<ITeamBlockInfo> teamBlockInfos, IList<IShiftCategory> shiftCategories);
    }

    public class DetermineTeamBlockPriority : IDetermineTeamBlockPriority
    {
        private readonly ISelectedAgentPoints _selectedAgentPoints;
        private readonly IShiftCategoryPoints _shiftCategoryPoints;

        public DetermineTeamBlockPriority(ISelectedAgentPoints selectedAgentPoints, IShiftCategoryPoints shiftCategoryPoints)
        {
            _selectedAgentPoints = selectedAgentPoints;
            _shiftCategoryPoints = shiftCategoryPoints;
        }

        public IDictionary<ITeamBlockInfo, PriorityDefinition> CalculatePriority(IList<ITeamBlockInfo> teamBlockInfos, IList<IShiftCategory> shiftCategories)
        {
            var tbPriorityDictionary = new Dictionary<ITeamBlockInfo, PriorityDefinition>();
            foreach (ITeamBlockInfo teamBlockInfo in teamBlockInfos)
            {
                IPrioritiseAgentForTeamBlock prioritiseAgentForTeamBlock = new PrioritiseAgentForTeamBlock(_selectedAgentPoints);
                IPriortiseShiftCategoryForTeamBlock priortiseShiftCategoryForTeamBlock = new PriortiseShiftCategoryForTeamBlock(_shiftCategoryPoints);
                extractAgentAndShiftCategoryPriority(teamBlockInfo, prioritiseAgentForTeamBlock, priortiseShiftCategoryForTeamBlock);
                
                var priority = new PriorityDefinition
                    {
                        AgentPriority = prioritiseAgentForTeamBlock.AveragePriority,
                        ShiftCategoryPriority = priortiseShiftCategoryForTeamBlock.AveragePriority
                    };
                tbPriorityDictionary.Add(teamBlockInfo, priority);
            }
            return tbPriorityDictionary;
        }

        private void extractAgentAndShiftCategoryPriority(ITeamBlockInfo teamBlockInfo, IPrioritiseAgentForTeamBlock prioritiseAgentForTeamBlock, IPriortiseShiftCategoryForTeamBlock priortiseShiftCategoryForTeamBlock)
        {
            prioritiseAgentForTeamBlock.GetPriortiseAgentByStartDate(teamBlockInfo.TeamInfo.GroupPerson.GroupMembers.ToList());
            var shiftCategories = new List<IShiftCategory>();
            foreach (var matrix in teamBlockInfo.TeamInfo.MatrixesForGroupAndPeriod(teamBlockInfo.BlockInfo.BlockPeriod)
                )
            {
                foreach (var day in teamBlockInfo.BlockInfo.BlockPeriod.DayCollection())
                {
                    var scheduleDay = matrix.GetScheduleDayByKey(day);
                    if (scheduleDay != null && scheduleDay.DaySchedulePart() != null &&
                        scheduleDay.DaySchedulePart().GetEditorShift() != null)
                    {
                        var sc = scheduleDay.DaySchedulePart().GetEditorShift().ShiftCategory;
                        if(!shiftCategories.Contains(sc ) )
                            shiftCategories.Add(sc);
                    }
                        
                }
            }
            priortiseShiftCategoryForTeamBlock.GetPriortiseShiftCategories(shiftCategories);
        }

        //public IEnumerable<int> HighToLowAgentPriorityList
        //{
        //    get { return (_tbPriorityDictionary.Values.Select(s => s.AgentPriority)).ToList().OrderByDescending(s => s); }
        //}

        //public IEnumerable<int> HighToLowShiftCategoryPriorityList
        //{
        //    get
        //    {
        //        return
        //            (_tbPriorityDictionary.Values.Select(s => s.ShiftCategoryPriority)).ToList()
        //                                                                               .OrderByDescending(s => s);
        //    }
        //}

        //public IEnumerable<int> LowToHighShiftCategoryPriorityList
        //{
        //    get { return (_tbPriorityDictionary.Values.Select(s => s.ShiftCategoryPriority)).ToList().OrderBy(s => s); }
        //}

        //public IEnumerable<int> LowToHighAgentPriorityList
        //{
        //    get { return (_tbPriorityDictionary.Values.Select(s => s.AgentPriority)).ToList().OrderBy(s => s); }
        //}

        //public void Clear()
        //{
        //    _tbPriorityDictionary.Clear();
        //}

        //public ITeamBlockInfo BlockOnAgentPriority(int priority)
        //{
        //    return _tbPriorityDictionary.FirstOrDefault(s => s.Value.AgentPriority == priority).Key;
        //}

        //public int GetShiftCategoryPriorityOfBlock(ITeamBlockInfo teamBlockInfo)
        //{
        //    return _tbPriorityDictionary[teamBlockInfo].ShiftCategoryPriority;
        //}
    }
}