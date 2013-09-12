﻿using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Collection
{
	public interface ICachedNumberOfEachCategoryPerDate
	{
		IDictionary<IShiftCategory, int> GetValue(DateOnly dateOnly);
		int ItemCount { get; }
		void Clear();
	}

	public class CachedNumberOfEachCategoryPerDate : ICachedNumberOfEachCategoryPerDate
	{
		private readonly IScheduleDictionary _scheduleDictionary;
		private readonly DateOnlyPeriod _periodToMonitor;
		private readonly IEnumerable<IPerson> _filteredAgents;
		private readonly IDictionary<DateOnly, IDictionary<IShiftCategory, int>> _internalDic = new Dictionary<DateOnly, IDictionary<IShiftCategory, int>>();

		public CachedNumberOfEachCategoryPerDate(IScheduleDictionary scheduleDictionary, DateOnlyPeriod periodToMonitor, IEnumerable<IPerson> filteredAgents)
		{
			_scheduleDictionary = scheduleDictionary;
			_periodToMonitor = periodToMonitor;
			_filteredAgents = filteredAgents;
			_scheduleDictionary.PartModified += scheduleDictionary_PartModified;
		}

		public IDictionary<IShiftCategory, int> GetValue(DateOnly dateOnly)
		{
			IDictionary<IShiftCategory, int> value;
			if (!_internalDic.TryGetValue(dateOnly, out value))
			{
				value = calculateValue(dateOnly);
				_internalDic.Add(dateOnly, value);
			}

			return value;
		}

		public int ItemCount
		{
			get
			{
				return _internalDic.Count;
			}
		}

		public void Clear()
		{
			_internalDic.Clear();
		}

		void scheduleDictionary_PartModified(object sender, ModifyEventArgs e)
		{
			if (e.ModifiedPart == null)
				_internalDic.Clear();
			else
			{
				var dateOnly = e.ModifiedPart.DateOnlyAsPeriod.DateOnly;
				if (_periodToMonitor.Contains(dateOnly))
					_internalDic.Remove(dateOnly);
			}
		}

		private IDictionary<IShiftCategory, int> calculateValue(DateOnly dateOnly)
		{
			IDictionary<IShiftCategory, int> value = new Dictionary<IShiftCategory, int>();
			foreach (var filteredAgent in _filteredAgents)
			{
				var scheduleDay = _scheduleDictionary[filteredAgent].ScheduledDay(dateOnly);
				var shiftCategory = scheduleDay.PersonAssignment(true).ShiftCategory;
				if (shiftCategory == null)
					continue;

				if (!value.ContainsKey(shiftCategory))
					value.Add(shiftCategory, 0);

				value[shiftCategory]++;
			}

			return value;
		}
	}
}