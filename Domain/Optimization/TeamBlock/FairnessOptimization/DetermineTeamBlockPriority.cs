using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization
{
    public interface IDetermineTeamBlockPriority
    {
        IEnumerable<int> HighToLowAgentPriorityList { get; }
        IEnumerable<int> HighToLowShiftCategoryPriorityList { get; }
        IEnumerable<int> LowToHighShiftCategoryPriorityList { get; }
        IEnumerable<int> LowToHighAgentPriorityList { get; }
        void CalculatePriority(IList<ITeamBlockInfo> teamBlockInfos, IList<IShiftCategory> shiftCategories);
        void Clear();
        ITeamBlockInfo BlockOnAgentPriority(int priority);
        int GetShiftCategoryPriorityOfBlock(ITeamBlockInfo teamBlockInfo);
    }

    public class DetermineTeamBlockPriority : IDetermineTeamBlockPriority
    {
        private readonly IPrioritiseAgentByContract _prioritiseAgentByContract;
        private readonly IPriortiseShiftCategory _priortiseShiftCategory;
        private readonly IDictionary<ITeamBlockInfo, PriorityDefinition> _tbPriorityDictionary;

        public DetermineTeamBlockPriority(IPrioritiseAgentByContract prioritiseAgentByContract,
                                          IPriortiseShiftCategory priortiseShiftCategory)
        {
            _prioritiseAgentByContract = prioritiseAgentByContract;
            _priortiseShiftCategory = priortiseShiftCategory;
            _tbPriorityDictionary = new Dictionary<ITeamBlockInfo, PriorityDefinition>();
        }

        public void CalculatePriority(IList<ITeamBlockInfo> teamBlockInfos, IList<IShiftCategory> shiftCategories)
        {
            Clear();
            foreach (ITeamBlockInfo teamBlockInfo in teamBlockInfos)
            {
                extractAgentAndShiftCategoryPriority(teamBlockInfo);
                var priority = new PriorityDefinition
                    {
                        AgentPriority = _prioritiseAgentByContract.AveragePriority,
                        ShiftCategoryPriority = _priortiseShiftCategory.AveragePriority
                    };
                _tbPriorityDictionary.Add(teamBlockInfo, priority);
            }
        }

        private void extractAgentAndShiftCategoryPriority(ITeamBlockInfo teamBlockInfo)
        {
            _prioritiseAgentByContract.GetPriortiseAgentByStartDate(teamBlockInfo.TeamInfo.GroupPerson.GroupMembers.ToList());
            var shiftCategories = new List<IShiftCategory>();
            foreach (var matrix in teamBlockInfo.TeamInfo.MatrixesForGroupAndPeriod(teamBlockInfo.BlockInfo.BlockPeriod)
                )
            {
                foreach (var day in teamBlockInfo.BlockInfo.BlockPeriod.DayCollection())
                {
                    var scheduleDay = matrix.GetScheduleDayByKey(day);
                    if(scheduleDay!=null && scheduleDay.DaySchedulePart()!=null && scheduleDay.DaySchedulePart().GetEditorShift()!= null)
                        shiftCategories.Add(scheduleDay.DaySchedulePart().GetEditorShift().ShiftCategory);
                }
            }
            _priortiseShiftCategory.GetPriortiseShiftCategories(shiftCategories);
        }

        public IEnumerable<int> HighToLowAgentPriorityList
        {
            get { return (_tbPriorityDictionary.Values.Select(s => s.AgentPriority)).ToList().OrderByDescending(s => s); }
        }

        public IEnumerable<int> HighToLowShiftCategoryPriorityList
        {
            get
            {
                return
                    (_tbPriorityDictionary.Values.Select(s => s.ShiftCategoryPriority)).ToList()
                                                                                       .OrderByDescending(s => s);
            }
        }

        public IEnumerable<int> LowToHighShiftCategoryPriorityList
        {
            get { return (_tbPriorityDictionary.Values.Select(s => s.ShiftCategoryPriority)).ToList().OrderBy(s => s); }
        }

        public IEnumerable<int> LowToHighAgentPriorityList
        {
            get { return (_tbPriorityDictionary.Values.Select(s => s.AgentPriority)).ToList().OrderBy(s => s); }
        }

        public void Clear()
        {
            _tbPriorityDictionary.Clear();
            _prioritiseAgentByContract.Clear();
            _priortiseShiftCategory.Clear();
        }

        public ITeamBlockInfo BlockOnAgentPriority(int priority)
        {
            return _tbPriorityDictionary.FirstOrDefault(s => s.Value.AgentPriority == priority).Key;
        }

        public int GetShiftCategoryPriorityOfBlock(ITeamBlockInfo teamBlockInfo)
        {
            return _tbPriorityDictionary[teamBlockInfo].ShiftCategoryPriority;
        }
    }
}