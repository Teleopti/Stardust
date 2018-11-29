using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class GridlockManager : IGridlockManager
    {
        private readonly IDictionary<string, GridlockDictionary> _gridlocks = new Dictionary<string, GridlockDictionary>();
		
        public IDictionary<string, GridlockDictionary> GridlocksDictionary
        {
            get { return _gridlocks; }
        }
		
        public void AddLock(IPerson person, DateOnly dateOnly, LockType lockType)
        {
            string key = GetPersonDateKey(person, dateOnly);

			GridlockDictionary personDateLocks;
            if (!_gridlocks.TryGetValue(key, out personDateLocks))
            {
				personDateLocks = new GridlockDictionary();
                _gridlocks.Add(key,personDateLocks);
            }
            Gridlock gridlock = new Gridlock(person, dateOnly, lockType);
            if (!personDateLocks.ContainsKey(gridlock.Key))
            {
                personDateLocks.Add(gridlock.Key, gridlock);
            }
        }

        public void AddLock(IScheduleDay schedulePart, LockType lockType)
        {
            DateOnly dateOnly = schedulePart.DateOnlyAsPeriod.DateOnly;
            AddLock(schedulePart.Person, dateOnly, lockType);
        }

        public void AddLock(IList<IScheduleDay> schedules, LockType lockType)
        {
            foreach (IScheduleDay schedule in schedules)
            {
                AddLock(schedule, lockType);
            }
        }
		
        public void RemoveLock(IPerson person, DateOnly dateOnly)
        {
            RemoveLock(person, dateOnly, LockType.Normal);
        }

        public void RemoveLock(IPerson person, DateOnly dateOnly, LockType lockType)
        {
            GridlockDictionary gridlocks = Gridlocks(person, dateOnly);
            string keyToRemove = "";
            if (gridlocks != null)
            {
                foreach (KeyValuePair<string, Gridlock> pair in gridlocks)
                {
                    if (pair.Value.LockType == lockType)
                    {
                        keyToRemove = pair.Key;
	                    break;
                    }
                }
                if (!string.IsNullOrEmpty(keyToRemove))
                {
                    gridlocks.Remove(keyToRemove);
                }
                if (gridlocks.Count == 0)
                    GridlocksDictionary.Remove(GetPersonDateKey(person, dateOnly));
            }
        }

	    public void RemoveLock(IScheduleDay schedulePart)
	    {
		    RemoveLock(schedulePart.Person, new DateOnly(schedulePart.Period.StartDateTimeLocal(schedulePart.TimeZone)));
	    }

	    public void RemoveLock(IList<IScheduleDay> schedules)
        {
            foreach (IScheduleDay schedule in schedules)
            {
                RemoveLock(schedule);
            }
        }
		
        public bool HasLocks => _gridlocks.Count > 0;

	    public GridlockDictionary Gridlocks(IPerson person, DateOnly dateOnly)
        {
            string dummyKey = GetPersonDateKey(person, dateOnly);

	        GridlockDictionary value;
	        if (_gridlocks.TryGetValue(dummyKey, out value))
                return value;
            return null;
        }
		
				public GridlockDictionary Gridlocks(IScheduleDay schedulePart)
        {
            return Gridlocks(schedulePart.Person, new DateOnly(schedulePart.Period.StartDateTimeLocal(schedulePart.TimeZone)));
        }

        public IList<IScheduleDay> UnlockedDays(IList<IScheduleDay> scheduleDays)
        {
            return scheduleDays.Where(scheduleDay => Gridlocks(scheduleDay.Person, scheduleDay.DateOnlyAsPeriod.DateOnly) == null).ToList();
        }
		
        public void Clear()
        {
            IList<GridlockDictionary> upForDelete = new List<GridlockDictionary>();

            foreach(KeyValuePair<string,GridlockDictionary> kvp in _gridlocks)
            {
                upForDelete.Add(kvp.Value);
            }

            IList<Gridlock> upForDelete2 = new List<Gridlock>();
            foreach (GridlockDictionary dictionary in upForDelete)
            {
                foreach (KeyValuePair<string, Gridlock> pair in dictionary)
                {
                    upForDelete2.Add(pair.Value);
                }
            }
            foreach (Gridlock gridLock in upForDelete2)
            {
                RemoveLock(gridLock.Person, gridLock.LocalDate);
            }
        }

        public void ClearWriteProtected()
        {
            IList<GridlockDictionary> upForDelete = new List<GridlockDictionary>();

            foreach (KeyValuePair<string, GridlockDictionary> kvp in _gridlocks)
            {
                upForDelete.Add(kvp.Value);
            }
            IList<Gridlock> upForDelete2 = new List<Gridlock>();
            foreach (GridlockDictionary dictionary in upForDelete)
            {
                foreach (KeyValuePair<string, Gridlock> pair in dictionary)
                {
                    upForDelete2.Add(pair.Value);
                }
            }
            foreach (Gridlock gridLock in upForDelete2)
            {
                RemoveLock(gridLock.Person, gridLock.LocalDate, LockType.WriteProtected);
            }
        }

	    public IEnumerable<LockInfo> LockInfos()
	    {
			foreach (var gridLockDictionary in GridlocksDictionary.Values)
			{
				foreach (var gridLock in gridLockDictionary)
				{
					yield return new LockInfo { Date = gridLock.Value.LocalDate, AgentId = gridLock.Value.Person.Id.Value, LockType = gridLock.Value.LockType};
				}
			}
		}

	    public static string GetPersonDateKey(IPerson person, DateOnly dateOnly)
        {
			return person.GetHashCode().ToString(CultureInfo.InvariantCulture) + "|" +  dateOnly.GetHashCode().ToString(CultureInfo.InvariantCulture);
        }
    }
}
