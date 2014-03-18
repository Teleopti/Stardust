using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Collection
{
	public interface ICachedNumberOfEachCategoryPerDate
	{
		IDictionary<IShiftCategory, int> GetValue(DateOnly dateOnly);
		int ItemCount { get; }
		void SetFilteredPersons(IEnumerable<IPerson> filteredPersons);
	}

	public class CachedNumberOfEachCategoryPerDate : ICachedNumberOfEachCategoryPerDate
	{
		private readonly IScheduleDictionary _scheduleDictionary;
		private readonly DateOnlyPeriod _periodToMonitor;
		private IEnumerable<IPerson> _filteredAgents = new List<IPerson>();
		private readonly IDictionary<DateOnly, IDictionary<IShiftCategory, int>> _internalDic = new Dictionary<DateOnly, IDictionary<IShiftCategory, int>>();

		public CachedNumberOfEachCategoryPerDate(IScheduleDictionary scheduleDictionary, DateOnlyPeriod periodToMonitor)
		{
			_scheduleDictionary = scheduleDictionary;
			_periodToMonitor = periodToMonitor;
			_scheduleDictionary.PartModified += scheduleDictionaryPartModified;
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

		public void SetFilteredPersons(IEnumerable<IPerson> filteredPersons)
		{
			_internalDic.Clear();
			_filteredAgents = filteredPersons;
		}

		private void scheduleDictionaryPartModified(object sender, ModifyEventArgs e)
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
			    if (dateOnly > filteredAgent.TerminalDate) continue;
                var scheduleDay = _scheduleDictionary[filteredAgent].ScheduledDay(dateOnly);
				if(!scheduleDay.SignificantPartForDisplay().Equals(SchedulePartView.MainShift)) continue;
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