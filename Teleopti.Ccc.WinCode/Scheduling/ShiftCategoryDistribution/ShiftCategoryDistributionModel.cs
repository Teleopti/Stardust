﻿

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
		MinMax<int> GetMinMaxForShiftCategory(IShiftCategory shiftCategory);
		double GetAverageForShiftCategory(IShiftCategory shiftCategory);
		double GetStandardDeviationForShiftCategory(IShiftCategory shiftCategory);
		double GetSumOfDeviations();
		event EventHandler ResetNeeded;
		void SetFilteredPersons(IEnumerable<IPerson> filteredPersons);
		int ShiftCategoryCount(IPerson person, IShiftCategory shiftCategory);
		int ShiftCategoryCount(DateOnly dateOnly, IShiftCategory shiftCategory);
		IList<IShiftCategory> GetSortedShiftCategories();
		IList<IShiftCategory> GetShiftCategoriesSortedByMinMax(bool ascending, bool min);
		IList<IShiftCategory> GetShiftCategoriesSortedByAverage(bool ascending);
		string CommonAgentName(IPerson person);
		void OnResetNeeded();
		IList<IPerson> GetSortedPersons(bool ascending);
		IList<DateOnly> GetSortedDates(bool ascending);
		IList<IPerson> GetAgentsSortedByNumberOfShiftCategories(IShiftCategory shiftCategory, bool ascending);
		IList<DateOnly> GetDatesSortedByNumberOfShiftCategories(IShiftCategory shiftCategory, bool ascending);
		ICachedShiftCategoryDistribution CachedShiftCategoryDistribution { get; }
		event EventHandler ChartUpdateNeeded;
		void OnChartUpdateNeeded();
		bool ShouldUpdateViews { get; set; }
		IDictionary<int, int> GetFrequencyForShiftCategory(IShiftCategory shiftCategory);
	}

	public class ShiftCategoryDistributionModel : IShiftCategoryDistributionModel
	{
		private IEnumerable<IPerson> _filteredPersons = new List<IPerson>();
		private readonly ICachedShiftCategoryDistribution _cachedShiftCategoryDistribution;
		private readonly ICachedNumberOfEachCategoryPerDate _cachedNumberOfEachCategoryPerDate;
		private readonly ICachedNumberOfEachCategoryPerPerson _cachedNumberOfEachCategoryPerPerson;
		private readonly DateOnlyPeriod _periodToMonitor;
		private readonly ISchedulerStateHolder _schedulerStateHolder;
		//private int _lastShiftCategoryCount;

		public ShiftCategoryDistributionModel(ICachedShiftCategoryDistribution cachedShiftCategoryDistribution, ICachedNumberOfEachCategoryPerDate cachedNumberOfEachCategoryPerDate, ICachedNumberOfEachCategoryPerPerson cachedNumberOfEachCategoryPerPerson, DateOnlyPeriod periodToMonitor, ISchedulerStateHolder schedulerStateHolder)
		{
			_cachedShiftCategoryDistribution = cachedShiftCategoryDistribution;
			_cachedNumberOfEachCategoryPerDate = cachedNumberOfEachCategoryPerDate;
			_cachedNumberOfEachCategoryPerPerson = cachedNumberOfEachCategoryPerPerson;
			_periodToMonitor = periodToMonitor;
			_schedulerStateHolder = schedulerStateHolder;
		}

		public bool ShouldUpdateViews { get; set; }

		public IDictionary<int, int> GetFrequencyForShiftCategory(IShiftCategory shiftCategory)
		{
			var frequency = new Dictionary<int, int>();

			foreach (var sortedPerson in GetSortedPersons(false))
			{
				var num = ShiftCategoryCount(sortedPerson, shiftCategory);

				if (!frequency.ContainsKey(num))
				{
					frequency.Add(num, 1);
				}
				else
				{
					frequency[num]++;
				}
			}

			return frequency;
		}

		public ICachedShiftCategoryDistribution CachedShiftCategoryDistribution
		{
			get { return _cachedShiftCategoryDistribution; }
		}

		public MinMax<int> GetMinMaxForShiftCategory(IShiftCategory shiftCategory)
		{
			var dic = _cachedShiftCategoryDistribution.GetMinMaxDictionary(_filteredPersons);
			return dic[shiftCategory];
		}

		//use something else than the _populationStatisticsCalculator, should be something you send in values to and it returns what you want and dont calculate anything else
		public double GetAverageForShiftCategory(IShiftCategory shiftCategory)
		{
			var values = new List<double>();
			foreach (var sortedPerson in GetSortedPersons(false))
			{
				values.Add(ShiftCategoryCount(sortedPerson, shiftCategory));
			}
			return Domain.Calculation.Variances.Average(values);
		}

		public double GetStandardDeviationForShiftCategory(IShiftCategory shiftCategory)
		{
			var values = new List<double>();
			foreach (var sortedPerson in GetSortedPersons(false))
			{
				values.Add(ShiftCategoryCount(sortedPerson, shiftCategory));
			}
			return Domain.Calculation.Variances.StandardDeviation(values);
		}

		public double GetSumOfDeviations()
		{
			double sum = 0;
			foreach (var category in GetSortedShiftCategories())
			{
				sum += GetStandardDeviationForShiftCategory(category);
			}

			return sum;
		}

		public event EventHandler ResetNeeded;
		public event EventHandler ChartUpdateNeeded;

		public void SetFilteredPersons(IEnumerable<IPerson> filteredPersons)
		{
			_filteredPersons = filteredPersons;
			_cachedNumberOfEachCategoryPerDate.SetFilteredPersons(_filteredPersons);
			_cachedShiftCategoryDistribution.SetFilteredPersons(_filteredPersons);
			OnResetNeeded();
		}

		public int ShiftCategoryCount(IPerson person, IShiftCategory shiftCategory)
		{
			IDictionary<IShiftCategory, int> shiftCategoryDic = _cachedNumberOfEachCategoryPerPerson.GetValue(person);
			int value;
			if (!shiftCategoryDic.TryGetValue(shiftCategory, out value))
				value = 0;

			return value;
		}

		public int ShiftCategoryCount(DateOnly dateOnly, IShiftCategory shiftCategory)
		{
			IDictionary<IShiftCategory, int> shiftCategoryDic = _cachedNumberOfEachCategoryPerDate.GetValue(dateOnly);
			int value;
			if (!shiftCategoryDic.TryGetValue(shiftCategory, out value))
				value = 0;

			return value;
		}

		public IList<IShiftCategory> GetSortedShiftCategories()
		{
			//var result = new HashSet<IShiftCategory>();
			//foreach (var filteredPerson in _filteredPersons)
			//{
			//	foreach (var shiftCategory in _cachedNumberOfEachCategoryPerPerson.GetValue(filteredPerson).Keys)
			//	{
			//		result.Add(shiftCategory);
			//	}
			//}

			//if (_lastShiftCategoryCount != result.Count)
			//{
			//	_lastShiftCategoryCount = result.Count;
			//	OnResetNeeded();
			//}

			//above returned all anyway so...
			var result = _cachedShiftCategoryDistribution.AllShiftCategories;
			var sortedCategories = from c in result
								   orderby c.Description.ShortName, c.Description.Name
								   select c;

			return sortedCategories.ToList();
		}

		public IList<IShiftCategory> GetShiftCategoriesSortedByMinMax(bool ascending, bool min)
		{
			var result = new List<shiftCategoryIntComparer>();
			var returnList = new List<IShiftCategory>();

			var shiftCategories = GetSortedShiftCategories();

			foreach (var shiftCategory in shiftCategories)
			{
				var minMax = GetMinMaxForShiftCategory(shiftCategory);
				if(min)
					result.Add(new shiftCategoryIntComparer(shiftCategory, minMax.Minimum));
				else
					result.Add(new shiftCategoryIntComparer(shiftCategory, minMax.Maximum));
			}

			result.Sort();
			returnList.AddRange(result.Select(shiftCategoryMinComparer => shiftCategoryMinComparer.ShiftCategory));


			if (ascending)
				returnList.Reverse();

			return returnList;
		}

		public IList<IShiftCategory> GetShiftCategoriesSortedByAverage(bool ascending)
		{
			var result = new List<shiftCategoryDoubleComparer>();
			var returnList = new List<IShiftCategory>();

			var shiftCategories = GetSortedShiftCategories();

			foreach (var shiftCategory in shiftCategories)
			{
				var average = GetAverageForShiftCategory(shiftCategory);
				result.Add(new shiftCategoryDoubleComparer(shiftCategory, average));	
			}

			result.Sort();
			returnList.AddRange(result.Select(shiftCategoryDoubleComparer => shiftCategoryDoubleComparer.ShiftCategory));


			if (ascending)
				returnList.Reverse();

			return returnList;
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

		public virtual void OnChartUpdateNeeded()
		{
			var tmp = ChartUpdateNeeded;
			if(tmp != null)
				ChartUpdateNeeded(this, new EventArgs());
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

		public IList<DateOnly> GetSortedDates(bool ascending)
		{
			List<DateOnly> sortedDates = new List<DateOnly>(_periodToMonitor.DayCollection());
			sortedDates.Sort();

			if (!ascending)
				sortedDates.Reverse();

			return sortedDates;
		}

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

		public IList<DateOnly> GetDatesSortedByNumberOfShiftCategories(IShiftCategory shiftCategory, bool ascending)
		{
			var result = new List<dateIntPair>();
			var returnList = new List<DateOnly>();

			if (shiftCategory == null)
				return returnList;

			foreach (var dateOnly in _periodToMonitor.DayCollection())
			{
				int value;
				if (!_cachedNumberOfEachCategoryPerDate.GetValue(dateOnly).TryGetValue(shiftCategory, out value))
					value = 0;

				result.Add(new dateIntPair(dateOnly, value));
			}

			result.Sort();
			returnList.AddRange(result.Select(dateIntPair => dateIntPair.DateOnly));

			if (ascending)
				returnList.Reverse();

			return returnList;
		}


		private class shiftCategoryDoubleComparer : IComparable
		{
			private readonly double _count;
			public IShiftCategory ShiftCategory { get; private set; }

			public shiftCategoryDoubleComparer(IShiftCategory shiftCategory, double count)
			{
				ShiftCategory = shiftCategory;
				_count = count;
			}

			public int CompareTo(object obj)
			{
				var other = (shiftCategoryDoubleComparer)obj;
				if (other._count == _count)
					return 0;

				if (other._count > _count)
					return 1;

				return -1;
			}
		}

		private class shiftCategoryIntComparer : IComparable
		{
			private readonly int _count;
			public IShiftCategory ShiftCategory { get; private set; }

			public shiftCategoryIntComparer(IShiftCategory shiftCategory, int count)
			{
				ShiftCategory = shiftCategory;
				_count = count;
			}

			public int CompareTo(object obj)
			{
				var other = (shiftCategoryIntComparer)obj;
				if (other._count == _count)
					return 0;

				if (other._count > _count)
					return 1;

				return -1;
			}
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

		private class dateIntPair : IComparable
		{
			private readonly int _count;
			public DateOnly DateOnly { get; private set; }

			public dateIntPair(DateOnly dateOnly, int count)
			{
				DateOnly = dateOnly;
				_count = count;
			}

			public int CompareTo(object obj)
			{
				var other = (dateIntPair)obj;
				if (other._count == _count)
					return 0;

				if (other._count > _count)
					return 1;

				return -1;
			}
		}

		private class intMinMax
		{
			public intMinMax()
			{
				Min = int.MaxValue;
				Max = int.MinValue;
			}

			public int Min { get; set; }

			public int Max { get; set; }
		}
	}
}