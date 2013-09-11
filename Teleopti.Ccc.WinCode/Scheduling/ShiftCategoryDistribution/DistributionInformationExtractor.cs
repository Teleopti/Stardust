using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution
{
    public interface IDistributionInformationExtractor
    {
        IList<IShiftCategory> ShiftCategories { get; }
        IList<ShiftDistribution> ShiftDistributions { get; }
        IList<ShiftCategoryPerAgent> ShiftCategoryPerAgents { get; }
        IList<DateOnly> Dates { get; }
        IList<IPerson> PersonInvolved { get; }
        IList<ShiftFairness> ShiftFairness { get; }

        /// <summary>
        /// For testing the code
        /// </summary>
        IDictionary<IPerson, IList<ShiftCategoryPerAgent>> PersonCache { get; }

        /// <summary>
        /// For testing the code
        /// </summary>
        IDictionary<DateOnly, IList<ShiftDistribution>> DateCache { get; }
        Dictionary<int, int> GetShiftCategoryFrequency(IShiftCategory shiftCategory);
        void ExtractDistributionInfo(IList<IScheduleDay> allSchedules, ModifyEventArgs modifyEventArgs, TimeZoneInfo timeZoneInfo);
    }

    public class DistributionInformationExtractor : IDistributionInformationExtractor
    {
        private readonly ShiftCategoryAttributesExtractor _shiftCategoryAttributedExtractor;
        private IList<ShiftCategoryStructure> _mappedScheduleDays;
        private IDictionary<IPerson, IList<ShiftCategoryPerAgent>> _personCache;
        private IDictionary<DateOnly, IList<ShiftDistribution>> _dateCache; 

        public DistributionInformationExtractor()
        {
            _shiftCategoryAttributedExtractor = new ShiftCategoryAttributesExtractor();
            _personCache = new Dictionary<IPerson, IList<ShiftCategoryPerAgent>>();
            _dateCache = new Dictionary<DateOnly, IList<ShiftDistribution>>();
        }

        public IList<IShiftCategory> ShiftCategories { get { return _shiftCategoryAttributedExtractor.ShiftCategories; } }

        public IList<DateOnly> Dates { get { return _shiftCategoryAttributedExtractor.Dates; } }

        public IList<IPerson> PersonInvolved { get { return _shiftCategoryAttributedExtractor.PersonInvolved; } }

        public Dictionary<int, int> GetShiftCategoryFrequency(IShiftCategory shiftCategory)
        {
            var result = new Dictionary<int, int>();
            foreach (var item in ShiftCategoryPerAgents.Where(i => i.ShiftCategory == shiftCategory))
            {
                if (result.ContainsKey(item.Count))
                    result[item.Count]++;
                else
                    result.Add(item.Count ,1);
                
            }
            return result;
        }

        /// <summary>
        /// For testing the code
        /// </summary>
        public IDictionary<IPerson, IList<ShiftCategoryPerAgent>> PersonCache
        {
            get { return _personCache; }
        }

        /// <summary>
        /// For testing the code
        /// </summary>
        public IDictionary<DateOnly, IList<ShiftDistribution>> DateCache
        {
            get { return _dateCache; }
        }

        public IList<ShiftDistribution> ShiftDistributions 
        { 
            get
            {
                var result = new List<ShiftDistribution>();
                foreach (var shiftDist in _dateCache.Values)
                {
                    result.AddRange(shiftDist);
                }
                return result;
            } 
        }
        public IList<ShiftCategoryPerAgent> ShiftCategoryPerAgents
        {
            get
            {
                var result = new List<ShiftCategoryPerAgent>();
                foreach (var perAgent in _personCache.Values)
                {
                    result.AddRange(perAgent);
                }
                return result;
            }
        }

        public IList<ShiftFairness> ShiftFairness
        {
            get
            {
                IList<ShiftFairness> result = new List<ShiftFairness>();
                if (ShiftCategoryPerAgents.Any())
                {
                    result = ShiftFairnessCalculator.GetShiftFairness(ShiftCategoryPerAgents);
                }
                return result;
            }
        }

        public void ExtractDistributionInfo(IList<IScheduleDay> allSchedules, ModifyEventArgs modifyEventArgs, TimeZoneInfo timeZoneInfo)
        {
            if (modifyEventArgs == null)
            {
                _personCache.Clear();
                _dateCache.Clear();
                _mappedScheduleDays = ScheduleDayToShiftCategoryMapper.MapScheduleDay(allSchedules);
                _shiftCategoryAttributedExtractor.ExtractShiftCategoryInformation(_mappedScheduleDays);
                updatePersonCache(_mappedScheduleDays);
                updateDateCache(_mappedScheduleDays);
            }
            else
            {
                var localDate = new DateOnly(modifyEventArgs.ModifiedPeriod.StartDateTimeLocal(timeZoneInfo));
                _dateCache.Remove(localDate);
                _personCache.Remove(modifyEventArgs.ModifiedPerson);

                var modifiedScheduleDays = allSchedules.Where(s => s.Person == modifyEventArgs.ModifiedPerson ).ToArray() ;
                var mappedScheduleDays = ScheduleDayToShiftCategoryMapper.MapScheduleDay(modifiedScheduleDays);
                updatePersonCache(mappedScheduleDays);
                modifiedScheduleDays = allSchedules.Where(s => s.DateOnlyAsPeriod.Period().Intersect(modifyEventArgs.ModifiedPeriod )).ToArray();
                mappedScheduleDays = ScheduleDayToShiftCategoryMapper.MapScheduleDay(modifiedScheduleDays);
                updateDateCache(mappedScheduleDays);
            }

        }

        private void updatePersonCache(IList<ShiftCategoryStructure> mappedScheduleDays)
        {
            var shiftCategoryPerAgentList = ShiftCategoryPerAgentCalculator.Extract(mappedScheduleDays);
            foreach (var shiftCategoryPerAgent in shiftCategoryPerAgentList)
            {
                if (_personCache.ContainsKey(shiftCategoryPerAgent.Person))
                    _personCache[shiftCategoryPerAgent.Person].Add(shiftCategoryPerAgent);
                else
                    _personCache.Add(shiftCategoryPerAgent.Person, new List<ShiftCategoryPerAgent> { shiftCategoryPerAgent });
            }
        }

        private void updateDateCache(IList<ShiftCategoryStructure> mappedScheduleDays)
        {
            var shiftDistributionList = ShiftDistributionCalculator.Extract(mappedScheduleDays);
            foreach (var shiftDistribution in shiftDistributionList)
            {
                if (_dateCache.ContainsKey(shiftDistribution.DateOnly))
                    _dateCache[shiftDistribution.DateOnly].Add(shiftDistribution);
                else
                    _dateCache.Add(shiftDistribution.DateOnly, new List<ShiftDistribution> { shiftDistribution });
            }
        }
    }
}