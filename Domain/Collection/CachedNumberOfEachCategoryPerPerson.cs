using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

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

		public int ItemCount 
		{ 
			get { return _internalDic.Count; }
		}

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
			IDictionary<IShiftCategory, int> value = new Dictionary<IShiftCategory, int>();
			var range = _scheduleDictionary[person];

			foreach (var dateOnly in _periodToMonitor.DayCollection())
			{
			    if (dateOnly > person.TerminalDate) continue;
                var scheduleDay = range.ScheduledDay(dateOnly);
				var shiftCategory = scheduleDay.PersonAssignment(true).ShiftCategory;
				if (shiftCategory == null)
					continue;

				if(!value.ContainsKey(shiftCategory))
					value.Add(shiftCategory, 0);

				value[shiftCategory]++;
			}

			return value;
		}
	}

}