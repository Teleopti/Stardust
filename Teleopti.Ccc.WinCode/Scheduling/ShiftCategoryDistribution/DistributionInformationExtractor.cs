using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution
{
    public interface IDistributionInformationExtractor
    {
        IList<IShiftCategory> ShiftCategories { get; }
        IList<DateOnly> Dates { get; }
        IList<IPerson> PersonInvolved { get; }
        IList<ShiftDistribution> GetShiftDistribution();
        IList<ShiftCategoryPerAgent> GetShiftCategoryPerAgent();
        IList<ShiftFairness> GetShiftFairness();
        Dictionary<int, int> GetShiftCategoryFrequency(IShiftCategory shiftCategory);
    }

    public class DistributionInformationExtractor : IDistributionInformationExtractor
    {
        private ShiftCategoryAttributesExtractor _shiftCategoryAttributedExtractor;
        private IList<ShiftCategoryStructure> _mappedScheduleDays;
        private IList<ShiftCategoryPerAgent> _shiftCategoryPerAgentList;

        public DistributionInformationExtractor()
        {
            //_mappedScheduleDays = ScheduleDayToShiftCategoryMapper.MapScheduleDay(scheduleDays);
            _shiftCategoryAttributedExtractor = new ShiftCategoryAttributesExtractor();
            //_shiftCategoryAttributedExtractor.ExtractShiftCategoryInformation( _mappedScheduleDays);
            _shiftCategoryPerAgentList = new List<ShiftCategoryPerAgent>();
        }

        public IList<IShiftCategory> ShiftCategories { get { return _shiftCategoryAttributedExtractor.ShiftCategories; } }

        public IList<DateOnly> Dates { get { return _shiftCategoryAttributedExtractor.Dates; } }

        public IList<IPerson> PersonInvolved { get { return _shiftCategoryAttributedExtractor.PersonInvolved; } }

        public IList<ShiftDistribution> GetShiftDistribution()
        {
            return ShiftDistributionCalculator.Extract(_mappedScheduleDays);
        }

        public IList<ShiftCategoryPerAgent> GetShiftCategoryPerAgent()
        {
            _shiftCategoryPerAgentList = ShiftCategoryPerAgentCalculator.Extract(_mappedScheduleDays);
            return ShiftCategoryPerAgentCalculator.Extract(_mappedScheduleDays);
        }

        public IList<ShiftFairness> GetShiftFairness()
        {
            if (!_shiftCategoryPerAgentList.Any())
                _shiftCategoryPerAgentList = ShiftCategoryPerAgentCalculator.Extract(_mappedScheduleDays);
            return ShiftFairnessCalculator.GetShiftFairness(_shiftCategoryPerAgentList);
        }

        public Dictionary<int, int> GetShiftCategoryFrequency(IShiftCategory shiftCategory)
        {
            var result = new Dictionary<int, int>();
            var shiftCategoriesPerAgent = GetShiftCategoryPerAgent();
            foreach (var item in shiftCategoriesPerAgent.Where(i=>i.ShiftCategory == shiftCategory ))
            {
                if (result.ContainsKey(item.Count))
                    result[item.Count]++;
                else
                    result.Add(item.Count ,1);
                
            }
            return result;
        }

        public void ExtractDistributionInfo(IList<IScheduleDay> allSchedules)
        {
            _mappedScheduleDays = ScheduleDayToShiftCategoryMapper.MapScheduleDay(allSchedules);
            _shiftCategoryAttributedExtractor.ExtractShiftCategoryInformation(_mappedScheduleDays);
        }
    }
}