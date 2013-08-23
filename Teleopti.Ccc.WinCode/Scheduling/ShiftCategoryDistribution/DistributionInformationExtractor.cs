using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.WinCodeTest.Scheduler.ShiftCategoryDistribution;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution
{
    public interface IDistributionInformationExtractor
    {
        IList<string> ShiftCategories { get; }
        IList<DateOnly> Dates { get; }
        IList<IPerson> PersonInvolved { get; }
        IList<ShiftDistribution> GetShiftDistribution();
        IList<ShiftCategoryPerAgent> GetShiftCategoryPerAgent();
        IList<ShiftFairness> GetShiftFairness();
    }

    public class DistributionInformationExtractor : IDistributionInformationExtractor
    {
        private readonly ShiftCategoryAttributesExtractor _shiftCategoryAttributedExtractor;
        private readonly IList<ShiftCategoryStructure> _mappedScheduleDays;
        private IList<ShiftCategoryPerAgent> _shiftCategoryPerAgentList;

        public DistributionInformationExtractor(IList<IScheduleDay> scheduleDays)
        {
            _mappedScheduleDays = ScheduleDayToShiftCategoryMapper.MapScheduleDay(scheduleDays);
            _shiftCategoryAttributedExtractor = new ShiftCategoryAttributesExtractor();
            _shiftCategoryAttributedExtractor.ExtractShiftCategoryInformation( _mappedScheduleDays);
            _shiftCategoryPerAgentList = new List<ShiftCategoryPerAgent>();
        }

        public IList<string> ShiftCategories { get { return _shiftCategoryAttributedExtractor.ShiftCategories; } }

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

    }
}