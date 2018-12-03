using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

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

		public int ItemCount => _internalDic.Count;

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
			var agents = _filteredAgents.Where(filteredAgent => dateOnly <= filteredAgent.TerminalDate.GetValueOrDefault(DateOnly.MaxValue)).ToArray();

			return _scheduleDictionary.SchedulesForPeriod(dateOnly.ToDateOnlyPeriod(), agents)
				.Where(s => s.SignificantPartForDisplay().Equals(SchedulePartView.MainShift))
				.Select(s => s.PersonAssignment()?.ShiftCategory)
				.Where(s => s != null)
				.GroupBy(s => s)
				.ToDictionary(k => k.Key,v => v.Count());
		}
	}
}