using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Collection
{
	public interface ICachedNumberOfEachCategoryPerPerson
	{
		IDictionary<IShiftCategory, int> GetValue(IPerson person);
		int ItemCount { get; }
	}

	public class CachedNumberOfEachCategoryPerPerson : ICachedNumberOfEachCategoryPerPerson
	{
		private readonly IScheduleDictionary _scheduleDictionary;
		private readonly DateOnlyPeriod _periodToMonitor;
		private readonly IDictionary<IPerson, IDictionary<IShiftCategory, int>> _internalDic = new Dictionary<IPerson, IDictionary<IShiftCategory, int>>();

		public CachedNumberOfEachCategoryPerPerson(IScheduleDictionary scheduleDictionary, DateOnlyPeriod periodToMonitor)
		{
			_scheduleDictionary = scheduleDictionary;
			_periodToMonitor = periodToMonitor;
			_scheduleDictionary.PartModified += scheduleDictionary_PartModified;
		}

		public IDictionary<IShiftCategory, int> GetValue(IPerson person)
		{
			IDictionary<IShiftCategory, int> value;
			if (!_internalDic.TryGetValue(person, out value))
			{
				value = calculateValue(person);
				_internalDic.Add(person, value);
			}

			return value;
		}

		public int ItemCount => _internalDic.Count;

		void scheduleDictionary_PartModified(object sender, ModifyEventArgs e)
		{
			if(e.ModifiedPart==null)
				_internalDic.Clear();
			else
			{
				if (_periodToMonitor.Contains(e.ModifiedPart.DateOnlyAsPeriod.DateOnly))
					_internalDic.Remove(e.ModifiedPart.Person);
			}
		}

		private IDictionary<IShiftCategory, int> calculateValue(IPerson person)
		{
			var range = _scheduleDictionary[person];
			var schedules = range.ScheduledDayCollection(_periodToMonitor);
			return schedules.Where(
					scheduleDay =>
						scheduleDay.DateOnlyAsPeriod.DateOnly <= person.TerminalDate.GetValueOrDefault(DateOnly.MaxValue) &&
						scheduleDay.SignificantPartForDisplay().Equals(SchedulePartView.MainShift))
				.Select(x => x.PersonAssignment()?.ShiftCategory)
				.Where(x => x != null)
				.GroupBy(x => x)
				.ToDictionary(k => k.Key, v => v.Count());
		}
	}
}