using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution
{
    public interface IDistributionInformationExtractor
    {
	    IList<IShiftCategory> GetShiftCategories();
        IList<ShiftDistribution> ShiftDistributions { get; }
        IList<ShiftCategoryPerAgent> ShiftCategoryPerAgents { get; }
        IList<DateOnly> Dates { get; }
        IList<IPerson> PersonInvolved { get; }
        IList<ShiftFairness> ShiftFairness { get; }
		ICachedNumberOfEachCategoryPerPerson PersonCache { get; }

        /// <summary>
        /// For testing the code
        /// </summary>
        IDictionary<DateOnly, IList<ShiftDistribution>> DateCache { get; }
        Dictionary<int, int> GetShiftCategoryFrequency(IShiftCategory shiftCategory);
	    IList<IPerson> GetAgentsSortedByNumberOfShiftCategories(IShiftCategory shiftCategory);
	    void SetFilteredPersons(IEnumerable<IPerson> filteredPersons);
    }

    public class DistributionInformationExtractor : IDistributionInformationExtractor
    {
	    private readonly ICachedNumberOfEachCategoryPerPerson _cachedNumberOfEachCategoryPerPerson;
	    private readonly DateOnlyPeriod _periodToMonitor;
        private IDictionary<DateOnly, IList<ShiftDistribution>> _dateCache;
	    private IList<IPerson> _filteredPersons = new List<IPerson>();

        public DistributionInformationExtractor(ICachedNumberOfEachCategoryPerPerson cachedNumberOfEachCategoryPerPerson, DateOnlyPeriod periodToMonitor)
        {
	        _cachedNumberOfEachCategoryPerPerson = cachedNumberOfEachCategoryPerPerson;
	        _periodToMonitor = periodToMonitor;
	        _dateCache = new Dictionary<DateOnly, IList<ShiftDistribution>>();
        }

		public void SetFilteredPersons(IEnumerable<IPerson> filteredPersons)
		{
			_filteredPersons = filteredPersons.ToList();
			//recreate datecache

		}

		public IList<IShiftCategory> GetShiftCategories()
		{
			var result = new HashSet<IShiftCategory>();
			foreach (var filteredPerson in _filteredPersons)
			{
				foreach (var shiftCategory in _cachedNumberOfEachCategoryPerPerson.GetValue(filteredPerson).Keys)
				{
					result.Add(shiftCategory);
				}
			}

			return new List<IShiftCategory>(result);
		}

		public IList<DateOnly> Dates
		{
			get { return _periodToMonitor.DayCollection(); }
		}

		public IList<IPerson> PersonInvolved
		{
			get { return _filteredPersons; }
		}

		//this one is a bit slow but we doesn't sort that often
		public IList<IPerson> GetAgentsSortedByNumberOfShiftCategories(IShiftCategory shiftCategory)
		{
			var result = new SortedSet<AgentIntPair>();

			foreach (var person in _filteredPersons)
			{
				int value;
				if (!_cachedNumberOfEachCategoryPerPerson.GetValue(person).TryGetValue(shiftCategory, out value))
					value = 0;

				result.Add(new AgentIntPair(person, value));
			}

			var returnList = new List<IPerson>();
			foreach (var agentIntPair in result)
			{
				returnList.Add(agentIntPair.Person);
			}

			return returnList;
		}

        public Dictionary<int, int> GetShiftCategoryFrequency(IShiftCategory shiftCategory)
        {
            var result = new Dictionary<int, int>();

	        foreach (var filteredPerson in _filteredPersons)
	        {
				int value;
				if (!_cachedNumberOfEachCategoryPerPerson.GetValue(filteredPerson).TryGetValue(shiftCategory, out value))
					value = 0;

				if (result.ContainsKey(value))
					result[value]++;
				else
					result.Add(value, 1);
	        }

            foreach (var item in ShiftCategoryPerAgents.Where(i => i.ShiftCategory == shiftCategory))
            {
                if (result.ContainsKey(item.Count))
                    result[item.Count]++;
                else
                    result.Add(item.Count ,1);
                
            }
            return result;
        }

		public ICachedNumberOfEachCategoryPerPerson PersonCache
        {
			get
			{
				return _cachedNumberOfEachCategoryPerPerson;
			}
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
				//foreach (var perAgent in _personCache.Values)
				//{
				//	result.AddRange(perAgent);
				//}
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

		private class AgentIntPair : IComparable
		{
			private readonly IPerson _person;
			private readonly int _count;

			public AgentIntPair(IPerson person, int count)
			{
				_person = person;
				_count = count;
			}

			public IPerson Person
			{
				get { return _person; }
			}

			public int CompareTo(object obj)
			{
				var other = (AgentIntPair) obj;
				if (other._count == _count)
					return 0;

				if (other._count > _count)
					return 1;

				return -1;
			}
		}
    }

}