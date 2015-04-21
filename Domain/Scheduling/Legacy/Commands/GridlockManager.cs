using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
    public interface IGridlockManager
    {
        /// <summary>
        /// Return dictionary with locks
        /// </summary>
        IDictionary<string, GridlockDictionary> GridlocksDictionary { get; }

        /// <summary>
        /// true if locks exists, false if not
        /// </summary>
        /// <returns></returns>
        bool HasLocks { get; }

        /// <summary>
        /// Add lock on person + date
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="dateOnly">The local date.</param>
        /// <param name="lockType">Type of the lock.</param>
        /// <param name="period">The period.</param>
        void AddLock(IPerson person, DateOnly dateOnly, LockType lockType, DateTimePeriod period);

        /// <summary>
        /// Add lock on schedule
        /// </summary>
        /// <param name="schedulePart">The schedule part.</param>
        /// <param name="lockType">Type of the lock.</param>
        void AddLock(IScheduleDay schedulePart, LockType lockType);

        /// <summary>
        /// Add lock on a list with schedules
        /// </summary>
        /// <param name="schedules">The schedules.</param>
        /// <param name="lockType">Type of the lock.</param>
        void AddLock(IList<IScheduleDay> schedules, LockType lockType);

        /// <summary>
        /// Remove lock on person + date
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="dateOnly">The date only.</param>
        void RemoveLock(IPerson person, DateOnly dateOnly);

        /// <summary>
        /// Remove lock on person + date
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="dateOnly">The date only.</param>
        /// <param name="lockType">Type of the lock.</param>
        void RemoveLock(IPerson person, DateOnly dateOnly, LockType lockType);

        /// <summary>
        /// Remove lock on schedule
        /// </summary>
        /// <param name="schedulePart"></param>
		void RemoveLock(IScheduleDay schedulePart);

        /// <summary>
        /// Remove lock on list of schedules
        /// </summary>
        /// <param name="schedules"></param>
        void RemoveLock(IList<IScheduleDay> schedules);

        /// <summary>
        /// Return lock for key
        /// </summary>
        /// <param name="person"></param>
        /// <param name="dateOnly"></param>
        /// <returns></returns>
        GridlockDictionary Gridlocks(IPerson person, DateOnly dateOnly);

        /// <summary>
        /// Return unlocked days
        /// </summary>
        /// <param name="scheduleDays"></param>
        /// <returns></returns>
        IList<IScheduleDay> UnlockedDays(IList<IScheduleDay> scheduleDays);

        /// <summary>
        /// Return lock for schedule
        /// </summary>
        /// <param name="schedulePart"></param>
        /// <returns></returns>
		GridlockDictionary Gridlocks(IScheduleDay schedulePart);

        /// <summary>
        /// Clear all locks
        /// </summary>
        void Clear();

        void ClearWriteProtected();
    }

    /// <summary>
    /// Manages grid locks
    /// </summary>
    public class GridlockManager : IGridlockManager
    {
        //public event EventHandler<ModifyEventArgs> LockModified;

        private readonly IDictionary<string, GridlockDictionary> _gridlocks;

        //private void OnLockModified(ModifyEventArgs e)
        //{
        //    if (LockModified != null)
        //    {
        //        LockModified(this, e);
        //    }
        //}

        /// <summary>
        /// Constructor
        /// </summary>
        public GridlockManager()
        {
            _gridlocks = new Dictionary<string, GridlockDictionary>();
        }

        /// <summary>
        /// Return dictionary with locks
        /// </summary>
        public IDictionary<string, GridlockDictionary> GridlocksDictionary
        {
            get { return _gridlocks; }
        }

        /// <summary>
        /// Add lock on person + date
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="dateOnly">The local date.</param>
        /// <param name="lockType">Type of the lock.</param>
        /// <param name="period">The period.</param>
        public void AddLock(IPerson person, DateOnly dateOnly, LockType lockType, DateTimePeriod period)
        {
            string key = GetPersonDateKey(person, dateOnly);
            
            if (!_gridlocks.ContainsKey(key))
            {
                _gridlocks.Add(key,new GridlockDictionary());
            }
            GridlockDictionary personDateLocks = _gridlocks[key];
            Gridlock gridlock = new Gridlock(person, dateOnly, lockType, period);
            if (!personDateLocks.ContainsKey(gridlock.Key))
            {
                personDateLocks.Add(gridlock.Key, gridlock);
                //OnLockModified(new ModifyEventArgs(ScheduleModifier.Scheduler, person, period));
            }
        }

        /// <summary>
        /// Add lock on schedule
        /// </summary>
        /// <param name="schedulePart">The schedule part.</param>
        /// <param name="lockType">Type of the lock.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void AddLock(IScheduleDay schedulePart, LockType lockType)
        {
            DateOnly dateOnly = schedulePart.DateOnlyAsPeriod.DateOnly;
            AddLock(schedulePart.Person, dateOnly, lockType, schedulePart.Period);
        }

        /// <summary>
        /// Add lock on a list with schedules
        /// </summary>
        /// <param name="schedules">The schedules.</param>
        /// <param name="lockType">Type of the lock.</param>
        public void AddLock(IList<IScheduleDay> schedules, LockType lockType)
        {
            foreach (IScheduleDay schedule in schedules)
            {
                AddLock(schedule, lockType);
            }
        }

        /// <summary>
        /// Remove lock on person + date
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="dateOnly">The date only.</param>
        /// <param name="period">The period.</param>
        public void RemoveLock(IPerson person, DateOnly dateOnly)//, DateTimePeriod period)
        {
            RemoveLock(person, dateOnly, LockType.Normal);//, period);
        }
        /// <summary>
        /// Remove lock on person + date
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="dateOnly">The date only.</param>
        /// <param name="lockType">Type of the lock.</param>
        /// <param name="period">The period.</param>
        public void RemoveLock(IPerson person, DateOnly dateOnly, LockType lockType)//, DateTimePeriod period)
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
                    }
                }
                if (!String.IsNullOrEmpty(keyToRemove))
                {
                    gridlocks.Remove(keyToRemove);
                    //DateTimePeriod period = new DateTimePeriod(dateOnly, dateOnly.AddDays(1));
                    //OnLockModified(new ModifyEventArgs(ScheduleModifier.Scheduler, person, period)); 
                }
                if (gridlocks.Count == 0)
                    GridlocksDictionary.Remove(GetPersonDateKey(person, dateOnly));
            }
        }

        /// <summary>
        /// Remove lock on schedule
        /// </summary>
        /// <param name="schedulePart"></param>
				public void RemoveLock(IScheduleDay schedulePart)
        {
            RemoveLock(schedulePart.Person, new DateOnly(schedulePart.Period.StartDateTimeLocal(schedulePart.TimeZone)));//, schedulePart.Period);
        }

        /// <summary>
        /// Remove lock on list of schedules
        /// </summary>
        /// <param name="schedules"></param>
        public void RemoveLock(IList<IScheduleDay> schedules)
        {
            foreach (IScheduleDay schedule in schedules)
            {
                RemoveLock(schedule);
            }
        }

        /// <summary>
        /// true if locks exists, false if not
        /// </summary>
        /// <returns></returns>
        public bool HasLocks
        {
            get {
                return _gridlocks.Count > 0;
            }
        }

        /// <summary>
        /// Return lock for key
        /// </summary>
        /// <param name="person"></param>
        /// <param name="dateOnly"></param>
        /// <returns></returns>
        public GridlockDictionary Gridlocks(IPerson person, DateOnly dateOnly)
        {
            string dummyKey = GetPersonDateKey(person, dateOnly);

            if (_gridlocks.ContainsKey(dummyKey))
                return _gridlocks[dummyKey];
            return null;
        }

        /// <summary>
        /// Return lock for schedule
        /// </summary>
        /// <param name="schedulePart"></param>
        /// <returns></returns>
				public GridlockDictionary Gridlocks(IScheduleDay schedulePart)
        {
            return Gridlocks(schedulePart.Person, new DateOnly(schedulePart.Period.StartDateTimeLocal(schedulePart.TimeZone)));
        }

        public IList<IScheduleDay> UnlockedDays(IList<IScheduleDay> scheduleDays)
        {
            return scheduleDays.Where(scheduleDay => Gridlocks(scheduleDay.Person, scheduleDay.DateOnlyAsPeriod.DateOnly) == null).ToList();
        }

        /// <summary>
        /// Clear all locks
        /// </summary>
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
                RemoveLock(gridLock.Person, gridLock.LocalDate);//, gridLock.Period);
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
                RemoveLock(gridLock.Person, gridLock.LocalDate, LockType.WriteProtected);//, gridLock.Period);
            }
        }

        public static string GetPersonDateKey(IPerson person, DateOnly dateOnly)
        {
	        return (person.GetHashCode() ^ dateOnly.GetHashCode()).ToString(CultureInfo.InvariantCulture);
        }
    }
}
