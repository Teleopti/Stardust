

using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.Collection
{
	public interface ICachedShiftCategoryDistribution
	{
		void SetFilteredPersons(IEnumerable<IPerson> filteredPersons);
		//refact should only return value for requested category
		IDictionary<IShiftCategory, MinMax<int>> GetMinMaxDictionary(IEnumerable<IPerson> filteredPersons);
		IList<IShiftCategory> AllShiftCategories { get; }
		event EventHandler<ModifyEventArgs> PartModified;
	}

	public class CachedShiftCategoryDistribution : ICachedShiftCategoryDistribution
	{
		private readonly IScheduleDictionary _scheduleDictionary;
		private readonly DateOnlyPeriod _periodToMonitor;
		private readonly ICachedNumberOfEachCategoryPerPerson _cachedNumberOfEachCategoryPerPerson;
		private readonly IList<IShiftCategory> _allShiftCategories;
		private readonly HashSet<IPerson> _changedPersons = new HashSet<IPerson>();
		private readonly IDictionary<IShiftCategory, MinMax<int>> _internalDic = new Dictionary<IShiftCategory, MinMax<int>>();

		public CachedShiftCategoryDistribution(IScheduleDictionary scheduleDictionary, DateOnlyPeriod periodToMonitor,
		                                       ICachedNumberOfEachCategoryPerPerson cachedNumberOfEachCategoryPerPerson,
		                                       IList<IShiftCategory> allShiftCategories)
		{
			_scheduleDictionary = scheduleDictionary;
			_periodToMonitor = periodToMonitor;
			_cachedNumberOfEachCategoryPerPerson = cachedNumberOfEachCategoryPerPerson;
			_allShiftCategories = allShiftCategories;
			_scheduleDictionary.PartModified += scheduleDictionary_PartModified;
		}

		public event EventHandler<ModifyEventArgs> PartModified;

		public IList<IShiftCategory> AllShiftCategories => _allShiftCategories;

		void scheduleDictionary_PartModified(object sender, ModifyEventArgs e)
		{
			if (e.ModifiedPart == null)
				_changedPersons.Add(e.ModifiedPerson);
			else
			{
				if (_periodToMonitor.Contains(e.ModifiedPart.DateOnlyAsPeriod.DateOnly))
					_changedPersons.Add(e.ModifiedPerson);
			}
			
			OnPartModified(e);
		}

		private void OnPartModified(ModifyEventArgs e)
		{
			PartModified?.Invoke(this, e);
		}

		public void SetFilteredPersons(IEnumerable<IPerson> filteredPersons)
		{
			_internalDic.Clear();
			_changedPersons.Clear();
			initializeInternalDic();
			foreach (var filteredPerson in filteredPersons)
			{
				_changedPersons.Add(filteredPerson);
			}
		}
		
		public IDictionary<IShiftCategory, MinMax<int>> GetMinMaxDictionary(IEnumerable<IPerson> filteredPersons)
		{
			if (_changedPersons.Count == 0) return _internalDic;
			SetFilteredPersons(filteredPersons);

			foreach (var changedPerson in _changedPersons)
			{
				var values = _cachedNumberOfEachCategoryPerPerson.GetValue(changedPerson);
				foreach (var shiftCategory in _allShiftCategories)
				{
					if (!values.ContainsKey(shiftCategory))
					{
						values.Add(shiftCategory, 0);
					}
				}

				foreach (var categoryCountPair in values)
				{
					var shiftCategory = categoryCountPair.Key;
					if (((IDeleteTag)shiftCategory).IsDeleted)
						continue;

					int count = categoryCountPair.Value;
					var minMax = _internalDic[shiftCategory];
					bool changed = false;
					int newMin = minMax.Minimum;
					int newMax = minMax.Maximum;
					if (count < minMax.Minimum || minMax.Minimum == -1)
					{
						newMin = count;
						changed = true;
					}
					if (count > minMax.Maximum)
					{
						newMax = count;
						changed = true;
					}
					if (changed)
						_internalDic[shiftCategory] = new MinMax<int>(newMin, newMax);
				}
			}
			_changedPersons.Clear();

			return _internalDic;
	
		}

		private void initializeInternalDic()
		{
			foreach (var shiftCategory in _allShiftCategories)
			{
				_internalDic.Add(shiftCategory, new MinMax<int>(-1, 0));
			}
		}
	}
}