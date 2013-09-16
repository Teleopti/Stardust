

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution
{
	public interface IShiftCategoryDistributionModel
	{
		void SetFilteredPersons(IEnumerable<IPerson> filteredPersons);

		//IEnumerable<IPerson> FilteredPersons { get; }
		int ShiftCategoryCount(IPerson person, IShiftCategory shiftCategory);
		IList<IShiftCategory> GetSortedShiftCategories();
		string CommonAgentName(IPerson person);
		IList<IPerson> GetSortedPersons(bool ascending);
		IList<IPerson> GetAgentsSortedByNumberOfShiftCategories(IShiftCategory shiftCategory, bool ascending);

		event EventHandler ResetNeeded;
	}

	public class ShiftCategoryDistributionModel : IShiftCategoryDistributionModel
	{
		private IEnumerable<IPerson> _filteredPersons = new List<IPerson>();
		private readonly ICachedNumberOfEachCategoryPerPerson _cachedNumberOfEachCategoryPerPerson;
		private readonly DateOnlyPeriod _periodToMonitor;
		private readonly ISchedulerStateHolder _schedulerStateHolder;
		private int _lastShiftCategoryCount;

		public ShiftCategoryDistributionModel(ICachedNumberOfEachCategoryPerPerson cachedNumberOfEachCategoryPerPerson, DateOnlyPeriod periodToMonitor, ISchedulerStateHolder schedulerStateHolder)
		{
			_cachedNumberOfEachCategoryPerPerson = cachedNumberOfEachCategoryPerPerson;
			_periodToMonitor = periodToMonitor;
			_schedulerStateHolder = schedulerStateHolder;
		}

		public event EventHandler ResetNeeded;

		public void SetFilteredPersons(IEnumerable<IPerson> filteredPersons)
		{
			_filteredPersons = filteredPersons;
			OnResetNeeded();
		}

		//public IEnumerable<IPerson> FilteredPersons
		//{
		//	get
		//	{
		//		return _filteredPersons;
		//	}
		//}

		public int ShiftCategoryCount(IPerson person, IShiftCategory shiftCategory)
		{
			IDictionary<IShiftCategory, int> shiftCategoryDic = _cachedNumberOfEachCategoryPerPerson.GetValue(person);
			int value;
			if (!shiftCategoryDic.TryGetValue(shiftCategory, out value))
				value = 0;

			return value;
		}

		public IList<IShiftCategory> GetSortedShiftCategories()
		{
			var result = new HashSet<IShiftCategory>();
			foreach (var filteredPerson in _filteredPersons)
			{
				foreach (var shiftCategory in _cachedNumberOfEachCategoryPerPerson.GetValue(filteredPerson).Keys)
				{
					result.Add(shiftCategory);
				}
			}

			if (_lastShiftCategoryCount != result.Count)
			{
				_lastShiftCategoryCount = result.Count;
				OnResetNeeded();
			}

			var sortedCategories = from c in result
								   orderby c.Description.ShortName, c.Description.Name
								   select c;

			return sortedCategories.ToList();
		}

		public string CommonAgentName(IPerson person)
		{
			return _schedulerStateHolder.CommonAgentName(person);
		}

		public virtual void OnResetNeeded()
		{
			var tmp = ResetNeeded;
			if(tmp != null)
				ResetNeeded(this, new EventArgs());
		}

		public IList<IPerson> GetSortedPersons(bool ascending)
		{
			CultureInfo loggedOnCulture = TeleoptiPrincipal.Current.Regional.Culture;
			IComparer<object> comparer = new PersonNameComparer(loggedOnCulture);

			var sortedFilteredPersons =
				_filteredPersons.OrderBy(p => p.Name, comparer).ToList();

			if (!ascending)
				sortedFilteredPersons.Reverse();

			return sortedFilteredPersons;
		}

		//this one is a bit slow but we doesn't sort that often
		public IList<IPerson> GetAgentsSortedByNumberOfShiftCategories(IShiftCategory shiftCategory, bool ascending)
		{
			var result = new List<agentIntPair>();
			var returnList = new List<IPerson>();

			if (shiftCategory == null)
				return returnList;

			foreach (var person in _filteredPersons)
			{
				int value;
				if (!_cachedNumberOfEachCategoryPerPerson.GetValue(person).TryGetValue(shiftCategory, out value))
					value = 0;

				result.Add(new agentIntPair(person, value));
			}

			result.Sort();
			returnList.AddRange(result.Select(agentIntPair => agentIntPair.Person));

			if (ascending)
				returnList.Reverse();

			return returnList;
		}

		private class agentIntPair : IComparable
		{
			private readonly int _count;
			public IPerson Person { get; private set; }

			public agentIntPair(IPerson person, int count)
			{
				Person = person;
				_count = count;
			}

			public int CompareTo(object obj)
			{
				var other = (agentIntPair)obj;
				if (other._count == _count)
					return 0;

				if (other._count > _count)
					return 1;

				return -1;
			}
		}
	}
}